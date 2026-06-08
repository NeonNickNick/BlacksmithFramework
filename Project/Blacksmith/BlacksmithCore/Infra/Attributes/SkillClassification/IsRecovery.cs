namespace BlacksmithCore.Infra.Attributes.SkillClassification
{
    [AttributeUsage(AttributeTargets.Method,
        AllowMultiple = false, Inherited = false)]
    public class IsRecovery : Attribute
    {
    }
}
