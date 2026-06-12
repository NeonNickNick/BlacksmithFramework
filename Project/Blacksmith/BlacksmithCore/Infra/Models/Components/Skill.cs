using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.BuiltInProfessions;
using ClapInfra.ClapModels.Components;
using ClapInfra.ClapUnit;

namespace BlacksmithCore.Infra.Models.Components
{
    public class PackageContainer : ClapPackageContainer<ISkillPackage>
    {
        public ClapSharedFlag Flag { get; set; } = new();
        public PackageContainer(ISkillPackage skillpackage) : base(skillpackage)
        {
        }
    }
    public class Skill : ClapSkill<PackageContainer, ISkillPackage, ISkillContext, IDSLSourceFile>
    {
        protected override List<PackageContainer> _packages { get; set; } = new() { new(new Common()) };
        public bool HaveProfession => _packages.Count > 1;
        public void Reset()
        {
            _packages = new() { new(new Common()) };
        }
        public override SkillDeclareResult TryDeclare(string skillName, ISkillContext sc)
        {
            foreach (var package in _packages.Where(p => p.Flag.IsActive))
            {
                if (!package.SkillPackage.AvailableSkillNames.Contains(skillName))
                {
                    continue;
                }
                if (package.SkillPackage.SkillChecker[skillName](sc))
                {
                    return SkillDeclareResult.Success;
                }
                else
                {
                    return SkillDeclareResult.Rejected;
                }
            }
            return SkillDeclareResult.Illegal;
        }
        public override IDSLSourceFile Declare(string skillName, ISkillContext sc)
        {
            foreach (var package in _packages.Where(p => p.Flag.IsActive))
            {
                if (!package.SkillPackage.AvailableSkillNames.Contains(skillName))
                {
                    continue;
                }
                return package.SkillPackage.SkillSourceFileGenerator[skillName](sc);
            }
            throw new ArgumentException("Unreachable1!");
        }
        public override List<string> GetAvailableSkillNames()
        {
            return _packages
                .Where(p => p.Flag.IsActive)
                .SelectMany(p => p.SkillPackage.AvailableSkillNames)
                .ToList();
        }
        public override List<string> GetActivePackageNames()
        {
            return _packages
                .Where(p => p.Flag.IsActive)
                .Select(p => p.Name)
                .ToList();
        }
        public HashSet<string> DisableAll()
        {
            var res = new HashSet<string>();
            foreach (var package in _packages)
            {
                package.Flag.Disable();
                res.Add(package.Name);
            }
            return res;
        }
        public void Enable(HashSet<string> names)
        {
            foreach (var package in _packages)
            {
                if (names.Contains(package.Name))
                {
                    package.Flag.Enable();
                }
            }
        }
        public List<IDSLSourceFile> GetPassiveSkill(ISkillContext sc)
        {
            return _packages.Where(p => p.Flag.IsActive).Select(p => p.SkillPackage.PassiveSkill(sc)).ToList();
        }
        public List<string> GetView()
        {
            if (_packages.Count < 2)
            {
                return new();
            }
            var temp = _packages.Select(p => p.Name).ToList();
            temp.RemoveAt(0);
            return temp;
        }
    }
}
