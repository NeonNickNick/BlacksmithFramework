using XioCore.Infra.Attributes;
using XioCore.Infra.Enum;

namespace XioCore.Infra.Models.Core
{
    public class DefenseType : XioEnum<DefenseType>
    {
        [IsXioEnumMember(0)]
        public CEValue Common() => GetCEValue();
        [IsXioEnumMember(0)]
        public CEValue Ying() => GetCEValue();
        [IsXioEnumMember(0)]
        public CEValue Xie() => GetCEValue();
        [IsXioEnumMember(0)]
        public CEValue Heng() => GetCEValue();
        [IsXioEnumMember(0)]
        public CEValue Shu() => GetCEValue();
    }
}
