using BlacksmithCore.Infra.Models.Components.Resolutions;
using BlacksmithCore.Infra.Models.Entites;
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
        private Dictionary<Type, Action<IResolution>> _preprocesses = new();
        public TurnContext() : base(new()
        {
            typeof(AttackResolution),
            typeof(DefenseResolution),
            typeof(ResourceResolution),
            typeof(EffectResolution)
        })
        {
            foreach (var key in _resolutionLists.Keys)
            {
                _preprocesses[key] = _ => { };
            }
        }
        public void AddPreprocess<TResolution>(Action<TResolution> preprocess)
            where TResolution : IResolution
        {
            var temp = (IResolution resolution) =>
            {
                preprocess((TResolution)resolution);
            };
            _preprocesses[typeof(TResolution)] += temp;
        }
        public override void WriteResolution(IResolution resolution)
        {
            var pp = _preprocesses[resolution.GetType()];
            pp(resolution);
            base.WriteResolution(resolution);
        }
        protected override void ExecuteImpl<TResolution>(Community community, List<TResolution> list, Func<TResolution, bool>? ifProcess)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].DelayRounds == 0)
                {
                    list[i].Execute(community);
                    list.RemoveAt(i);
                }
                else
                {
                    list[i].DelayRounds--;
                }
            }
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