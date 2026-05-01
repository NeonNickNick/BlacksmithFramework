using BlacksmithCore.Infra.Models.Components.Resolutions;
using ClapInfra.ClapModels.Components;

namespace BlacksmithCore.Infra.Models.Components
{
    public interface IResolution : IClapResolution<Community>
    {
        public int DelayRounds { get; set; }
        public float Power { get; set; }
    }
    public class TurnContext : ClapTurnContext<IResolution, Community>
    {
        public TurnContext() : base(new()
        {
            typeof(AttackResolution),
            typeof(DefenseResolution),
            typeof(ResourceResolution),
            typeof(EffectResolution)
        })
        {
        }
        protected override void ExecuteImpl<TResolution>(Community community, List<TResolution> list)
        {
            var resolutions = list.Where(d => d.DelayRounds == 0).ToList();
            foreach (var temp in resolutions)
            {
                temp.Execute(community);
            }
            list.RemoveAll(d => resolutions.Contains(d));
            list.ForEach(d => d.DelayRounds--);
        }
        public List<(string name, int delayRounds, int power)> GetFutureDefenseView()
        {
            return Get<DefenseResolution>()
                .Select(d => (d.Defense.GetType().Name, d.DelayRounds, d.Defense.Power))
                .ToList();
        }
        public List<(string name, int delayRounds, int power)> GetFutureAttackView()
        {
            return Get<AttackResolution>()
                .Select(a => (a.Type.ToString(), a.DelayRounds, (int)a.Power))
                .ToList();
        }
    }
}