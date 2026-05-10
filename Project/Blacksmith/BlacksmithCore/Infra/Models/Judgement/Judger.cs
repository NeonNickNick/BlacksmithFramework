using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Models.Judgement.Core;
using ClapInfra.ClapJudgement;

namespace BlacksmithCore.Infra.Models.Judgement
{
    public class Judger : ClapJudger<Community, Judger, JudgeRuleManager, Intent, IDSLSourceFile>
    {
        public void Swap()
        {
            (Player, Enemy) = (Enemy, Player);
        }
        public Judger(Community player, Community enemy) : base(player, enemy)
        {

        }
        Action<Community> temp = (a) => { };
        protected override IEnumerable<Intent> Compile(List<IDSLSourceFile> sourceFiles)
        {
            var skillIntents = new List<Intent>() { new() { Execute = temp } };
            var passive = sourceFiles[0];
            int n = sourceFiles.Count;
            for (int i = 1; i < n; ++i)
            {
                skillIntents.Add(sourceFiles[i].Compile(this));
            }
            skillIntents[0] = passive.Compile(this);
            return skillIntents;
        }
    }
}
