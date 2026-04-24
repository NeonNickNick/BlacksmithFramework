using System.Reflection;
using BlacksmithCore.AI;
using BlacksmithCore.AI.NaturalSelection;
using BlacksmithCore.AI.Strategies;
using BlacksmithCore.Backend.SkillPackages.Core;
using BlacksmithClient.Frontend;
using BlacksmithCore.Infra;
using BlacksmithCore.Infra.Attributes;
using BlacksmithCore.Infra.ExtensibleEnum;
using BlacksmithCore.Infra.ExtensibleProfession;
namespace Blacksmith
{
    public static class Program
    {
        public static void Main()
        {
            PluginLoader.Initialize(AppContext.BaseDirectory);
            LoadBlacksmithEnumModifierPlugins();
            LoadProfessionPlugins();

            GeneralStrategyParams? param = null;
            try
            {
                param = Selector.LoadFromFile<GeneralStrategyParams>("data.json");
            }
            catch
            {
                param = null;
            }
            List<IAIStrategy> strategies = new()
            {
                new BloodSigilStrategy(),
                new GeneralStrategy(param)
            };

            Console.WriteLine("Welcome!\n");
            LocalHost.Start(strategies);
        }
        private static void LoadBlacksmithEnumModifierPlugins()
        {
            //先注册所有BlacksmithEnum
            var BlacksmithEnumPlugins = PluginLoader.LoadPluginsByType<BlacksmithEnum>();

            foreach (var plugin in BlacksmithEnumPlugins)
            {
                BlacksmithEnumRegistry.RegistBlacksmithEnum(plugin.GetType(), plugin);
            }
            //这里扩展方法情形稍微复杂一些
            //在刚才，BlacksmithEnum反射已经处理好定义，接下来只需要加入Modifier
            PluginLoader.LoadBlacksmithEnumModifierPlugins();
        }
        private static void LoadProfessionPlugins()
        {
            //先注册Mod包名
            var ModProfessionPlugins = PluginLoader.LoadPluginsByType<SkillPackageBase>();
            foreach (var plugin in ModProfessionPlugins)
            {
                if (plugin.PackageType == PackageType.Main)
                {
                    ProfessionRegistry.RegistProfessionName(plugin.GetType().Name);
                }
            }
            //接下来记录Mod对已有包的修改，最重要的是给Common包扩展技能，否则无法使用Mod职业
            foreach (var plugin in ModProfessionPlugins)
            {
                if (plugin.PackageType == PackageType.Modifier)
                {
                    var metaData = plugin.GetType().GetCustomAttribute<IsProfessionModifier>();
                    if (metaData == null)
                    {
                        return;
                    }
                    ProfessionRegistry.RegistProfessionModifier(metaData.TargetName, plugin);
                }
            }
        }
    }
}
