using BlacksmithCore.Infra.Attributes;
using BlacksmithCore.Infra.Enum;

namespace BlacksmithCore.Infra.Models.Core
{
    public class ResourceType : BlacksmithEnum<ResourceType>
    {
        [IsBlacksmithEnumMember(0)]
        public CEValue Iron() => GetBEValue();
        [IsBlacksmithEnumMember(1)]
        public CEValue Gold_Iron() => GetBEValue();
        [IsBlacksmithEnumMember(2)]
        public CEValue Space() => GetBEValue();
        [IsBlacksmithEnumMember(3)]
        public CEValue Time() => GetBEValue();
        [IsBlacksmithEnumMember(4)]
        public CEValue Magic() => GetBEValue();
    }
}
