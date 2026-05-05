using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.BuiltInProfessions;
using ClapInfra.ClapModels.Components;
namespace BlacksmithCore.Infra.Models.Components
{
    public class PackageContainer : ClapPackageContainer<ISkillPackage>
    {
        public PackageContainer(ISkillPackage skillpackage) : base(skillpackage)
        {
        }
    }
    public class Skill : ClapSkill<PackageContainer, ISkillPackage, ISkillContext, IDSLSourceFile>
    {
        protected override List<PackageContainer> _packages { get; set; } = new() { new(new Common()) };
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
        public bool HaveProfession => _packages.Count > 1;

    }
}
