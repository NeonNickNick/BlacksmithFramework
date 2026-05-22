using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapUnit;

namespace BlacksmithCore.Infra.Models.Components.Resolutions
{
    public class ResourceResolution : IResolution
    {
        public required ClapRoundClock Clock { get; set; }
        public ResourceType.CEValue Type { get; set; }
        public float Power { get; set; }
        public Action<Community> Execute { get; set; } = null!;
        public ResourceResolution() { }
        public ResourceResolution(ResourceType.CEValue type, float power, Action<Community> execute)
        {
            Type = type;
            Power = power;
            Execute = execute;
        }
    }
}
