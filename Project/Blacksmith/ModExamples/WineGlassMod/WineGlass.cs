using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.Resolutions;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Profession;

namespace ModExamples.WineGlassMod
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public partial class WineGlass : MainProfession
    {
        private bool WineCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 0.5f);
        }
        private IDSLSourceFile Wine(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(0.5f, ResourceType.Instance.Iron())
                .WriteResource(1f, ResourceType.Instance.Wine());
            return DSL.Create(sc.Self, pen);
        }
        private bool CarnivalCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Wine(), 3f);
        }
        private IDSLSourceFile Carnival(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3f, ResourceType.Instance.Wine())
                .WriteEffect(
                    EffectType.Instance.AfterTransport(),
                    EffectTargetType.Instance.Self(),
                    power: 0f,
                    duration: 1,
                    (source, target, entity) =>
                    {
                        var alist = target.Get<TurnContext>().Get<AttackResolution>();
                        target.Get<TurnContext>().Get<AttackResolution>()
                              .RemoveAll(a => a.Clock.IsRinging);
                    }
                );
            return DSL.Create(sc.Self, pen);
        }
        private bool ForgetCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Wine(), 3f);
        }
        private IDSLSourceFile Forget(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3f, ResourceType.Instance.Wine())
                .WriteEffect(
                    EffectType.Instance.AfterResolutionWritten(),
                    EffectTargetType.Instance.Self(),
                    power: 0f,
                    duration: 3,
                    (source, target, entity) =>
                    {
                        var alist = target.Get<TurnContext>().Get<AttackResolution>();
                        target.Get<TurnContext>().Get<AttackResolution>()
                              .RemoveAll(a => a.Clock.IsRinging);
                    },
                    delayRounds: 1
                );
            return DSL.Create(sc.Self, pen);
        }
    }
}
