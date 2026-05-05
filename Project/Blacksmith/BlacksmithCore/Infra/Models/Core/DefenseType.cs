using BlacksmithCore.Infra.Attributes;
using BlacksmithCore.Infra.Enum;

namespace BlacksmithCore.Infra.Models.Core
{
    public class DefenseType : BlacksmithEnum<DefenseType>
    {
        [IsBlacksmithEnumMember(0)]
        public CEValue RealReduction() => GetBEValue();
        [IsBlacksmithEnumMember(8)]
        public CEValue ThornReduction() => GetBEValue();
        [IsBlacksmithEnumMember(16)]
        public CEValue CommonReduction() => GetBEValue();
        [IsBlacksmithEnumMember(32)]
        public CEValue RockArmor() => GetBEValue();
        [IsBlacksmithEnumMember(64)]
        public CEValue ReadlArmor() => GetBEValue();
        [IsBlacksmithEnumMember(128)]
        public CEValue CommonArmor() => GetBEValue();
    }
}