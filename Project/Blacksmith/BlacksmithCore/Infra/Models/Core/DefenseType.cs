using BlacksmithCore.Infra.Attributes.BlacksmithEnum;
using BlacksmithCore.Infra.Enum;

namespace BlacksmithCore.Infra.Models.Core
{
    public class DefenseType : BlacksmithEnum<DefenseType>
    {
        [IsBlacksmithEnumMember(-16)]
        public DefenseType.CEValue PhysicalImmunity() => DefenseType.GetCEValue();
        [IsBlacksmithEnumMember(-16)]
        public DefenseType.CEValue MagicalImmunity() => DefenseType.GetCEValue();
        [IsBlacksmithEnumMember(0)]
        public CEValue RealReduction() => GetCEValue();
        [IsBlacksmithEnumMember(8)]
        public CEValue ThornReduction() => GetCEValue();
        [IsBlacksmithEnumMember(16)]
        public CEValue CommonReduction() => GetCEValue();
        [IsBlacksmithEnumMember(32)]
        public CEValue StoneShell() => GetCEValue();
        [IsBlacksmithEnumMember(64)]
        public CEValue RealArmor() => GetCEValue();
        [IsBlacksmithEnumMember(128)]
        public CEValue CommonArmor() => GetCEValue();
    }
}