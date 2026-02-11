using System;
using System.Collections.Generic;
using UnityEngine;
using VocabCardGame.Data;

namespace VocabCardGame.Map
{
    /// <summary>
    /// 地圖設定（資料驅動）
    /// </summary>
    [Serializable]
    public class MapConfig
    {
        public int steps = 15;
        public int lanes = 3;
        public List<MapFixedRoom> fixedRooms = new List<MapFixedRoom>();
        public List<MapRoomCount> roomCounts = new List<MapRoomCount>();
        public MapRules rules = new MapRules();
    }

    /// <summary>
    /// 固定房間設定
    /// </summary>
    [Serializable]
    public class MapFixedRoom
    {
        public int step;
        public RoomType type;
    }

    /// <summary>
    /// 房間數量設定
    /// </summary>
    [Serializable]
    public class MapRoomCount
    {
        public RoomType type;
        public int count;
    }

    /// <summary>
    /// 地圖規則設定
    /// </summary>
    [Serializable]
    public class MapRules
    {
        public int noEliteBeforeStep = 5;
        public int minEliteGap = 3;
        public int minRestGap = 4;
        public int studyNotBeforeStep = 4;
        public bool studyNotAdjacentRest = true;
        public bool noConsecutiveSameNonCombat = true;
        public float branchChance = 0.5f;
    }

    /// <summary>
    /// 地圖節點
    /// </summary>
    [Serializable]
    public class MapNode
    {
        public int id;
        public int step;
        public int lane;
        public RoomType roomType;
        public List<int> nextNodeIds = new List<int>();
    }

    /// <summary>
    /// 地圖資料
    /// </summary>
    [Serializable]
    public class MapGraph
    {
        public int steps;
        public int lanes;
        public List<MapNode> nodes = new List<MapNode>();
    }
}
