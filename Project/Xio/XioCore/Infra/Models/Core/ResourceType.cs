using XioCore.Infra.Attributes;
using XioCore.Infra.Enum;

namespace XioCore.Infra.Models.Core
{
    public class ResourceType : XioEnum<ResourceType>
    {
        [IsXioEnumMember(0)]
        public CEValue Xio() => GetCEValue();
        [IsXioEnumMember(0)]
        public CEValue IceShield() => GetCEValue();
        [IsXioEnumMember(0)]
        public CEValue Light() => GetCEValue();
    }
}
