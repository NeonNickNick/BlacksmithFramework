using ClapInfra.ClapEnum;

namespace XioCore.Infra.Attributes
{
    [AttributeUsage(AttributeTargets.Method,
        AllowMultiple = false, Inherited = false)]
    public class IsXioEnumMember : Attribute, IIsClapEnumMember
    {
        public int Priority { get; }
        public IsXioEnumMember(int priority)
        {
            Priority = priority;
        }
    }
}
