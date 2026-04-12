using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.FrontendBackendInterface;
using Blacksmith.Program;

namespace Blacksmith.FrontEndBackendInterface
{
    using DSL = Blacksmith.Backend.SkillPackages.Logic.DSLforSkillLogic;
    public class BackendEntry : ISkillChoose
    {
        public ActorSet Player { get; private set; }
        public ActorSet Enemy { get; private set; }
        public Judger Judger { get; private set; }
        public BackendEntry()
        {
            Player = new();
            Enemy = new();
            Judger = new(Player, Enemy);
        }
        public SkillDeclareResult TryDeclare(string skillName, int param)
        {
            DefaultSkillContext context = new(Player, param);
            return Player.Focus.Skill.TryDeclare(skillName, context);
        }
        private int temp = 1;
        public void Declare(string skillName, int param)
        {
            DSL.SourceFile playerFile = Player.Focus.Skill.Declare(skillName, new DefaultSkillContext(Player, param));
            DSL.SourceFile enemyFile;
            if (temp % 3 != 0)
            {
                enemyFile = Enemy.Focus.Skill.Declare("iron", new DefaultSkillContext(Enemy, 0));
            }
            else
            {
                enemyFile = Enemy.Focus.Skill.Declare("drill", new DefaultSkillContext(Enemy, 0));
            }
            temp++;
            Judger.Judge(playerFile, enemyFile);
        }
    }
}
