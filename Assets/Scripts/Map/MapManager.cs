using UnityEngine;
using VocabCardGame.Core;

namespace VocabCardGame.Map
{
    /// <summary>
    /// 地圖管理器
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        public MapGraph CurrentMap { get; private set; }

        /// <summary>
        /// 生成新地圖
        /// </summary>
        public void GenerateNewMap()
        {
            var config = GameManager.Instance.dataManager.GetMapConfig();
            var generator = new MapGenerator(config);
            CurrentMap = generator.Generate();
        }
    }
}
