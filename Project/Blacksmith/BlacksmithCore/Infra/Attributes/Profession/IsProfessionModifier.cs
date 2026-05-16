namespace BlacksmithCore.Infra.Attributes.Profession
{
    [AttributeUsage(AttributeTargets.Class,
        AllowMultiple = false, Inherited = false)]
    public class IsProfessionModifier : Attribute
    {
        public readonly string TargetName;
        public IsProfessionModifier(string targetName)
        {
            TargetName = targetName;
        }
    }
}

