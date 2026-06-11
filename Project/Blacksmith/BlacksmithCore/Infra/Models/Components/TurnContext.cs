using BlacksmithCore.Infra.Models.Components.Resolutions;
using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapModels.Components;
using ClapInfra.ClapModels.Entities;
using ClapInfra.ClapUnit;

namespace BlacksmithCore.Infra.Models.Components
{
    public interface IResolution : IClapResolution<Community>
    {
        public ClapRoundClock Clock { get; set; }
        public float Power { get; set; }
    }
    public class TurnContext : ClapTurnContext<IResolution, Community>, IUpdatePerRound
    {
        private class Unit
        {
            public Action<IResolution> Action;
            public ClapRoundClock Clock;
            public Unit(Action<IResolution> action, ClapRoundClock clock)
            {
                Action = action;
                Clock = clock;
            }
        }
        private Dictionary<Type, List<Unit>> _preprocesses = new();
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
                _preprocesses[key] = new();
            }
        }
        public override void Reset()
        {
            foreach (var key in _resolutionLists.Keys)
            {
                _preprocesses[key].Clear();
            }
            base.Reset();
        }
        public void AddPreprocess<TResolution>(
            Action<TResolution> preprocess,
            int delayRounds = 0,
            int remainingRounds = 1,
            bool isInfinite = false)
            where TResolution : IResolution
        {
            var temp = (IResolution resolution) =>
            {
                preprocess((TResolution)resolution);
            };
            _preprocesses[typeof(TResolution)].Add(new(temp, new(remainingRounds: remainingRounds, delayRounds: delayRounds, isInfinite: isInfinite)));
        }
        public void Update()
        {
            foreach (var list in _preprocesses.Values)
            {
                int n = list.Count;
                for (int i = n - 1; i >= 0; i--)
                {
                    if (list[i].Clock.IsDead)
                    {
                        list.RemoveAt(i);
                        continue;
                    }
                    list[i].Clock.RoundPass();
                }
            }
        }
        public override void WriteResolution(IResolution resolution)
        {
            var pp = _preprocesses[resolution.GetType()];
            pp.ForEach(p =>
            {
                if (p.Clock.IsRinging)
                {
                    p.Action(resolution);
                }
            });
            base.WriteResolution(resolution);
        }
        protected override void ExecuteImpl<TResolution>(Community community, List<TResolution> list, Func<TResolution, bool>? ifProcess)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Clock.IsRinging)
                {
                    list[i].Execute(community);
                    list.RemoveAt(i);
                    i--;
                }
                else
                {
                    list[i].Clock.RoundPass();
                }
            }
        }
        public List<(string name, int delayRounds, int power)> GetFutureDefenseView()
        {
            return Get<DefenseResolution>()
                .Select(d => (d.Defense.GetType().Name, d.Clock.DelayRounds, d.Defense.Power))
                .ToList();
        }
        public List<(string name, int delayRounds, int power)> GetFutureAttackView()
        {
            return Get<AttackResolution>()
                .Select(a => (a.Type.ToString(), a.Clock.DelayRounds, (int)a.Power))
                .ToList();
        }
    }
}