using BlacksmithCore.Infra.Attributes.BlacksmithEnum;
using BlacksmithCore.Infra.Enum;

namespace BlacksmithCore.Infra.Models.Core
{
    public class EffectTargetType : BlacksmithEnum<EffectTargetType>
    {
        [IsBlacksmithEnumMember(0)]
        public CEValue Self() => GetCEValue();
        [IsBlacksmithEnumMember(8)]
        public CEValue Enemy() => GetCEValue();
    }
}