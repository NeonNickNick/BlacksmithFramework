using BlacksmithCore.Infra.Models.Components;
using ClapInfra.ClapUnit;

namespace BlacksmithCore.Infra.Models.Entites
{
    public class Community
    {
        private class Unit
        {
            public Action Action;
            public ClapRoundClock Clock;
            public Unit(Action action, ClapRoundClock clock)
            {
                Action = action;
                Clock = clock;
            }
        }
        public Body Focus { get; set; }
        public List<Body> SummonList { get; private set; } = new();
        private List<Unit> _transforms = new();
        private Dictionary<Body, Action> _callbacks = new();
        public Community()
        {
            Focus = new(this, "Main");
        }
        public void Reset()
        {
            SummonList.Clear();
            _transforms.Clear();
            _callbacks.Clear();
            Focus.Reset();
        }
        public void AddTransform(
            Action action,
            int delayRounds = 0,
            int remainingRounds = 1,
            bool isInfinite = false)
        {
            _transforms.Add(new(action, new(delayRounds: delayRounds, remainingRounds: remainingRounds, isInfinite: isInfinite)));
        }
        public void AddCallbackKilled(Body body, Action callback)
        {
            _callbacks[body] = callback;
        }
        public void Update()
        {
            SummonList.RemoveAll(s =>
            {
                if (s.Get<Health>().IsKilled)
                {
                    if (_callbacks.TryGetValue(s, out var action))
                    {
                        action();
                    }
                    return true;
                }
                return false;
            });
            int n = _transforms.Count;
            for (int i = n - 1; i >= 0; i--)
            {
                if (_transforms[i].Clock.IsDead)
                {
                    _transforms.RemoveAt(i);
                    continue;
                }
                _transforms[i].Action();
                _transforms[i].Clock.RoundPass();
            }
            SummonList.RemoveAll(s =>
            {
                if (s.Get<Health>().IsKilled)
                {
                    if (_callbacks.TryGetValue(s, out var action))
                    {
                        action();
                    }
                    return true;
                }
                return false;
            });
            Focus.Update();
            foreach (var s in SummonList)
            {
                s.Update();
            }
        }
    }
}