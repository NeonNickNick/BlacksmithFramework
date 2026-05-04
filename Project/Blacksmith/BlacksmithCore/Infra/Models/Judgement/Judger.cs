using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Models.Judgement.Core;
using ClapInfra.ClapJudgement;
using ClapInfra.ClapJudgement.Core;

namespace BlacksmithCore.Infra.Models.Judgement
{
    using DSL = DSLforSkillLogic;
    public class Judger : ClapJudger<Community, Judger, JudgeRuleManager, Intent, IDSLSourceFile>
    {
        public void Swap()
        {
            (Player, Enemy) = (Enemy, Player);
        }
        public Judger(Community player, Community enemy) : base(player, enemy)
        {

        }
        protected override List<Intent> Compile(List<IDSLSourceFile> sourceFiles)
        {
            var passive = sourceFiles[0];
            sourceFiles.RemoveAt(0);
            var skillIntents = sourceFiles.Select(s => s.Compile(this)).ToList();
            skillIntents.Insert(0, passive.Compile(this));
            return skillIntents;
        }
    }
}
