using BlacksmithCore.Infra.Attributes.BlacksmithEnum;
using BlacksmithCore.Infra.Models.Core;

namespace ModExamples.ProphetMod
{
    [IsBlacksmithEnumModifier]
    public static class ResourceExtension
    {
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Crystal(this ResourceType resourceType) => ResourceType.GetCEValue();
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue CrystalBall(this ResourceType resourceType) => ResourceType.GetCEValue();
    }
}
