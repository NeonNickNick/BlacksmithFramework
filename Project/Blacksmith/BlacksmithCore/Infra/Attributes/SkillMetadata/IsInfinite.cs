using BlacksmithCore.Infra.Attributes.SkillMetadata.Core;

namespace BlacksmithCore.Infra.Attributes.SkillMetadata
{
    [AttributeUsage(AttributeTargets.Method,
        AllowMultiple = false, Inherited = false)]
    public class IsInfinite : Attribute, ISkillMetadata
    {
    }
}
