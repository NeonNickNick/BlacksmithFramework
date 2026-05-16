using BlacksmithCore.Infra.Attributes.BlacksmithEnum;
using BlacksmithCore.Infra.Models.Core;

namespace ModExamples.PhantomBookMod
{
    [IsBlacksmithEnumModifier]
    public static class ResourceExtension
    {
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Dream(this ResourceType resourceType) => ResourceType.GetCEValue();
    }
    [IsBlacksmithEnumModifier]
    public static class DefenseExtension
    {
        [IsBlacksmithEnumMember(-16)]
        public static DefenseType.CEValue PhysicalImmunity(this DefenseType defenseType) => DefenseType.GetCEValue();
    }
}
