using BlacksmithCore.Infra.Attributes;
using BlacksmithCore.Infra.Enum;

namespace BlacksmithCore.Infra.Models.Judgement.Core
{
    public class DynamicJudgeRuleName : BlacksmithEnum<DynamicJudgeRuleName>
    {
        [IsBlacksmithEnumMember(0)]
        public CEValue Reflect() => GetBEValue();
        [IsBlacksmithEnumMember(1)]
        public CEValue Charge() => GetBEValue();
    }
}
