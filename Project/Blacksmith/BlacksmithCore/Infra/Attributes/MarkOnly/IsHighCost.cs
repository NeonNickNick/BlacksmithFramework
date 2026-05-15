namespace BlacksmithCore.Infra.Attributes.MarkOnly
{
    [AttributeUsage(AttributeTargets.Method,
        AllowMultiple = false, Inherited = false)]
    public class IsHighCost : Attribute
    {
        public string Description { get; }
        public IsHighCost(string description = "")
        {
            Description = description;
        }
    }
}