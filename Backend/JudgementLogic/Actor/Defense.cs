using Blacksmith.Backend.JudgementLogic.Core;

namespace Blacksmith.Backend.JudgementLogic.Actor
{
    public class Defense
    {
        private readonly List<DefenseBase> _defenses = new();
        public List<DefenseBase> Defenses => _defenses;

        public void Update()
        {
            int n = _defenses.Count;
            for (int i = n - 1; i >= 0; i--)
            {
                _defenses[i].Update();
                var power = _defenses[i].Power;
                power = (int)power;
                _defenses[i].Power = power;
                if (_defenses[i].IsDead)
                {
                    _defenses.RemoveAt(i);
                }
            }
        }
        public void Add(DefenseBase addition)
        {
            if (Merge(addition))
            {
                return;
            }
            _defenses.Add(addition);
        }
        private bool Merge(DefenseBase addition)
        {
            if (!addition.CanMerge)
            {
                return false;
            }
            DefenseBase firstMatch = _defenses.Find(d => d.Name == addition.Name);
            if (firstMatch == null)
            {
                return false;
            }
            firstMatch.Merge(addition);
            return true;
        }
        public void Init()
        {
            _defenses.Clear();
        }
        public List<IDefenseWork> Get()
        {
            return _defenses.ConvertAll(d => (IDefenseWork)d);
        }
    }
}