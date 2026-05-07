using System.Reflection;
namespace ClapInfra.ClapProfession
{
    public interface IClapSkillPackage<TISkillContext, TIDSLSourceFile>
    {
        public List<string> AvailableSkillNames { get; }
        public Dictionary<string, Func<TISkillContext, bool>> SkillChecker { get; }
        public Dictionary<string, Func<TISkillContext, TIDSLSourceFile>> SkillSourceFileGenerator { get; }
        public abstract TIDSLSourceFile PassiveSkill(TISkillContext sc);
    }
    public enum PackageType
    {
        Main,
        Modifier
    }
    public abstract class ClapSkillPackage<TISkillContext, TIDSLSourceFile>
        : IClapSkillPackage<TISkillContext, TIDSLSourceFile>
    {
        public abstract PackageType PackageType { get; protected set; }
        private readonly List<string> _availableSkillNames = new();
        private readonly Dictionary<string, Func<TISkillContext, bool>> _skillChecker = new();
        private readonly Dictionary<string, Func<TISkillContext, TIDSLSourceFile>> _skillSourceFileGenerator = new();

        public List<string> AvailableSkillNames => _availableSkillNames;
        public Dictionary<string, Func<TISkillContext, bool>> SkillChecker => _skillChecker;
        public Dictionary<string, Func<TISkillContext, TIDSLSourceFile>> SkillSourceFileGenerator => _skillSourceFileGenerator;
        // 缓存结构：每个子类 Type 对应一组预解析的技能方法对
        private static readonly Dictionary<Type, List<SkillMethodPair>> s_skillMethodCache = new();
        private static readonly object s_cacheLock = new();

        // 技能方法对
        private sealed class SkillMethodPair
        {
            public string SkillName { get; init; } = string.Empty;
            public MethodInfo CheckMethod { get; init; } = null!;
            public MethodInfo GeneratorMethod { get; init; } = null!;
        }

        protected ClapSkillPackage(PackageType packageType)
        {
            var type = GetType();

            // 获取或构建该类型的缓存
            if (!s_skillMethodCache.TryGetValue(type, out var skillPairs))
            {
                lock (s_cacheLock)
                {
                    if (!s_skillMethodCache.TryGetValue(type, out skillPairs))
                    {
                        skillPairs = BuildSkillMethodPairs(type);
                        s_skillMethodCache[type] = skillPairs;
                    }
                }
            }

            // 基于缓存的 MethodInfo 创建委托并绑定当前实例
            foreach (var pair in skillPairs)
            {
                var checkDelegate = (Func<TISkillContext, bool>)Delegate.CreateDelegate(
                    typeof(Func<TISkillContext, bool>), this, pair.CheckMethod);
                var generatorDelegate = (Func<TISkillContext, TIDSLSourceFile>)Delegate.CreateDelegate(
                    typeof(Func<TISkillContext, TIDSLSourceFile>), this, pair.GeneratorMethod);

                _availableSkillNames.Add(pair.SkillName);
                _skillChecker.Add(pair.SkillName, checkDelegate);
                _skillSourceFileGenerator.Add(pair.SkillName, generatorDelegate);
            }

            PackageType = packageType;
            if (PackageType == PackageType.Main)
            {
                AddModOnInit();
            }
        }

        /// <summary>
        /// 一次性反射解析类型的所有技能方法对，使用字典避免 O(n²) 查找
        /// </summary>
        private static List<SkillMethodPair> BuildSkillMethodPairs(Type type)
        {
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

            // 按方法名（小写）建立索引，方便 O(1) 匹配
            var methodMap = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var method in methods)
            {
                methodMap[method.Name] = method;
            }

            var result = new List<SkillMethodPair>();

            foreach (var method in methods)
            {
                // 仅处理以 "Check" 结尾的方法
                if (!method.Name.EndsWith("Check", StringComparison.Ordinal))
                    continue;

                // 验证 Check 方法签名：bool (TISkillContext)
                if (method.ReturnType != typeof(bool) ||
                    method.GetParameters().Length != 1 ||
                    method.GetParameters()[0].ParameterType != typeof(TISkillContext))
                    continue;

                // 提取技能名
                string skillName = method.Name[..^"Check".Length];

                // 在字典中查找同名生成方法
                if (!methodMap.TryGetValue(skillName, out var generatorMethod))
                    continue;

                // 验证 Generator 方法签名：TIDSLSourceFile (TISkillContext)
                if (generatorMethod.ReturnType != typeof(TIDSLSourceFile) ||
                    generatorMethod.GetParameters().Length != 1 ||
                    generatorMethod.GetParameters()[0].ParameterType != typeof(TISkillContext))
                    continue;

                result.Add(new SkillMethodPair
                {
                    SkillName = skillName.ToLowerInvariant(),
                    CheckMethod = method,
                    GeneratorMethod = generatorMethod
                });
            }

            return result;
        }
        protected abstract void AddModOnInit();
        public abstract TIDSLSourceFile PassiveSkill(TISkillContext sc);
    }

}
