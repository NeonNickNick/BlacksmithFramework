using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Judgement;
using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.Resolutions;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.Defense;
using ClapInfra.ClapUnit;

namespace BlacksmithCore.Specific.BuiltInProfessions
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public partial class Lancer : MainProfession
    {
        private readonly ClapStateVar<bool> _fire = new(false);
        private readonly ClapStateVar<bool> _ice = new(false);
        private readonly Pen _icePen = sf => sf
            .WriteDefense(2, new CommonArmor());
        private readonly ClapStateVar<bool> _light = new(false);
        private readonly Pen _lightPen = sf => sf
            .WriteRecovery(2);
        private readonly ClapStateVar<bool> _dark = new(false);
        private readonly Pen _darkPen = sf => sf
            .LoseMHP(1)
            .WriteAttack(1, AttackType.Instance.Real(), delayRounds: 0)
            .WriteAttack(1, AttackType.Instance.Real(), delayRounds: 1);


        private int Fire()
        {
            if (_fire.Value)
            {
                _fire.Reset();
                return 2;
            }
            else
            {
                return 0;
            }
        }
        private Pen Others(Pen pen)
        {
            var res = pen;
            if (_ice.Value)
            {
                _ice.Reset();
                res += _icePen;
            }
            if (_light.Value)
            {
                _light.Reset();
                res += _lightPen;
            }
            if (_dark.Value)
            {
                _dark.Reset();
                res += _darkPen;
            }
            return res;
        }
        private bool SkyStrikeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        [HasAttack(3)]
        [HasBuff]
        [Labels(Impression.Aggressive, Strength.Strong)]
        private IDSLSourceFile SkyStrike(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteAttack(3 + Fire(), AttackType.Instance.Physical())
                    .WithFree(AttackStage.OnHitArmorFirstTime, (a, b, c) => _fire.Set(true))
                    .WithInterupt();
            return DSL.Create(sc.Self, Others(pen));
        }
        private bool DragonToothCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        [HasAttack(3)]
        [HasDefense]
        [HasBuff]
        [Labels(Impression.Robust, Strength.Strong)]
        private IDSLSourceFile DragonTooth(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteAttack(3 + Fire(), AttackType.Instance.Physical())
                    .WithFree(AttackStage.OnHitArmorFirstTime, (a, b, c) => _ice.Set(true))
                .WriteDefense(3, new CommonReduction());
            return DSL.Create(sc.Self, Others(pen));
        }
        private bool TyrantDestructionCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        [HasAttack(3)]
        [HasBuff]
        [Labels(Impression.Robust, Strength.Strong)]
        private IDSLSourceFile TyrantDestruction(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteAttack(3 + Fire(), AttackType.Instance.Physical(), APFactor: 2)
                    .WithFree(AttackStage.OnHitArmorFirstTime, (a, b, c) => _light.Set(true));
            return DSL.Create(sc.Self, Others(pen));
        }
        private bool TripleStabCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        [HasAttack(2)]
        [HasAttack(2)]
        [HasAttack(1)]
        [HasBuff]
        [Labels(Impression.Robust, Strength.Strong)]
        private IDSLSourceFile TripleStab(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteAttack(2 + Fire(), AttackType.Instance.Physical())
                    .WithFree(AttackStage.OnHitArmorFirstTime, (a, b, c) => _dark.Set(true))
                .WriteAttack(2, AttackType.Instance.Physical())
                    .WithFree(AttackStage.OnHitArmorFirstTime, (a, b, c) => _dark.Set(true))
                .WriteAttack(1, AttackType.Instance.Physical())
                    .WithFree(AttackStage.OnHitArmorFirstTime, (a, b, c) => _dark.Set(true));
            return DSL.Create(sc.Self, Others(pen));
        }
        private ClapStateVar<int> _chargeCount = new(0);
        private ClapStateVar<int> _chargeCost = new(4);
        private ClapStateVar<bool> _wasPassive = new(false);
        private bool RisingDragonCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), _chargeCost.Value);
        }
        [HasAttack(10)]
        [Labels(Impression.Aggressive, Strength.Strong)]
        private IDSLSourceFile RisingDragon(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(_chargeCost.Value, ResourceType.Instance.Iron())
                .WriteAttack(9 + _chargeCount.Value * 4 + Fire(), AttackType.Instance.Magical());
            return DSL.Create(sc.Self, Others(pen));
        }
        private bool ChargeCheck(ISkillContext sc)
        {
            return _chargeCount.Value < 2 && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), _chargeCost.Value);
        }
        /*
        蓄力是目前最复杂的技能，与它相关的有三个状态变量：
            _chargeCount：蓄力层数，取值为0/1/2
            _chargeCost：蓄力消耗铁数，取值为4/0
            _wasPassive：是否是因为受到攻击被动触发，取值为false/true
        五种状态如下：
            state1: (0, 4, false)
            state2/3: (1, 0, false/true)
            state4/5: (2, 0, false/true)
        在_wasPassive为false的条件下，蓄力状态转移路径为1->2->4->1
        如果当回合判定为true，那么移动到平行true状态，之后在蓄力检测后回到1
        这一版本的蓄力只要对方在攻击抵消后还有残余攻击就会触发，且不考虑战矛可用的跨回合伤害技能（目前没有）
        以下“复位”指的是将_chargeCount设为0，_chargeCost设为4，不动_wasPassive

        反击逻辑：
        当玩家选择战矛这个职业的时候，如果不考虑炼药锅，他就不会打出跨回合伤害技能
        基于这个观察，首先在当回合攻击抵消阶段前插入一条规则，如果检测到对方有攻击，那就反击伏龙翔天
        在这之后复位，同时将_wasPassive设为true，表示刚才触发了反击
        
        蓄力检测逻辑：
        除了这个特性之外，另一个特性是如果中断蓄力那么蓄力效果就清除
        在下回合初插入一条规则检查蓄力层数，如果发现蓄力层数没有增长，说明要么这回合出的并不是蓄力，要么是上回合触发了反击
        反击时已经复位了，而反击之后下回合可以继续蓄力，因此这种情况不应该复位
        由此得到复位条件是蓄力层数没有增加并且上一回合没有触发反击
        在这之后，基于另一个观察，即蓄力检测插入的规则总是先于反击逻辑插入的规则结算，可以放心地将_wasPassive重置为false，
        如果这回合使用蓄力，又触发了反击，那么会正确地进入循环
        如果没有触发，那么就回到了state2
        如果没有使用蓄力，那么就回到了state1

        注意：前提条件是不考虑跨回合伤害技能。如果出现了战矛可用的这种技能，那么要将反击规则插入到攻击抵消之后，并且重新再做一遍抵消
        */
        [HasBuff]
        [Labels(Impression.Robust, Strength.Super)]
        private IDSLSourceFile Charge(ISkillContext sc)
        {
            int chargeCountThis = _chargeCount.Value + 1;
            Pen pen = sf => sf
                .UseResource(_chargeCost.Value, ResourceType.Instance.Iron())
                .WriteFree(a => _chargeCount.Increment(), true)
                .WriteFree(a => _chargeCost.Set(0), true)
                .RegistCallbackOnJudge(new()
                {
                    new ModifierCallback(AttackCanceling_Modifier_Before,
                    JudgeStage.Instance.OnAttackCanceling(),
                    ModifierOrder.Before,
                    new()),
                    new ModifierCallback((player, enemy) =>
                    {
                        if(_chargeCount.Value == chargeCountThis && !_wasPassive.Value)
                        {
                            _chargeCount.Reset();
                            _chargeCost.Reset();
                        }
                        _wasPassive.Reset();
                    },
                    JudgeStage.Instance.OnBegin(),
                    ModifierOrder.Before,
                    new(delayRounds: 1))
                });
            return DSL.Create(sc.Self, pen);
        }
        private void AttackCanceling_Modifier_Before(Community player, Community enemy)
        {
            if (enemy.Focus.Get<TurnContext>().Get<AttackResolution>().Find(a => a.Clock.IsRinging) == null)
            {
                return;
            }
            _wasPassive.Set(true);
            Pen pen = sf => sf
                .WriteAttack(10 + _chargeCount.Value * 4 + Fire(), AttackType.Instance.Magical())
                .WriteFree(a => _chargeCount.Reset(), true)
                .WriteFree(a => _chargeCost.Reset(), true);
            DSL.Create(player, Others(pen)).Compile().Execute(player);

        }
    }

}
