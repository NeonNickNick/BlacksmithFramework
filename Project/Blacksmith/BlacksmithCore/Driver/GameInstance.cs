using BlacksmithCore.Infra.Judgement;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Profession;
using ClapInfra.ClapModels.Components;

namespace BlacksmithCore.Driver
{
    public class DefaultSkillContext : ISkillContext
    {
        public ISudoOperations SudoOperations { get; }
        public string SkillName { get; }
        public Community Self { get; }
        public int Param { get; }
        public string StringParam { get; }
        public DefaultSkillContext(ISudoOperations sudoOperations, string skillName, Community self, int param, string stringParam)
        {
            SudoOperations = sudoOperations;
            SkillName = skillName;
            Self = self;
            Param = param;
            StringParam = stringParam;
        }
    }
    public class GameInstance : ISudoOperations
    {
        public Community Player { get; private set; }
        public Community Enemy { get; private set; }
        public Judger Judger { get; private set; }
        public GameHistory History { get; private set; }
        public GameMetadata Metadata { get; private set; }
        public GameInstance()
        {
            Player = new();
            Enemy = new();
            Judger = new(Player, Enemy);
            History = new();
            Metadata = new();
        }
        public bool IsPlayer(Community community)
        {
            return community == Player;
        }
        public IReadOnlyList<(ISkillContext, ISkillContext)> SkillHistory => History.SkillHistory;
        public GameInstance DeepCopy(int preRounds = 0)
        {
            GameInstance res = new();

            int n = History.SkillHistory.Count - preRounds;
            if (n < 0)
            {
                throw new ArgumentException("PreRounds out of limit!");
            }
            for (int i = 0; i < n; ++i)
            {
                var pair = History.SkillHistory[i];
                res.Declare(pair.Item1.SkillName, pair.Item1.Param,
                            pair.Item2.SkillName, pair.Item2.Param,
                            pair.Item1.StringParam, pair.Item2.StringParam);
            }
            return res;
        }
        public IGameMetadata GameMetadata => Metadata;
        public SkillDeclareResult TryDeclare(string skillName, int param, string stringParam = "")
        {
            DefaultSkillContext context = new(this, skillName, Player, param, stringParam);
            return Player.Focus.Get<Skill>().TryDeclare(skillName, context);
        }
        public SkillDeclareResult ETryDeclare(string skillName, int param, string stringParam = "")
        {
            DefaultSkillContext context = new(this, skillName, Enemy, param, stringParam);
            return Enemy.Focus.Get<Skill>().TryDeclare(skillName, context);
        }

        public void Declare(string skillName, int param, string esn, int ep, string stringParam = "", string esp = "")
        {
            Metadata.UpdateCurrentSkill(skillName, esn);
            var playerContext = new DefaultSkillContext(this, skillName, Player, param, stringParam);
            var enemyContext = new DefaultSkillContext(this, esn, Enemy, ep, esp);

            var ps = Player.Focus.Get<Skill>();
            var psfs = ps.GetPassiveSkill(playerContext);
            foreach (var s in Player.SummonList)
            {
                psfs = psfs.Concat(s.Get<Skill>().GetPassiveSkill(playerContext));
            }
            psfs = psfs.Append(ps.Declare(skillName, playerContext));

            var es = Enemy.Focus.Get<Skill>();
            var esfs = es.GetPassiveSkill(enemyContext);
            foreach (var s in Enemy.SummonList)
            {
                esfs = esfs.Concat(s.Get<Skill>().GetPassiveSkill(playerContext));
            }
            esfs = esfs.Append(es.Declare(esn, enemyContext));

            Judger.Judge(psfs, esfs);
            History.SkillHistory.Add((playerContext, enemyContext));
        }
    }
}
