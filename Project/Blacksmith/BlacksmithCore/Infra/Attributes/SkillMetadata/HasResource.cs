using BlacksmithCore.Infra.Attributes.SkillMetadata.Core;

namespace BlacksmithCore.Infra.Attributes.SkillMetadata
{
    [AttributeUsage(AttributeTargets.Method,
        AllowMultiple = true, Inherited = false)]
    public class HasResource : Attribute, ISkillMetadata
    {
    }
}
