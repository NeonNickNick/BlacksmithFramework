using BlacksmithCore.Infra.Attributes.BlacksmithEnum;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Judgement.Core;

namespace ModExamples.CauldronMod
{
    [IsBlacksmithEnumModifier]
    public static class ResourceExtension
    {
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Fire(this ResourceType resourceType) => ResourceType.GetCEValue();
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Water(this ResourceType resourceType) => ResourceType.GetCEValue();
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Wood(this ResourceType resourceType) => ResourceType.GetCEValue();
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Earth(this ResourceType resourceType) => ResourceType.GetCEValue();
    }
    [IsBlacksmithEnumModifier]
    public static class DynamicJudgeRuleNameExtension
    {
        [IsBlacksmithEnumMember(0)]
        public static DynamicJudgeRuleName.CEValue LifeBurn(this DynamicJudgeRuleName name) => DynamicJudgeRuleName.GetCEValue();
    }
}
