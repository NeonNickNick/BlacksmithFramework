using BlacksmithCore.Infra.Attributes.SkillMetadata.Core;
using BlacksmithCore.Infra.Models.Core;

namespace BlacksmithCore.Infra.Attributes.SkillMetadata
{
    [AttributeUsage(AttributeTargets.Method,
        AllowMultiple = true, Inherited = false)]
    public class HasAttack : Attribute, ISkillMetadata
    {
        public readonly int Power;
        private readonly string _name;
        public AttackType.CEValue Type => AttackType.EnumDict[_name];
        public HasAttack(int power, string name = nameof(AttackType.Instance.Physical))
        {
            Power = power;
            _name = name;
        }
    }
}
