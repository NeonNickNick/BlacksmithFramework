using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.Backend.JudgementLogic.Judgement.Core;
using Blacksmith.Backend.JudgementLogic.TurnContexts;

namespace Blacksmith.Backend.JudgementLogic.Actor
{
    public class TurnContext
    {
        public List<AttackResolution> AttackResolutions { get; set; } = new();
        public List<DefenseResolution> DefenseResolutions { get; set; } = new();
        public List<ResourceResolution> ResourceResolutions { get; set; } = new();
        public List<EffectResolution> EffectResolutions { get; set; } = new();

        public void WriteResolution(IResolution resolution)
        {
            if (resolution is AttackResolution attackResolution)
            {
                AttackResolutions.Add(attackResolution);
            }
            else if (resolution is DefenseResolution defenseResolution)
            {
                DefenseResolutions.Add(defenseResolution);
            }else if(resolution is ResourceResolution resourceResolution)
            {
                ResourceResolutions.Add(resourceResolution);
            }
            else if (resolution is EffectResolution effectReslution)
            {
                EffectResolutions.Add(effectReslution);
            }
        }
        public void ExecuteDefense(ActorSet actorSet)
        {
            foreach (var temp in DefenseResolutions)
            {
                temp.Execute(actorSet);
            }
            DefenseResolutions.Clear();
        }
        public void ExecuteResource(ActorSet actorSet)
        {
            foreach (var temp in ResourceResolutions)
            {
                temp.Execute(actorSet);
            }
            ResourceResolutions.Clear();
        }
        public void ExecuteAttack(ActorSet actorSet)
        {
            foreach (var temp in AttackResolutions)
            {
                temp.Execute(actorSet);
            }
            AttackResolutions.Clear();
        }
        public void ExecuteEffect(ActorSet actorSet)
        {
            foreach (var temp in EffectResolutions)
            {
                temp.Execute(actorSet);
            }
            EffectResolutions.Clear();
        }
    }
}