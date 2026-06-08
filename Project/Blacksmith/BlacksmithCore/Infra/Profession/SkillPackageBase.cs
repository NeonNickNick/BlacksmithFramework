using BlacksmithCore.Infra.DSL;
using ClapInfra.ClapProfession;

namespace BlacksmithCore.Infra.Profession
{
    using DSL = DSLforSkillLogic;
    public abstract class SkillPackageBase
        : ClapSkillPackage<ISkillContext, IDSLSourceFile>
    {
        protected override void AddModOnInit() => ProfessionRegistry.AddModOnInit(this);
        protected SkillPackageBase(PackageType packageType) : base(packageType)
        {
        }
        public sealed override IDSLSourceFile PassiveSkill(ISkillContext sc)
        {
            var sf = PassiveSkillImpl(sc);
            sf.IsPassive = true;
            return sf;
        }
        public virtual IDSLSourceFile PassiveSkillImpl(ISkillContext sc)
        {
            return new DSL.SourceFile(sc.Self);
        }
    }
}
