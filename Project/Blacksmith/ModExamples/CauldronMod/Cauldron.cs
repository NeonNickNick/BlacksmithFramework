using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Models.Particular;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.Defense;
using ModExamples.CauldronMod.Defense;
using ModExamples.PhantomBookMod;
using ModExamples.PhantomBookMod.Defense;

namespace ModExamples.CauldronMod
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public partial class Cauldron : MainProfession
    {
        private bool FireCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        private IDSLSourceFile Fire(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Iron())
                .WriteResource(1f, ResourceType.Instance.Fire());
            return DSL.Create(sc.Self, pen);
        }
        private bool WaterCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        private IDSLSourceFile Water(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Iron())
                .WriteResource(1f, ResourceType.Instance.Water());
            return DSL.Create(sc.Self, pen);
        }
        private bool WoodCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        private IDSLSourceFile Wood(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Iron())
                .WriteResource(1f, ResourceType.Instance.Wood());
            return DSL.Create(sc.Self, pen);
        }
        private bool EarthCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        private IDSLSourceFile Earth(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Iron())
                .WriteResource(1f, ResourceType.Instance.Earth());
            return DSL.Create(sc.Self, pen);
        }
        private bool ExplosionCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Fire(), 1f);
        }
        private IDSLSourceFile Explosion(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Fire())
                .WriteAttack(4f, AttackType.Instance.Magical());
            return DSL.Create(sc.Self, pen);
        }
        private bool IceBladeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Water(), 1f);
        }
        private IDSLSourceFile IceBlade(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Water())
                .WriteAttack(5f, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }
        private bool RegenerationCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Wood(), 1f);
        }
        private IDSLSourceFile Regeneration(ISkillContext sc)
        {
            float begin = 1f;
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Wood())
                .WriteEffect(EffectType.Instance.AfterResolutionWritten()
                            , EffectTargetType.Instance.Self()
                            , begin
                            , 3
                            , (Community source, Body target, EffectEntity effectEntity) =>
                            {
                                target.Get<Health>().GainHP((int)effectEntity.Power);
                                begin++;
                                effectEntity.Power = begin;
                            });
            return DSL.Create(sc.Self, pen);
        }
        private bool StoneShellCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Earth(), 1f);
        }
        private IDSLSourceFile StoneShell(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Earth())
                .WriteDefense(0f, new StoneShell());
            return DSL.Create(sc.Self, pen);
        }
        private bool FireRainCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Earth(), 1f)
                && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Fire(), 1f);
        }
        private IDSLSourceFile FireRain(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Fire())
                .UseResource(1f, ResourceType.Instance.Earth())
                .WriteAttack(8f, AttackType.Instance.Physical(), delayRounds: 0)
                .WriteAttack(2f, AttackType.Instance.Real(), delayRounds: 0)
                .WriteAttack(1f, AttackType.Instance.Real(), delayRounds: 0);
            return DSL.Create(sc.Self, pen);
        }
        private bool ElementalArmorCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Earth(), 1f)
                && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Water(), 1f);
        }
        private IDSLSourceFile ElementalArmor(ISkillContext sc)
        {
            HashSet<string> packageNames = new();
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Earth())
                .UseResource(1f, ResourceType.Instance.Water())
                .WriteDefense(0f, new PhysicalImmunity())
                .WriteDefense(8f, new Defense.ElementalArmor(() =>
                {
                    sc.Self.Focus.Get<Skill>().RemovePackage(nameof(ElementalArmor));
                    sc.Self.Focus.Get<Skill>().Enable(packageNames);
                }))
                .WriteFree(source =>
                {
                    packageNames = source.Focus.Get<Skill>().DisableAll();
                    source.Focus.Get<Skill>().AddPackage(new(new ElementalArmor()));
                }, true);
            return DSL.Create(sc.Self, pen);
        }
    }
}
