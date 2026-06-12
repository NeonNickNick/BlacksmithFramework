using System.Reflection;
using System.Text.Json;
using BlacksmithCore.Infra.Attributes.BlacksmithEnum;
using BlacksmithCore.Infra.Attributes.Profession;
using BlacksmithCore.Infra.Enum;
using BlacksmithCore.Infra.Profession;
using ClapInfra.ClapEnum;
using ClapInfra.ClapUtils;
namespace BlacksmithCore.Infra.Utils
{
    public static class ModLoader
    {
        private static readonly DllLoader _dllLoader = new();
        private static readonly string _modConfigName = "mod.json";
        private static string _configDirectory = ".blacksmith";
        public static List<T> LoadByType<T>() => _dllLoader.LoadByType<T>();
        public static void Initialize(string basePath)
        {
            _configDirectory = Path.Combine(basePath, _configDirectory);
            var configPath = Path.Combine(_configDirectory, _modConfigName);
            var dict = new Dictionary<string, object>();

            if (!Directory.Exists(_configDirectory))
            {
                Console.WriteLine($"Mod config directory not found: {_configDirectory}");
            }
            else if (!File.Exists(configPath))
            {
                Console.WriteLine($"Mod config file not found: {configPath}");
            }
            else
            {
                try
                {
                    var jsonString = File.ReadAllText(configPath);
                    dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);
                }
                catch (JsonException)
                {
                    Console.WriteLine($"Failed to parse {configPath}: expected string keys with string or string[] values");
                }
            }

            _dllLoader.Initialize(GetModDirectories(dict));
            LoadBlacksmithEnums();
            LoadProfessions();
        }
        private static List<string> GetModDirectories(Dictionary<string, object>? dict)
        {
            if (dict == null)
                return new();

            var res = new List<string>();
            foreach (var key in dict.Keys)
            {
                switch (dict[key])
                {
                    case string dir:
                        res.Add(Path.Combine(AppContext.BaseDirectory, dir.TrimStart('\\', '/')));
                        break;
                    case JsonElement je when je.ValueKind == JsonValueKind.String:
                        res.Add(Path.Combine(AppContext.BaseDirectory, je.GetString()!.TrimStart('\\', '/')));
                        break;
                    case JsonElement je when je.ValueKind == JsonValueKind.Array:
                        foreach (var item in je.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.String)
                                res.Add(Path.Combine(AppContext.BaseDirectory, item.GetString()!.TrimStart('\\', '/')));
                        }
                        break;
                    default:
                        Console.WriteLine($"Invalid value for key \"{key}\" in mod.json: expected string or string[]");
                        break;
                }
            }
            return res;
        }
        private static void LoadBlacksmithEnums()
        {
            //先注册所有BlacksmithEnum
            var BlacksmithEnumPlugins = _dllLoader.LoadByType<IBlacksmithEnum>();

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
            //先注册Mod包名，然后从里面收集关于职业和装备技能的信息
            var ModProfessionPlugins = _dllLoader.LoadByType<SkillPackageBase>();
            foreach (var p in ModProfessionPlugins)
            {
                if (p is MainProfession plugin)
                {
                    ProfessionRegistry.RegistProfessionName(plugin.GetType().Name);
                    ProfessionRegistry.CollectSkillMetadata(plugin);
                }
            }
            //接下来记录Mod对已有包的修改，最重要的是给Common包扩展技能，否则无法使用Mod职业
            foreach (var p in ModProfessionPlugins)
            {
                if (p is ProfessionModifier plugin)
                {
                    var metaData = plugin.GetType().GetCustomAttribute<IsProfessionModifier>();
                    if (metaData == null)
                    {
                        continue;
                    }
                    ProfessionRegistry.RegistProfessionModifier(metaData.TargetName, plugin);
                    // 元数据直接覆盖
                    ProfessionRegistry.CollectSkillMetadata(plugin);
                }
            }
        }
        private static void LoadBlacksmithEnumModifiers()
        {
            _dllLoader.LoadStaticByAttribute(typeof(IsBlacksmithEnumModifier), ProcessBlacksmithEnumModifiers);
            ClapEnum.CloseFactory();
        }
        private static void ProcessBlacksmithEnumModifiers(Type type)
        {
            var supportedEnumDict = BlacksmithEnumRegistry.SupportedEnumDict;
            var eeValueTypeDict = BlacksmithEnumRegistry.CEValueTypeDict;
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