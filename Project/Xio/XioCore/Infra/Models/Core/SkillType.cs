using XioCore.Infra.Attributes;
using XioCore.Infra.Enum;

namespace XioCore.Infra.Models.Core
{
    public class SkillType : XioEnum<SkillType>
    {
        [IsXioEnumMember(0)]
        public CEValue Attack() => GetBEValue();
        [IsXioEnumMember(0)]
        public CEValue Defense() => GetBEValue();
        [IsXioEnumMember(0)]
        public CEValue Resource() => GetBEValue();
        [IsXioEnumMember(0)]
        public CEValue Xiaoxiao() => GetBEValue();
        [IsXioEnumMember(0)]
        public CEValue Tan() => GetBEValue();
        [IsXioEnumMember(0)]
        public CEValue Taichi() => GetBEValue();
        [IsXioEnumMember(0)]
        public CEValue Shengji() => GetBEValue();
        [IsXioEnumMember(0)]
        public CEValue Zige() => GetBEValue();
    }
}
