using BlacksmithCore.AI;
using BlacksmithCore.Driver;
using BlacksmithCore.Infra.Models.Components;

namespace AIPVPPlatform
{
    internal class AIPVP
    {
        private IAIStrategy _s1;
        private IAIStrategy _s2;
        private static readonly int Times = 1000;

        public AIPVP(IAIStrategy s1, IAIStrategy s2)
        {
            _s1 = s1;
            _s2 = s2;
        }
        public float Start()
        {
            int s1winTimes = 0;
            
            for (int _ = 0; _ < Times; _++)
            {
                if(_ % 10 == 0)
                {
                    Console.WriteLine($"已完成{_ * 100.0f / Times}%");
                }
                var ins1 = new BackendStarter().StartBackend();
                var ins2 = new BackendStarter().StartBackend();
                _s1.Init(ins1);
                _s2.Init(ins2);
                while (true)
                {
                    var t1 = _s1.ChooseSkill();
                    var t2 = _s2.ChooseSkill();
                    ins1.Declare(t2.skillName, t2.param, t1.skillName, t1.param, t2.stringParam, t1.stringParam);
                    ins2.Declare(t1.skillName, t1.param, t2.skillName, t2.param, t1.stringParam, t2.stringParam);
                    if (ins1.Player.Focus.Get<Health>().IsKilled)
                    {
                        s1winTimes++;
                        break;
                    }
                    if (ins1.Enemy.Focus.Get<Health>().IsKilled)
                    {
                        break;
                    }
                }
            }
            return 100.0f * s1winTimes / Times;
        }
    }
}
