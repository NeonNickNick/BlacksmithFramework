using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.Resolutions;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Models.Particular;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.Defense;

namespace BlacksmithCore.Specific.BuiltInProfessions
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public partial class Warlock : MainProfession
    {
        private bool MagicCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        [HasResource]
        [Labels(Impression.Robust, Strength.Strong)]
        private IDSLSourceFile Magic(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteResource(1, ResourceType.Instance.Magic());
            return DSL.Create(sc.Self, pen);
        }

        private bool MagicAttackCheck(ISkillContext sc)
        {
            return sc.Param > 0 && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Magic(), sc.Param);
        }
        [HasAttack(2)]
        [HasAttack(2)]
        [HasAttack(2)]
        [IsInfinite]
        [Labels(Impression.Robust, Strength.Strong)]
        private IDSLSourceFile MagicAttack(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.Param, ResourceType.Instance.Magic())
                .WriteAttack(2 * sc.Param, AttackType.Instance.Physical(), delayRounds: 0)
                .WriteAttack(2 * sc.Param, AttackType.Instance.Physical(), delayRounds: 1)
                .WriteAttack(2 * sc.Param, AttackType.Instance.Physical(), delayRounds: 2);
            return DSL.Create(sc.Self, pen);
        }

        private bool MuteCheck(ISkillContext sc) => true;
        [Labels(Impression.Aggressive, Strength.Super)]
        private IDSLSourceFile Mute(ISkillContext sc)
        {
            Pen pen = sf => sf
               .WriteEffect(EffectType.Instance.AfterTransport(), EffectTargetType.Instance.Enemy(), 0, 1,
               (Community source, Body main, EffectEntity effectEntity) =>
               {
                   main.Get<TurnContext>().Get<ResourceResolution>().RemoveAll(r => r.Type == ResourceType.Instance.Space() || r.Type == ResourceType.Instance.Time());
               });
            return DSL.Create(sc.Self, pen);
        }

        private bool SacrificeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Health>().HP > 1;
        }
        [HasDefense]
        [HasResource]
        [Labels(Impression.Robust, Strength.Super)]
        private IDSLSourceFile Sacrifice(ISkillContext sc)
        {
            Pen pen = sf => sf
                .LoseHP(1)
                .LoseMHP(1)
                .WriteDefense(7, new RealReduction())
                .WriteResource(1.5f, ResourceType.Instance.Iron());
            return DSL.Create(sc.Self, pen);
        }

        private bool AlchemyCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 2.5f);
        }
        [IsEquipmentSkill]
        [Labels(Impression.Robust, Strength.Strong)]
        private IDSLSourceFile Alchemy(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2.5f, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    source.Focus.Get<Skill>().RemoveSkill(nameof(Warlock), nameof(Alchemy).ToLower());
                    source.Focus.Get<Skill>().AddPackage(new(new Alchemy()));
                }, false);
            return DSL.Create(sc.Self, pen);
        }
    }
}