using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VocabCardGame.Data;

namespace VocabCardGame.Map
{
    /// <summary>
    /// 地圖生成器
    /// </summary>
    public class MapGenerator
    {
        private readonly MapConfig config;
        private readonly System.Random random = new System.Random();

        public MapGenerator(MapConfig config)
        {
            this.config = config ?? new MapConfig();
        }

        public MapGraph Generate()
        {
            var roomPlan = GenerateRoomPlan();
            return BuildGraph(roomPlan);
        }

        private RoomType[] GenerateRoomPlan()
        {
            int steps = Mathf.Max(1, config.steps);
            var plan = new RoomType[steps + 1];
            var baseCounts = BuildRoomCountMap();

            int attempts = 0;
            while (attempts < 500)
            {
                Array.Clear(plan, 0, plan.Length);

                var counts = BuildRoomCountMap(baseCounts);
                ApplyFixedRooms(plan, counts);

                var eliteSteps = new List<int>();
                var restSteps = new List<int>();
                for (int step = 1; step <= steps; step++)
                {
                    if (plan[step] == RoomType.Elite) eliteSteps.Add(step);
                    if (plan[step] == RoomType.Rest) restSteps.Add(step);
                }

                if (TryFillPlan(plan, counts, eliteSteps, restSteps))
                {
                    return plan;
                }
                attempts++;
            }

            for (int step = 1; step <= steps; step++)
            {
                if (plan[step] == RoomType.None)
                {
                    plan[step] = RoomType.Enemy;
                }
            }

            return plan;
        }

        private Dictionary<RoomType, int> BuildRoomCountMap()
        {
            var counts = new Dictionary<RoomType, int>();
            foreach (var entry in config.roomCounts)
            {
                counts[entry.type] = entry.count;
            }
            return counts;
        }

        private Dictionary<RoomType, int> BuildRoomCountMap(Dictionary<RoomType, int> source)
        {
            var counts = new Dictionary<RoomType, int>();
            foreach (var kvp in source)
            {
                counts[kvp.Key] = kvp.Value;
            }
            return counts;
        }

        private void ApplyFixedRooms(RoomType[] plan, Dictionary<RoomType, int> counts)
        {
            foreach (var fixedRoom in config.fixedRooms)
            {
                if (fixedRoom.step <= 0 || fixedRoom.step >= plan.Length) continue;
                plan[fixedRoom.step] = fixedRoom.type;
                if (counts.ContainsKey(fixedRoom.type))
                {
                    counts[fixedRoom.type] = Mathf.Max(0, counts[fixedRoom.type] - 1);
                }
            }
        }

        private bool IsFixedStep(int step)
        {
            return config.fixedRooms.Any(r => r.step == step);
        }

        private bool TryFillPlan(RoomType[] plan, Dictionary<RoomType, int> counts, List<int> eliteSteps, List<int> restSteps)
        {
            for (int step = 1; step < plan.Length; step++)
            {
                if (plan[step] != RoomType.None) continue;

                var candidates = GetCandidateRoomTypes(step, plan, counts, eliteSteps, restSteps);
                if (candidates.Count == 0) return false;

                var picked = candidates[random.Next(candidates.Count)];
                plan[step] = picked;
                counts[picked]--;

                if (picked == RoomType.Elite) eliteSteps.Add(step);
                if (picked == RoomType.Rest) restSteps.Add(step);
            }

            return counts.Values.All(c => c == 0);
        }

        private List<RoomType> GetCandidateRoomTypes(int step, RoomType[] plan, Dictionary<RoomType, int> counts, List<int> eliteSteps, List<int> restSteps)
        {
            var result = new List<RoomType>();
            foreach (var kvp in counts)
            {
                if (kvp.Value <= 0) continue;
                var type = kvp.Key;
                if (!IsRoomAllowed(step, plan, type, eliteSteps, restSteps)) continue;
                result.Add(type);
            }
            return result;
        }

        private bool IsRoomAllowed(int step, RoomType[] plan, RoomType type, List<int> eliteSteps, List<int> restSteps)
        {
            var rules = config.rules ?? new MapRules();

            if (type == RoomType.Elite && step < rules.noEliteBeforeStep)
            {
                return false;
            }

            if (type == RoomType.Study && step < rules.studyNotBeforeStep)
            {
                return false;
            }

            if (type == RoomType.Elite && eliteSteps.Any(s => Mathf.Abs(s - step) <= rules.minEliteGap))
            {
                return false;
            }

            if (type == RoomType.Rest && restSteps.Any(s => Mathf.Abs(s - step) <= rules.minRestGap))
            {
                return false;
            }

            if (type == RoomType.Study && rules.studyNotAdjacentRest)
            {
                if (restSteps.Any(s => Mathf.Abs(s - step) == 1)) return false;
            }

            if (rules.noConsecutiveSameNonCombat)
            {
                var prev = plan[Mathf.Max(1, step - 1)];
                if (IsNonCombat(type) && IsNonCombat(prev) && prev == type)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsNonCombat(RoomType type)
        {
            if (type == RoomType.None) return false;
            return type != RoomType.Enemy && type != RoomType.Elite;
        }

        private MapGraph BuildGraph(RoomType[] plan)
        {
            var graph = new MapGraph
            {
                steps = config.steps,
                lanes = config.lanes
            };

            int id = 0;
            var nodesByStepLane = new MapNode[config.steps + 1, config.lanes];

            for (int step = 1; step <= config.steps; step++)
            {
                for (int lane = 0; lane < config.lanes; lane++)
                {
                    var node = new MapNode
                    {
                        id = id++,
                        step = step,
                        lane = lane,
                        roomType = plan[step]
                    };
                    nodesByStepLane[step, lane] = node;
                    graph.nodes.Add(node);
                }
            }

            for (int step = 1; step < config.steps; step++)
            {
                for (int lane = 0; lane < config.lanes; lane++)
                {
                    var node = nodesByStepLane[step, lane];
                    var nextLanes = new List<int> { lane };
                    float branchChance = config.rules != null ? config.rules.branchChance : 0.5f;
                    if (lane > 0 && random.NextDouble() < branchChance) nextLanes.Add(lane - 1);
                    if (lane < config.lanes - 1 && random.NextDouble() < branchChance) nextLanes.Add(lane + 1);

                    foreach (var nextLane in nextLanes.Distinct())
                    {
                        node.nextNodeIds.Add(nodesByStepLane[step + 1, nextLane].id);
                    }
                }
            }

            EnsureIncomingConnections(graph, nodesByStepLane);

            return graph;
        }

        private void EnsureIncomingConnections(MapGraph graph, MapNode[,] nodesByStepLane)
        {
            var incoming = new Dictionary<int, int>();
            foreach (var node in graph.nodes)
            {
                incoming[node.id] = 0;
            }
            foreach (var node in graph.nodes)
            {
                foreach (var next in node.nextNodeIds)
                {
                    incoming[next] = incoming[next] + 1;
                }
            }

            for (int step = 2; step <= config.steps; step++)
            {
                for (int lane = 0; lane < config.lanes; lane++)
                {
                    var node = nodesByStepLane[step, lane];
                    if (incoming[node.id] > 0) continue;

                    var prevLane = Mathf.Clamp(lane, 0, config.lanes - 1);
                    var prevNode = nodesByStepLane[step - 1, prevLane];
                    prevNode.nextNodeIds.Add(node.id);
                    incoming[node.id] = 1;
                }
            }
        }
    }
}
