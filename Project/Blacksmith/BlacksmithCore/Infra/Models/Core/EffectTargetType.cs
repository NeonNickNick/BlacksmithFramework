using BlacksmithCore.Infra.Attributes;
using BlacksmithCore.Infra.Enum;

namespace BlacksmithCore.Infra.Models.Core
{
    public class EffectTargetType : BlacksmithEnum<EffectTargetType>
    {
        [IsBlacksmithEnumMember(0)]
        public CEValue Self() => GetBEValue();
        [IsBlacksmithEnumMember(8)]
        public CEValue Enemy() => GetBEValue();
    }
}