using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.Resolutions;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Models.Particular;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.Defense;
using ModExamples.ProphetMod.Defense;

namespace ModExamples.ProphetMod
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public partial class Prophet : MainProfession
    {
        private Action<Community, Body, EffectEntity> _afterDodge = (source, b, c) =>
        {
            source.Focus.Get<Resource>().Gain(ResourceType.Instance.Crystal(), 1f);
        };
        private Action<Community, Body, EffectEntity> _dodgeFail = (a, b, c) => { };
        private Pen Dodge => sf => sf
                .WriteEffect(
                    EffectType.Instance.AfterTransport(),
                    EffectTargetType.Instance.Self(),
                    power: 0f,
                    duration: 1,
                    (source, target, entity) =>
                    {
                        var alist = target.Get<TurnContext>().Get<AttackResolution>();
                        bool hasAtk = alist.Find(a => a.Clock.IsRinging) != null;
                        target.Get<TurnContext>().Get<AttackResolution>()
                              .RemoveAll(a => a.Clock.IsRinging);
                        if (hasAtk)
                        {
                            _afterDodge(source, target, entity);
                            _afterDodge = (source, b, c) =>
                            {
                                source.Focus.Get<Resource>().Gain(ResourceType.Instance.Crystal(), 1f);
                            };
                        }
                        else
                        {
                            _dodgeFail(source, target, entity);
                            _dodgeFail = (a, b, c) => { };
                        }
                    }
                );
        private bool CrystalCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1.5f);
        }
        private IDSLSourceFile Crystal(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1.5f, ResourceType.Instance.Iron())
                .WriteResource(1f, ResourceType.Instance.Crystal());
            return DSL.Create(sc.Self, pen);
        }
        private bool CrystalBallCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Crystal(), 2f)
                && !sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.CrystalBall(), 1f);
        }
        private IDSLSourceFile CrystalBall(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2f, ResourceType.Instance.Crystal())
                .WriteResource(1f, ResourceType.Instance.CrystalBall());
            return DSL.Create(sc.Self, pen);
        }
        private bool ForetoldCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        private IDSLSourceFile Foretold(ISkillContext sc)
        {
            Pen pen = (sf => sf
                .UseResource(1f, ResourceType.Instance.Iron()))
                + Dodge;
            return DSL.Create(sc.Self, pen);
        }
        private bool GreatestCautionCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Crystal(), 1f);
        }
        private IDSLSourceFile GreatestCaution(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Crystal())
                .WriteFree(a =>
                {
                    _afterDodge += (Community source, Body target, EffectEntity entity) =>
                    {
                        DSL.Create(source, sf => sf.WriteAttack(4f, AttackType.Instance.Real(), delayRounds: 1)).Compile().Execute(source);
                    };
                }, true);
            return DSL.Create(sc.Self, pen);
        }
        private bool RevelationCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        private IDSLSourceFile Revelation(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Iron())
                .WriteFree(a =>
                {
                    _afterDodge += (Community source, Body target, EffectEntity entity) =>
                    {
                        source.Focus.Get<Resource>().Gain(ResourceType.Instance.Crystal(), 1f);
                    };
                }, true);
            return DSL.Create(sc.Self, pen);
        }
        private bool AssertionCheck(ISkillContext sc) => true;
        private IDSLSourceFile Assertion(ISkillContext sc)
        {
            Pen pen = (sf => sf
                .WriteFree(a =>
                {
                    _dodgeFail += (Community source, Body target, EffectEntity entity) =>
                    {
                        source.Focus.Get<Health>().LoseHP(2);
                    };
                }, true))
                + Dodge;
            return DSL.Create(sc.Self, pen);
        }
        private bool CrystalWallCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.CrystalBall(), 1f);
        }
        private IDSLSourceFile CrystalWall(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.CrystalBall())
                .WriteDefense(7, new CrystalWall(), delayRounds: 0)
                .WriteDefense(7, new CrystalWall(), delayRounds: 1);
            return DSL.Create(sc.Self, pen);
        }
        private bool RefractionCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.CrystalBall(), 1f);
        }
        private IDSLSourceFile Refraction(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(1, new PercentageReduction(baseline: 2));
            return DSL.Create(sc.Self, pen);
        }
        private bool UltimatumCheck(ISkillContext sc)
        {
            return sc.Param > 5
                && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.CrystalBall(), 1f)
                && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Crystal(), 2f);
        }
        [HasAttack]
        private IDSLSourceFile Ultimatum(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.CrystalBall())
                .UseResource(2f, ResourceType.Instance.Crystal())
                .WriteAttack(12f, AttackType.Instance.Real(), delayRounds: sc.Param);
            return DSL.Create(sc.Self, pen);
        }
    }
}
