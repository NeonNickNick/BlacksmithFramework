using BlacksmithCore.Infra.Attributes.BlacksmithEnum;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Judgement.Core;

namespace ModExamples.CrossBowMod
{
    [IsBlacksmithEnumModifier]
    public static class ResourceExtension
    {
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Bolt(this ResourceType resourceType) => ResourceType.GetCEValue();
    }

}
