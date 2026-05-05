using ClapInfra.ClapProfession;
using XioCore.Infra.DSL;

namespace XioCore.Infra.Profession
{
    using DSL = DSLforSkillLogic;
    public abstract class SkillPackageBase
        : ClapSkillPackage<ISkillContext, IDSLSourceFile>, ISkillPackage
    {
        protected override void AddModOnInit() => ProfessionRegistry.AddModOnInit(this);
        protected SkillPackageBase(PackageType packageType) : base(packageType)
        {
        }
        public override IDSLSourceFile PassiveSkill(ISkillContext sc)
        {
            return new DSL.SourceFile();
        }
    }
}