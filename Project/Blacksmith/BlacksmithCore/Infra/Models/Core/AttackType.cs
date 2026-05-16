using BlacksmithCore.Infra.Attributes.BlacksmithEnum;
using BlacksmithCore.Infra.Enum;

namespace BlacksmithCore.Infra.Models.Core
{
    public class AttackType : BlacksmithEnum<AttackType>
    {
        [IsBlacksmithEnumMember(256)]
        public CEValue Physical() => GetCEValue();
        [IsBlacksmithEnumMember(128)]
        public CEValue Magical() => GetCEValue();
        [IsBlacksmithEnumMember(0)]
        public CEValue Real() => GetCEValue();
    }
}