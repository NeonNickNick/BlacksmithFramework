using BlacksmithCore.Infra.Attributes.BlacksmithEnum;
using BlacksmithCore.Infra.Models.Core;

namespace ModExamples.HolyBookMod
{
    [IsBlacksmithEnumModifier]
    public static class ResourceExtension
    {
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Cross(this ResourceType resourceType) => ResourceType.GetCEValue();
    }
    [IsBlacksmithEnumModifier]
    public static class DefenseExtension
    {
        [IsBlacksmithEnumMember(-8)]
        public static DefenseType.CEValue PercentageReduction(this DefenseType defenseType) => DefenseType.GetCEValue();
        [IsBlacksmithEnumMember(32768)]
        public static DefenseType.CEValue GreyHP(this DefenseType defenseType) => DefenseType.GetCEValue();
    }
}
