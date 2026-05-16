using BlacksmithCore.Infra.Attributes.BlacksmithEnum;
using BlacksmithCore.Infra.Enum;

namespace BlacksmithCore.Infra.Models.Core
{
    public class EffectType : BlacksmithEnum<EffectType>
    {
        [IsBlacksmithEnumMember(0)]
        public CEValue AfterResolutionWritten() => GetCEValue();
        [IsBlacksmithEnumMember(8)]
        public CEValue AfterTransport() => GetCEValue();
        [IsBlacksmithEnumMember(16)]
        public CEValue AfterResult() => GetCEValue();
    }
}