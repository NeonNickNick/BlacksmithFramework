using ClapInfra.ClapModels.Entities;
using XioCore.Infra.Models.Core;

namespace XioCore.Infra.Models.Components
{
    public class Reduction
    {
        public DefenseType.CEValue DefenseType { get; private set; }
        public float Power { get; } = 0;
        public float Work(float attack)
        {
            return MathF.Max(0, attack - Power);
        }
        public Reduction(DefenseType.CEValue defenseType, float power = 0)
        {
            DefenseType = defenseType;
            Power = power;
        }
    }
    public class Defense : IUpdatePerRound
    {
        private List<Reduction> _defenses = new();
        public List<Reduction> Defenses => _defenses;
        public void Add(Reduction reduction)
        {
            _defenses.Add(reduction);
        }
        public void Update()
        {
            _defenses.Clear();
        }
    }
}
