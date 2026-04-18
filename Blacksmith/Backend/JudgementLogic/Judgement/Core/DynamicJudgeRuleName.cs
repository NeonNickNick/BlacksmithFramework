using Blacksmith.Infra.Attributes;
using Blacksmith.Infra.ExtensibleEnum;

namespace Blacksmith.Backend.JudgementLogic.Judgement.Core
{
    public class DynamicJudgeRuleName : BlacksmithEnum<DynamicJudgeRuleName>
    {
        [IsBlacksmithEnumMember(0)]
        public BEValue Reflect() => GetBEValue();
        [IsBlacksmithEnumMember(1)]
        public BEValue Charge() => GetBEValue();
    }
}
