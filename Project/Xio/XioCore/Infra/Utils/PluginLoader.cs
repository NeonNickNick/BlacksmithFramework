using System.Reflection;
using XioCore.Infra.Attributes;
using XioCore.Infra.Enum;
using XioCore.Infra.Profession;
using ClapInfra.ClapEnum;
using ClapInfra.ClapProfession;
using ClapInfra.ClapUtils;
namespace XioCore.Infra.Utils
{
    public static class PluginLoader
    {
        private static DllLoader dllLoader = new();
        public static void Initialize(string folderPath = ".")
        {
            dllLoader.Initialize(folderPath);
            LoadXioEnums();
            LoadProfessions();
        }
        private static void LoadXioEnums()
        {
            //先注册所有XioEnum
            var XioEnumPlugins = dllLoader.LoadByType<IXioEnum>();

            foreach (var plugin in XioEnumPlugins)
            {
                XioEnumRegistry.RegistXioEnum(plugin.GetType(), plugin);
            }
            //这里扩展方法情形稍微复杂一些
            //在刚才，XioEnum反射已经处理好定义，接下来只需要加入Modifier
            //LoadXioEnumModifiers();
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
        }/*
        private static void LoadXioEnumModifiers()
        {
            dllLoader.LoadStaticByAttribute(typeof(IsXioEnumModifier), ProcessXioEnumModifiers);
            ClapEnum.CloseFactory();
        }*/
        private static void ProcessXioEnumModifiers(Type type)
        {
            var supportedEnumDict = XioEnumRegistry.SupportedEnumDict;
            var eeValueTypeDict = XioEnumRegistry.CEValueTypeDict;
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (var method in methods)
            {
                var metaData = method.GetCustomAttribute<IsXioEnumMember>();
                var temp = method.GetParameters()[0].ParameterType;
                if (metaData == null ||
                    method.GetParameters().Length != 1 ||
                    !supportedEnumDict.Keys.Contains(temp) ||
                    method.ReturnType != eeValueTypeDict[temp])
                {
                    continue;
                }
                XioEnumRegistry.RegistXioEnumModifier(supportedEnumDict[temp], method.Name, metaData.Priority);
            }
        }
    }
}