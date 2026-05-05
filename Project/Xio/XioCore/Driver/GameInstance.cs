using ClapInfra.ClapModels.Components;
using XioCore.Infra.DSL;
using XioCore.Infra.Models.Components;
using XioCore.Infra.Models.Entities;
using XioCore.Infra.Models.Judgement;
using XioCore.Infra.Profession;

namespace XioCore.Driver
{
    public class DefaultSkillContext : ISkillContext
    {
        public int Rank { get; }
        public string SkillName { get; }
        public Body Self { get; }
        public DefaultSkillContext(int rank, string skillName, Body self)
        {
            Rank = rank;
            SkillName = skillName;
            Self = self;
        }
    }
    public class GameInstance
    {
        public Body Player { get; private set; }
        public Body Enemy { get; private set; }
        public Judger Judger { get; private set; }
        public GameHistory History { get; private set; }
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
            foreach (var pair in History.SkillHistory)
            {
                res.Declare(pair.Item1.SkillName, pair.Item1.Rank, pair.Item2.SkillName, pair.Item2.Rank);
            }
            return res;
        }
        public SkillDeclareResult TryDeclare(string skillName, int rank)
        {
            DefaultSkillContext context = new(rank, skillName, Player);
            return Player.Get<Skill>().TryDeclare(skillName, context);
        }
        public SkillDeclareResult ETryDeclare(string skillName, int rank)
        {
            DefaultSkillContext context = new(rank, skillName, Player);
            return Enemy.Get<Skill>().TryDeclare(skillName, context);
        }

        public void Declare(string skillName, int rank, string esn, int er)
        {
            var playerContext = new DefaultSkillContext(rank, skillName, Player);
            var enemyContext = new DefaultSkillContext(er, esn, Enemy);

            History.SkillHistory.Add((playerContext, enemyContext));

            var psfs = new List<IDSLSourceFile>() { Player.Get<Skill>().Declare(skillName, playerContext) };
            var esfs = new List<IDSLSourceFile>() { Enemy.Get<Skill>().Declare(esn, enemyContext) };

            Judger.Judge(psfs, esfs);

        }
    }
}
