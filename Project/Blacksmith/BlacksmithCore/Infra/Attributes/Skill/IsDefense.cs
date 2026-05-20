namespace BlacksmithCore.Infra.Attributes.Skill
{
    [AttributeUsage(AttributeTargets.Method,
        AllowMultiple = false, Inherited = false)]
    public class IsDefense : Attribute
    {
    }
}
