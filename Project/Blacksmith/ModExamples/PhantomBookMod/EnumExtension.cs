using BlacksmithCore.Infra.Attributes.BlacksmithEnum;
using BlacksmithCore.Infra.Models.Core;

namespace ModExamples.PhantomBookMod
{
    [IsBlacksmithEnumModifier]
    public static class ResourceExtension
    {
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Dream(this ResourceType resourceType) => ResourceType.GetCEValue();
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Spirit(this ResourceType resourceType) => ResourceType.GetCEValue();
    }
}
