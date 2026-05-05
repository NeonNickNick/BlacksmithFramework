using XioCore.Infra.Models.Core;
using XioCore.Infra.Models.Entities;
namespace XioCore.Infra.Models.Components.Resolutions
{
    public class UniversalResolution : IResolution
    {
        public required SkillType.CEValue SkillType { get; init; }
        public float Power { get; set; }
        public required Action<Body> Execute { get; set; }
        public bool IsLyt { get; set; } = false;
    }
}
