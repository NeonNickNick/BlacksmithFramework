using BlacksmithCore.Infra.Attributes;
using BlacksmithCore.Infra.Enum;

namespace BlacksmithCore.Infra.Models.Core
{
    public class EffectType : BlacksmithEnum<EffectType>
    {
        [IsBlacksmithEnumMember(0)]
        public CEValue AfterResolutionWritten() => GetBEValue();
        [IsBlacksmithEnumMember(8)]
        public CEValue AfterTransport() => GetBEValue();
        [IsBlacksmithEnumMember(16)]
        public CEValue AfterResult() => GetBEValue();
    }
}