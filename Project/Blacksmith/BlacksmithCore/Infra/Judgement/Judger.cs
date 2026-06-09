using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapJudgement;

namespace BlacksmithCore.Infra.Judgement
{
    public class Judger : ClapJudger<Community, Judger, JudgeRuleManager, Intent, IDSLSourceFile>
    {
        public Judger(Community player, Community enemy) : base(player, enemy)
        {

        }
        protected override IEnumerable<Intent> Compile(IEnumerable<IDSLSourceFile> sourceFiles)
        {
            Intent temp = new() { Execute = null! };
            var skillIntents = new List<Intent>() { };
            var group = sourceFiles.ToLookup(s => s.IsPassive);
            foreach (var sf in group[true])
            {
                skillIntents.Add(temp);
            }
            foreach (var sf in group[false])
            {
                skillIntents.Add(sf.Compile(this));
            }
            var index = 0;
            foreach (var sf in group[true])
            {
                skillIntents[index] = sf.Compile(this);
                index++;
            }
            return skillIntents;
        }
    }
}
