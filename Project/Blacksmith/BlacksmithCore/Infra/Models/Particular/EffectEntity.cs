using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapUnit;

namespace BlacksmithCore.Infra.Models.Particular
{
    public class EffectEntity
    {
        public readonly EffectType.CEValue Type;
        public ClapRoundClock Clock { get; }
        public float Power { get; set; }
        public Action<Body> Execute { get; set; } = null!;
        public EffectEntity(EffectType.CEValue type, float power, ClapRoundClock clock)
        {
            Type = type;
            Power = power;
            Clock = clock;
        }
    }
}