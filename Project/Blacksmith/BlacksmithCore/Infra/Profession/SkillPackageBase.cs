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
        public override IDSLSourceFile PassiveSkill(ISkillContext sc)
        {
            return new DSL.SourceFile(sc.Self);
        }
    }
}
