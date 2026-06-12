using BlacksmithCore.Infra.Attributes.SkillMetadata.Core;

namespace BlacksmithCore.Infra.Attributes.SkillMetadata
{
    public enum Impression
    {
        Conservative,
        Robust,
        Aggressive
    }
    public enum Strength
    {
        Useless,
        Ordinary,
        Strong,
        Super
    }
    [AttributeUsage(AttributeTargets.Method,
        AllowMultiple = false, Inherited = false)]
    public class Labels : Attribute, ISkillMetadata
    {
        public readonly Impression Impression;
        public readonly Strength Strength;
        public Labels(Impression impression, Strength strength)
        {
            Impression = impression;
            Strength = strength;
        }
    }
}
