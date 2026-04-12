using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Defenses;

namespace Blacksmith.Backend.SkillPackages.Logic
{
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    using DSL = DSLforSkillLogic;
    public interface ISkillPackage
    {
        public string Name { get; }
        public List<string> AvailableSkillNames { get; }
        public IReadOnlyDictionary<string, Func<ISkillContext, bool>> SkillChecker { get; }
        public IReadOnlyDictionary<string, Func<ISkillContext, DSL.SourceFile>> SkillSourceFileGenerator { get; }
    }
    public class Common : ISkillPackage
    {
        public string Name => "common";
        private static readonly string iron = "iron";
        private static readonly string stick = "stick";
        private static readonly string drill = "drill";
        private static readonly string slash = "slash";
        private static readonly string shield = "shield";
        private List<string> _availableSkillNames = new List<string>()
        {
            iron,
            stick,
            drill,
            slash,
            shield
        };
        public List<string> AvailableSkillNames
        {
            get => _availableSkillNames;
            set => _availableSkillNames = value;
        }
        private Dictionary<string, Func<ISkillContext, bool>> _skillChecker = new Dictionary<string, Func<ISkillContext, bool>>()
        {
            { iron, IronCheck },
            { stick, StickCheck},
            { drill, DrillCheck},
            { slash, SlashCheck},
            { shield, ShieldCheck},
        };
        public IReadOnlyDictionary<string, Func<ISkillContext, bool>> SkillChecker => _skillChecker;
        private Dictionary<string, Func<ISkillContext, DSL.SourceFile>> _skillSourceFileGenerator = new Dictionary<string, Func<ISkillContext, DSL.SourceFile>>()
        {
            { iron, Iron },
            { stick, Stick},
            { drill, Drill},
            { slash, Slash},
            { shield, Shield},
        };
        public IReadOnlyDictionary<string, Func<ISkillContext, DSL.SourceFile>> SkillSourceFileGenerator => _skillSourceFileGenerator;
        private static bool IronCheck(ISkillContext sc)
        {
            return true;
        }
        private static DSL.SourceFile Iron(ISkillContext sc)
        {
            Pen pen = sf => sf.WriteResource(1, ResourceType.Iron);
            return DSL.Create(pen);
        }
        private static bool StickCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 0.5f);
        }
        private static DSL.SourceFile Stick(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.Self, 0.5f, ResourceType.Iron)
                .WriteAttack(1, AttackType.Physical);
            return DSL.Create(pen);
        }
        private static bool DrillCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 1.5f);
        }
        private static DSL.SourceFile Drill(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.Self, 0.5f, ResourceType.Iron)
                .WriteAttack(3, AttackType.Physical);
            return DSL.Create(pen);
        }
        private static bool SlashCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 2.5f);
        }
        private static DSL.SourceFile Slash(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.Self, 0.5f, ResourceType.Iron)
                .WriteAttack(5, AttackType.Physical);
            return DSL.Create(pen);
        }
        private static bool ShieldCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, sc.Param * 0.5f);
        }
        private static DSL.SourceFile Shield(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.Self, sc.Param * 0.5f, ResourceType.Iron)
                .WriteDefense(2 + sc.Param, new TemporaryArmor());
            return DSL.Create(pen);
        }

    }
}