using Blacksmith.Backend.SkillPackages.Core;
using Blacksmith.DLC;
using Blacksmith.Frontend;
namespace Blacksmith
{
    public static class Program
    {
        public static void Main()
        {
            LoadProfessionPlugins();
            ConsoleFrontend.Start();
        }
        private static void LoadProfessionPlugins()
        {
            var plugins = PluginLoader.LoadPlugins<SkillPackageBase>(".");
            foreach (var plugin in plugins)
            {
                ProfessionRegistry.Regist(plugin.Name);
            }
        }
    }
}
