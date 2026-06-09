using BlacksmithCore.Infra.DSL;
using ClapInfra.ClapProfession;

namespace BlacksmithCore.Infra.Profession
{
    using DSL = DSLforSkillLogic;
    public abstract class SkillPackageBase
        : ClapSkillPackage<ISkillContext, IDSLSourceFile>
    {
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
