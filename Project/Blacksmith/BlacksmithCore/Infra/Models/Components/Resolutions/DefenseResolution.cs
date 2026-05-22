using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapUnit;

namespace BlacksmithCore.Infra.Models.Components.Resolutions
{
    public class DefenseResolution : IResolution
    {
        public required ClapRoundClock Clock { get; set; }
        public DefenseBase Defense { get; set; } = null!;
        public float Power { get; set; }
        public Action<Community> Execute { get; set; } = (_) => { };
        public DefenseResolution() { }
        public DefenseResolution(DefenseBase defense, float power, Action<Community> execute)
        {
            Defense = defense;
            Power = power;
            Execute = execute;
        }
    }
}