using XioCore.Infra.Attributes;
using XioCore.Infra.Enum;

namespace XioCore.Infra.Models.Core
{
    public class DefenseType : XioEnum<DefenseType>
    {
        [IsXioEnumMember(0)]
        public CEValue Common() => GetBEValue();
        [IsXioEnumMember(0)]
        public CEValue Ying() => GetBEValue();
        [IsXioEnumMember(0)]
        public CEValue Xie() => GetBEValue();
        [IsXioEnumMember(0)]
        public CEValue Heng() => GetBEValue();
        [IsXioEnumMember(0)]
        public CEValue Shu() => GetBEValue();
    }
}
