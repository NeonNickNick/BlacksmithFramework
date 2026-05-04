using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.Resolutions;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Models.Judgement;
using BlacksmithCore.Infra.Models.Judgement.Core;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.Defenses;

namespace BlacksmithCore.Specific.BuiltInProfessions
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public class Lancer : MainProfession
    {
        private bool _fire = false;
        private bool _ice = false;
        private Pen _icePen = sf => sf
            .WriteDefense(2, new CommonArmor());
        private bool _light = false;
        private Pen _lightPen = sf => sf
            .WriteRecovery(2);
        private bool _dark = false;
        private Pen _darkPen = sf => sf
            .WriteFree(source => source.Focus.Get<Health>().LoseMHP(1))
            .WriteAttack(1, AttackType.Instance.Real(), delayRounds: 0)
            .WriteAttack(1, AttackType.Instance.Real(), delayRounds: 1);


        private int Fire()
        {
            if (_fire)
            {
                _fire = false;
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
            if (_ice)
            {
                _ice = false;
                res += _icePen;
            }
            if (_light)
            {
                _light = false;
                res += _lightPen;
            }
            if (_dark)
            {
                _dark = false;
                res += _darkPen;
            }
            return res;
        }
        private bool SkyStrikeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        private IDSLSourceFile SkyStrike(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteAttack(3 + Fire(), AttackType.Instance.Physical())
                    .WithFree(AttackStage.OnHitArmorFirstTime, (a, b, c) => _fire = true)
                    .WithInterupt();
            return DSL.Create(sc.Self, Others(pen));
        }
        private bool DragonToothCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        private IDSLSourceFile DragonTooth(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteAttack(3 + Fire(), AttackType.Instance.Physical())
                    .WithFree(AttackStage.OnHitArmorFirstTime, (a, b, c) => _ice = true)
                .WriteDefense(3, new CommonReduction());
            return DSL.Create(sc.Self, Others(pen));
        }
        private bool TyrantDestructionCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        private IDSLSourceFile TyrantDestruction(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteAttack(3 + Fire(), AttackType.Instance.Physical(), APFactor: 2)
                    .WithFree(AttackStage.OnHitArmorFirstTime, (a, b, c) => _light = true);
            return DSL.Create(sc.Self, Others(pen));
        }
        private bool TripleStabCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        private IDSLSourceFile TripleStab(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteAttack(2 + Fire(), AttackType.Instance.Physical())
                    .WithFree(AttackStage.OnHitArmorFirstTime, (a, b, c) => _dark = true)
                .WriteAttack(2, AttackType.Instance.Physical())
                    .WithFree(AttackStage.OnHitArmorFirstTime, (a, b, c) => _dark = true)
                .WriteAttack(1, AttackType.Instance.Physical())
                    .WithFree(AttackStage.OnHitArmorFirstTime, (a, b, c) => _dark = true);
            return DSL.Create(sc.Self, Others(pen));
        }
        private int _chargeCount = 0;
        private int _chargeCost = 4;
        private bool _ifPassive = false;
        private bool RisingDragonCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), _chargeCost);
        }
        private IDSLSourceFile RisingDragon(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(_chargeCost, ResourceType.Instance.Iron())
                .WriteAttack(9 + _chargeCount * 4 + Fire(), AttackType.Instance.Magical());
            return DSL.Create(sc.Self, Others(pen));
        }
        private bool ChargeCheck(ISkillContext sc)
        {
            return _chargeCount < 2 && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), _chargeCost);
        }
        private IDSLSourceFile Charge(ISkillContext sc)
        {
            int chargeCountThis = _chargeCount + 1;
            Pen pen = sf => sf
                .UseResource(_chargeCost, ResourceType.Instance.Iron())
                .WriteFree(a => _chargeCount++)
                .WriteFree(a => _chargeCost = 0)
                .LinkJudgeRuleDynamic(DynamicJudgeRuleName.Instance.Charge(), new()
                {
                    new(AttackCanceling_Modifier_Before,
                    JudgeStage.Instance.OnAttackCanceling(),
                    RuleType.Modifier,
                    ModifierOrder.Before),
                    new((player, enemy) =>
                    {
                        if(_chargeCount == chargeCountThis && !_ifPassive)
                        {
                            _chargeCount = 0;
                            _chargeCost = 4;
                        }
                        _ifPassive = false;
                    },
                    JudgeStage.Instance.OnBegin(),
                    RuleType.Modifier,
                    ModifierOrder.Before,
                    delayRounds: 1)
                });
            return DSL.Create(sc.Self, pen);
        }
        private void AttackCanceling_Modifier_Before(Community player, Community enemy)
        {
            if (enemy.Focus.Get<TurnContext>().Get<AttackResolution>().Find(a => a.DelayRounds == 0) == null)
            {
                return;
            }
            _ifPassive = true;
            Pen pen = sf => sf
                .WriteAttack(10 + _chargeCount * 4 + Fire(), AttackType.Instance.Magical())
                .WriteFree(a => _chargeCount = 0)
                .WriteFree(a => _chargeCost = 4);
            DSL.Create(player, Others(pen)).Compile().Execute(player);

        }
    }

}
