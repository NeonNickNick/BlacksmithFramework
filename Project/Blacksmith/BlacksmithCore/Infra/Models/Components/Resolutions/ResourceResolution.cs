using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;

namespace BlacksmithCore.Infra.Models.Components.Resolutions
{
    public class ResourceResolution : IResolution
    {
        public int DelayRounds { get; set; } = 0;
        public ResourceType.BEValue Type { get; set; }
        public float Power { get; set; }
        public Action<Community> Execute { get; set; } = null!;
        public ResourceResolution() { }
        public ResourceResolution(ResourceType.BEValue type, float power, Action<Community> execute)
        {
            Type = type;
            Power = power;
            Execute = execute;
        }
    }
}
