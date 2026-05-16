using BlacksmithCore.Infra.Attributes.BlacksmithEnum;
using BlacksmithCore.Infra.Enum;

namespace BlacksmithCore.Infra.Models.Core
{
    public class ResourceType : BlacksmithEnum<ResourceType>
    {
        [IsBlacksmithEnumMember(0)]
        public CEValue Iron() => GetCEValue();
        [IsBlacksmithEnumMember(1)]
        public CEValue Gold_Iron() => GetCEValue();
        [IsBlacksmithEnumMember(2)]
        public CEValue Space() => GetCEValue();
        [IsBlacksmithEnumMember(3)]
        public CEValue Time() => GetCEValue();
        [IsBlacksmithEnumMember(4)]
        public CEValue Magic() => GetCEValue();
    }
}
