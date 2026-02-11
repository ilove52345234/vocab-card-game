using System;

namespace VocabCardGame.StudyRoom
{
    /// <summary>
    /// 書房設定（資料驅動）
    /// </summary>
    [Serializable]
    public class StudyRoomConfig
    {
        public int previewCost = 8;
        public int previewOptionCount = 3;
        public int stashCost = 5;
        public int evolveCost = 6;
        public int deepenCost = 6;
        public int noteCost = 2;
    }
}
