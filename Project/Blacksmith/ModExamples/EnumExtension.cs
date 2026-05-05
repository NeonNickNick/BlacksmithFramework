using BlacksmithCore.Infra.Attributes;
using BlacksmithCore.Infra.Models.Core;

namespace ModExamples
{
    [IsBlacksmithEnumModifier]
    public static class ResourceExtension
    {
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Cross(this ResourceType resourceType) => ResourceType.GetBEValue();
    }
    [IsBlacksmithEnumModifier]
    public static class DefenseExtension
    {
        [IsBlacksmithEnumMember(-8)]//百分比在最外面
        public static DefenseType.CEValue PercentageReduction(this DefenseType defenseType) => DefenseType.GetBEValue();
        [IsBlacksmithEnumMember(32768)]//灰血贴身给一个很大的数值
        public static DefenseType.CEValue GreyHP(this DefenseType defenseType) => DefenseType.GetBEValue();
    }
}
