using XioCore.Infra.Models.Core;

namespace XioCore.Infra.Models.Components
{
    public class Level
    {
        public static readonly int CycleLength = 22;
        private int _level = 1;
        public int Rank => _level / CycleLength;
        public int InternalLevel => _level % CycleLength;
        public int KilledTimes { get; set; } = 0;
        public void Upgrade(int addition)
        {
            _level += addition;
            int cnt = RankType.Instance.RankDict.Count;
            if (_level > cnt * CycleLength)
            {
                _level = cnt * CycleLength;
            }
        }
    }
}
