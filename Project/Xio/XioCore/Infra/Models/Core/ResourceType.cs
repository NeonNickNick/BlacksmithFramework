using XioCore.Infra.Attributes;
using XioCore.Infra.Enum;

namespace XioCore.Infra.Models.Core
{
    public class ResourceType : XioEnum<ResourceType>
    {
        [IsXioEnumMember(0)]
        public CEValue Xio() => GetBEValue();
        [IsXioEnumMember(0)]
        public CEValue IceShield() => GetBEValue();
        [IsXioEnumMember(0)]
        public CEValue Light() => GetBEValue();
    }
}
