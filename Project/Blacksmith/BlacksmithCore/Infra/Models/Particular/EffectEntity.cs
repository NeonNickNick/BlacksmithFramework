using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;

namespace BlacksmithCore.Infra.Models.Particular
{
    public class EffectEntity
    {
        public readonly EffectType.CEValue Type;
        public int DelayTimes { get; set; } = 0;
        public int RemainingTimes { get; set; }
        public float Power { get; set; }
        public Action<Body> Execute { get; set; } = null!;
        public EffectEntity(EffectType.CEValue type, int remainingTimes, float power)
        {
            Type = type;
            RemainingTimes = remainingTimes;
            Power = power;
        }
    }
}