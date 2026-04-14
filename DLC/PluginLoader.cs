using System.Reflection;

namespace Blacksmith.DLC
{
    public static class PluginLoader
    {
        public static List<T> LoadPlugins<T>(string folderPath)
        {
            var plugins = new List<T>();

            if (!Directory.Exists(folderPath))
                return plugins;

            foreach (var dll in Directory.GetFiles(folderPath, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dll);

                    var types = assembly.GetTypes()
                        .Where(t => typeof(T).IsAssignableFrom(t)
                                    && t.IsClass
                                    && !t.IsAbstract);

                    foreach (var type in types)
                    {
                        // 创建实例
                        if (Activator.CreateInstance(type) is T plugin)
                            plugins.Add(plugin);
                    }
                }
                catch (BadImageFormatException)
                {
                    // 不是有效的 .NET 程序集，跳过
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载 {dll} 失败: {ex.Message}");
                }
            }

            return plugins;
        }
    }
}