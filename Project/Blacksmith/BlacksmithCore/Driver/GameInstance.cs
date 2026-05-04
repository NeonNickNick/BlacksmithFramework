using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Models.Judgement;
using BlacksmithCore.Infra.Profession;

namespace BlacksmithCore.Driver
{
    public class DefaultSkillContext : ISkillContext
    {
        public string SkillName { get; }
        public Community Self { get; }
        public int Param { get; }
        public DefaultSkillContext(string skillName, Community self, int param)
        {
            SkillName = skillName;
            Self = self;
            Param = param;
        }
    }
    public class GameInstance
    {
        public Community Player { get; private set; }
        public Community Enemy { get; private set; }
        public Judger Judger { get; private set; }
        public GameHistory History { get; private set; }
        public void Swap()
        {
            (Player, Enemy) = (Enemy, Player);
            Judger.Swap();
            History.Swap();
        }
        public GameInstance()
        {
            Player = new();
            Enemy = new();
            Judger = new(Player, Enemy);
            History = new();
        }
        public GameInstance DeepCopy()
        {
            GameInstance res = new();
            foreach(var pair in History.SkillHistory)
            {
                res.Declare(pair.Item1.SkillName, pair.Item1.Param, pair.Item2.SkillName, pair.Item2.Param);
            }
            return res;
        }
        public SkillDeclareResult TryDeclare(string skillName, int param)
        {
            DefaultSkillContext context = new(skillName, Player, param);
            return Player.Focus.Get<Skill>().TryDeclare(skillName, context);
        }
        public SkillDeclareResult ETryDeclare(string skillName, int param)
        {
            DefaultSkillContext context = new(skillName, Enemy, param);
            return Enemy.Focus.Get<Skill>().TryDeclare(skillName, context);
        }
       
        public void Declare(string skillName, int param, string esn, int ep)
        {
            var playerContext = new DefaultSkillContext(skillName, Player, param);
            var enemyContext = new DefaultSkillContext(esn, Enemy, ep);

            History.SkillHistory.Add((playerContext, enemyContext));

            var psfs = new List<IDSLSourceFile>(){ Player.Focus.Get<Skill>().Declare(skillName, playerContext) }; 
            psfs.InsertRange(0, Player.Focus.Get<Skill>().GetPassiveSkill(playerContext));

            var esfs = new List<IDSLSourceFile>() { Enemy.Focus.Get<Skill>().Declare(esn, enemyContext) };
            esfs.InsertRange(0, Enemy.Focus.Get<Skill>().GetPassiveSkill(enemyContext));

            Judger.Judge(psfs, esfs);

        }
    }
}
