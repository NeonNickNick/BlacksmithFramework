namespace Blacksmith.Infra.Attributes
{
    [AttributeUsage(AttributeTargets.Method,
        AllowMultiple = false, Inherited = false)]
    public class IsBlacksmithEnumMember : Attribute
    {
        public readonly int Priority;
        public IsBlacksmithEnumMember(int priority)
        {
            Priority = priority;
        }
    }
}
