using BlacksmithCore.Infra.Attributes.BlacksmithEnum;
using BlacksmithCore.Infra.Enum;

namespace BlacksmithCore.Infra.Models.Judgement.Core
{
    public class DynamicJudgeRuleName : BlacksmithEnum<DynamicJudgeRuleName>
    {
        [IsBlacksmithEnumMember(0)]
        public CEValue Reflect() => GetCEValue();
        [IsBlacksmithEnumMember(1)]
        public CEValue Charge() => GetCEValue();
    }
}
