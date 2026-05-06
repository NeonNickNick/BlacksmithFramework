using XioCore.Infra.Attributes;
using XioCore.Infra.Enum;

namespace XioCore.Infra.Models.Core
{
    public class SkillType : XioEnum<SkillType>
    {
        [IsXioEnumMember(0)]
        public CEValue Attack() => GetCEValue();
        [IsXioEnumMember(0)]
        public CEValue Defense() => GetCEValue();
        [IsXioEnumMember(0)]
        public CEValue Resource() => GetCEValue();
        [IsXioEnumMember(0)]
        public CEValue Xiaoxiao() => GetCEValue();
        [IsXioEnumMember(0)]
        public CEValue Tan() => GetCEValue();
        [IsXioEnumMember(0)]
        public CEValue Taichi() => GetCEValue();
        [IsXioEnumMember(0)]
        public CEValue Shengji() => GetCEValue();
        [IsXioEnumMember(0)]
        public CEValue Zige() => GetCEValue();
    }
}
