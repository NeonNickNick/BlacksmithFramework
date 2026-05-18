using BlacksmithCore.Infra.Attributes.BlacksmithEnum;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Judgement.Core;

namespace ModExamples.CrossBowMod
{
    [IsBlacksmithEnumModifier]
    public static class ResourceExtension
    {
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Bolt(this ResourceType resourceType) => ResourceType.GetCEValue();
    }
    [IsBlacksmithEnumModifier]
    public static class DynamicJudgeRuleNameExtension
    {
        [IsBlacksmithEnumMember(0)]
        public static DynamicJudgeRuleName.CEValue MarkingBolt(this DynamicJudgeRuleName name) => DynamicJudgeRuleName.GetCEValue();
    }
}
