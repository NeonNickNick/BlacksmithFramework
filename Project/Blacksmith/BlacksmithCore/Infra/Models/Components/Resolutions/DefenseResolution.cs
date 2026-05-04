using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;

namespace BlacksmithCore.Infra.Models.Components.Resolutions
{
    public class DefenseResolution : IResolution
    {
        public int DelayRounds { get; set; } = 0;
        public DefenseBase Defense { get; set; }
        public float Power { get; set; }
        public Action<Community> Execute { get; set; }
        public DefenseResolution() { }
        public DefenseResolution(DefenseBase defense, float power, Action<Community> execute)
        {
            Defense = defense;
            Power = power;
            Execute = execute;
        }
    }
}