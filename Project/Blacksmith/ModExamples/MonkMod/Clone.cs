using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.Defense;
using ClapInfra.ClapUnit;

namespace ModExamples.MonkMod
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public partial class Clone : MainProfession
    {
        private ClapStateVar<int> _gbcTimes = new(0);
        public override IDSLSourceFile PassiveSkillImpl(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteFree(a => _gbcTimes.Increment(), true)
                .WriteDefense(100f - 60f * _gbcTimes.Value, new PercentageReduction(baseline: 100));
            return DSL.Create(sc.Self, pen);
        }
    }
}
