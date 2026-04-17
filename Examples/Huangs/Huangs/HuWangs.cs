using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.SkillPackages.Core;
using Blacksmith.Backend.SkillPackages.Logic;
using Blacksmith.Backend.SkillPackages.Logic.BuitinProfessions;
using Blacksmith.Infra.Attributes;

namespace HuWangs
{
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    using DSL = DSLforSkillLogic;
    public class HuWangs : MainProfession
    {
        private bool HappyCheck(ISkillContext sc) => true;
        private DSL.SourceFile Happy(ISkillContext sc)
        {
            Console.WriteLine("I am happy");
            return new(sc.Self);
        }
    }
    [IsProfessionModifier("Common")]
    public class HuWangsModifier : ProfessionModifier
    {
        private bool HuWangsCheck(ISkillContext sc) => true;
        private DSL.SourceFile HuWangs(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Iron)
                .WriteFree(source =>
                {
                    Common.ExcludeAllProfessions(source);
                    source.Focus.Skill.AddPackage(new HuWangs());
                });
            return DSL.Create(sc.Self, pen);
        }
        private bool IronCheck(ISkillContext sc) => true;
        private DSL.SourceFile Iron(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteResource(2, ResourceType.Iron);
            return DSL.Create(sc.Self, pen);
        }
    }
}
