using ClapInfra.ClapJudgement;
using XioCore.Infra.Models.Components;
using XioCore.Infra.Models.Components.Resolutions;
using XioCore.Infra.Models.Core;
using XioCore.Infra.Models.Entities;

namespace XioCore.Infra.Models.Judgement
{
    public class JudgeRuleManager : ClapJudgeRuleManager<Body>
    {
        private Action<Body, Body> _rule;
        public JudgeRuleManager()
        {
            _rule = (a, b) =>
            {
                Tan(a, b);
                Xiaoxiao(a, b);
                Zige(a, b);
                Resource(a, b);
                Defense(a, b);
                Taichi(a, b);
                CancelAttack(a, b);
                Attack(a, b);
                Shengji(a, b);
                End(a, b);
            };
        }
        public int Round { get; private set; } = 1;
        public int InnerRound { get; private set; } = 1;
        public override Action<Body, Body> GetRule() => _rule;
        private static void Default(Body player, Body enemy, SkillType.CEValue skillType)
        {
            if (player.Get<Level>().KilledTimes > 0 || enemy.Get<Level>().KilledTimes > 0)
            {
                return;
            }
            player.Get<TurnContext>().Execute<UniversalResolution>(player, r =>
            {
                return r.SkillType == skillType;
            });

            enemy.Get<TurnContext>().Execute<UniversalResolution>(enemy, r =>
            {
                return r.SkillType == skillType;
            });
        }
        private static void Swap(Body player, Body enemy, SkillType.CEValue skillType)
        {
            if (player.Get<Level>().KilledTimes > 0 || enemy.Get<Level>().KilledTimes > 0)
            {
                return;
            }
            var plist = player.Get<TurnContext>().Get<UniversalResolution>();
            var elist = enemy.Get<TurnContext>().Get<UniversalResolution>();

            var ep = plist.Where(r => r.SkillType == skillType).ToList();
            var ee = elist.Where(r => r.SkillType == skillType).ToList();

            plist.RemoveAll(r => ep.Contains(r));
            elist.RemoveAll(r => ee.Contains(r));

            plist.AddRange(ee);
            elist.AddRange(ep);
        }
        private static void Tan(Body player, Body enemy)
        {
            Swap(player, enemy, SkillType.Instance.Tan());
            Default(player, enemy, SkillType.Instance.Tan());
        }
        private static void Xiaoxiao(Body player, Body enemy)
        {
            Swap(player, enemy, SkillType.Instance.Xiaoxiao());
            Default(player, enemy, SkillType.Instance.Xiaoxiao());
        }
        private static void Zige(Body player, Body enemy)
        {
            Default(player, enemy, SkillType.Instance.Zige());
        }
        private static void Resource(Body player, Body enemy)
        {
            Default(player, enemy, SkillType.Instance.Resource());
        }
        private static void Defense(Body player, Body enemy)
        {
            Default(player, enemy, SkillType.Instance.Defense());
        }
        private static void Taichi(Body player, Body enemy)
        {
            Default(player, enemy, SkillType.Instance.Taichi());
        }
        private static void CancelAttack(Body player, Body enemy)
        {

            if (player.Get<Level>().KilledTimes > 0 || enemy.Get<Level>().KilledTimes > 0)
            {
                return;
            }
            var plist = player.Get<TurnContext>().Get<UniversalResolution>();
            var elist = enemy.Get<TurnContext>().Get<UniversalResolution>();

            var pa = plist.Find(r => r.SkillType == SkillType.Instance.Attack());
            var ea = elist.Find(r => r.SkillType == SkillType.Instance.Attack());

            if (pa != null && ea != null)
            {
                var temp1 = pa.Power;
                var temp2 = ea.Power;
                pa.Power = temp1 - temp2;
                ea.Power = temp2 - temp1;
            }
        }
        private static void Attack(Body player, Body enemy)
        {
            Swap(player, enemy, SkillType.Instance.Attack());
            Default(player, enemy, SkillType.Instance.Attack());
        }
        private static void Shengji(Body player, Body enemy)
        {
            Default(player, enemy, SkillType.Instance.Shengji());
        }
        private void End(Body player, Body enemy)
        {
            player.Update();
            enemy.Update();
            if (player.Get<Level>().KilledTimes == 0 && enemy.Get<Level>().KilledTimes == 0)
            {
                InnerRound++;
                return;
            }
            player.Get<Resource>().Init();
            enemy.Get<Resource>().Init();
            player.Get<Level>().Upgrade(enemy.Get<Level>().KilledTimes);
            enemy.Get<Level>().Upgrade(player.Get<Level>().KilledTimes);
            enemy.Get<Level>().KilledTimes = 0;
            player.Get<Level>().KilledTimes = 0;
            InnerRound = 1;
            Round++;
        }
    }
}
