using System.Reflection;

namespace ClapInfra.ClapUtils
{
    public class DllLoader
    {
        private readonly List<Assembly> _cache = new();
        public void Initialize(string folderPath = ".")
        {
            if (!Directory.Exists(folderPath))
                return;

            foreach (var dll in Directory.GetFiles(folderPath, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dll);
                    _cache.Add(assembly);
                }
                catch
                {
                    Console.WriteLine($"加载 {dll} 失败");
                }
            }
        }
        public List<T> LoadByType<T>()
        {
            var plugins = new List<T>();
            foreach (var assembly in _cache)
            {
                try
                {
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
                catch
                {
                    Console.WriteLine($"加载 {assembly} 失败");
                }
            }

            return plugins;
        }
        public void LoadStaticByAttribute(Type attributeType, Action<Type> process)
        {
            foreach (var assembly in _cache)
            {
                try
                {
                    var staticClasses = assembly.GetTypes()
                        .Where(t => t.IsClass
                                    && t.IsAbstract
                                    && t.IsSealed  // 静态类的特征
                                    && t.GetCustomAttribute(attributeType) != null);

                    foreach (Type type in staticClasses)
                    {
                        process(type);
                    }
                }
                catch
                {
                    Console.WriteLine($"加载{assembly}失败");
                }
            }
        }
    }
}
