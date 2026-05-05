using ClapInfra.ClapJudgement;
using XioCore.Infra.DSL;
using XioCore.Infra.Models.Entities;
using XioCore.Infra.Models.Judgement.Core;

namespace XioCore.Infra.Models.Judgement
{
    public class Judger : ClapJudger<Body, Judger, JudgeRuleManager, Intent, IDSLSourceFile>
    {
        public Judger(Body player, Body enemy) : base(player, enemy)
        {
            player.manager = JudgeRuleManager;
            enemy.manager = JudgeRuleManager;
        }
    }
}
