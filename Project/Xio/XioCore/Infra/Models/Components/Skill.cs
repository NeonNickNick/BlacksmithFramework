using ClapInfra.ClapModels.Components;
using XioCore.Infra.DSL;
using XioCore.Infra.Profession;
using XioCore.Specific.BuiltInProfessions;

namespace XioCore.Infra.Models.Components
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

    }
}
