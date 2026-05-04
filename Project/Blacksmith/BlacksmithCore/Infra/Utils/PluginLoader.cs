using System.Reflection;
using BlacksmithCore.Infra.Attributes;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Enum;
using BlacksmithCore.Infra.Profession;
using ClapInfra.ClapEnum;
using ClapInfra.ClapProfession;
using ClapInfra.ClapUtils;
namespace BlacksmithCore.Infra.Utils
{
    using DSL = DSLforSkillLogic;
    public static class PluginLoader
    {
        private static DllLoader dllLoader = new();
        public static void Initialize(string folderPath = ".")
        {
            dllLoader.Initialize(folderPath);
            LoadBlacksmithEnums();
            LoadProfessions();
        }
        private static void LoadBlacksmithEnums()
        {
            //先注册所有BlacksmithEnum
            var BlacksmithEnumPlugins = dllLoader.LoadByType<IBlacksmithEnum>();

            foreach (var plugin in BlacksmithEnumPlugins)
            {
                BlacksmithEnumRegistry.RegistBlacksmithEnum(plugin.GetType(), plugin);
            }
            //这里扩展方法情形稍微复杂一些
            //在刚才，BlacksmithEnum反射已经处理好定义，接下来只需要加入Modifier
            LoadBlacksmithEnumModifiers();
        }
        private static void LoadProfessions()
        {
            //先注册Mod包名
            var ModProfessionPlugins = dllLoader.LoadByType<SkillPackageBase>();
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
        private static void LoadBlacksmithEnumModifiers()
        {
            dllLoader.LoadStaticByAttribute(typeof(IsBlacksmithEnumModifier), ProcessBlacksmithEnumModifiers);
            ClapEnum.CloseFactory();
        }
        private static void ProcessBlacksmithEnumModifiers(Type type)
        {
            var supportedEnumDict = BlacksmithEnumRegistry.SupportedEnumDict;
            var eeValueTypeDict = BlacksmithEnumRegistry.BEValueTypeDict;
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (var method in methods)
            {
                var metaData = method.GetCustomAttribute<IsBlacksmithEnumMember>();
                var temp = method.GetParameters()[0].ParameterType;
                if (metaData == null ||
                    method.GetParameters().Length != 1 ||
                    !supportedEnumDict.Keys.Contains(temp) ||
                    method.ReturnType != eeValueTypeDict[temp])
                {
                    continue;
                }
                BlacksmithEnumRegistry.RegistBlacksmithEnumModifier(supportedEnumDict[temp], method.Name, metaData.Priority);
            }
        }
    }
}