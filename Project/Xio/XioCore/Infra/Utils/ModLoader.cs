using System.Text.Json;
using ClapInfra.ClapProfession;
using ClapInfra.ClapUtils;
using XioCore.Infra.Attributes;
using XioCore.Infra.Enum;
using XioCore.Infra.Profession;

namespace XioCore.Infra.Utils
{
    public static class ModLoader
    {
        private static readonly DllLoader _dllLoader = new();
        private static readonly string _modConfigName = "mod.json";
        private static string _configDirectory = ".xio";

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
            LoadXioEnums();
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

        private static void LoadXioEnums()
        {
            var XioEnumPlugins = _dllLoader.LoadByType<IXioEnum>();

            foreach (var plugin in XioEnumPlugins)
            {
                XioEnumRegistry.RegistXioEnum(plugin.GetType(), plugin);
            }
        }

        private static void LoadProfessions()
        {
            var ModProfessionPlugins = _dllLoader.LoadByType<SkillPackageBase>();
            foreach (var p in ModProfessionPlugins)
            {
                if (p is MainProfession plugin)
                {
                    ProfessionRegistry.RegistProfessionName(plugin.GetType().Name);
                }
            }
        }
    }
}
