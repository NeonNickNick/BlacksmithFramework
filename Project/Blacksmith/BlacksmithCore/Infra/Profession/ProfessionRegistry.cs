using System.Reflection;
using BlacksmithCore.Infra.Attributes.Profession;
using BlacksmithCore.Infra.DSL;

namespace BlacksmithCore.Infra.Profession
{
    public static class ProfessionRegistry
    {
        public static readonly HashSet<string> Professions = new();
        public static readonly HashSet<string> ProfessionSkillNames = new();
        public static readonly HashSet<string> EquipmentSkillNames = new();
        private static readonly Dictionary<string, List<Type>> _modifierTypes = new();

        public static void RegistProfessionName(string professionName)
        {
            if (Professions.Contains(professionName))
            {
                throw new ArgumentException($"Profession \"{professionName}\" already exists! Expansion addition failed!");
            }
            Professions.Add(professionName);
            Console.WriteLine($"Successfully added the extended profession \"{professionName}\"!");
        }

        public static void RegistProfessionEquipmentSkillName(SkillPackageBase package)
        {
            static bool IsValidSkillMethod(MethodInfo method)
            {
                return method.IsPrivate
                    && method.ReturnType == typeof(IDSLSourceFile)
                    && method.GetParameters() is { Length: 1 } parameters
                    && parameters[0].ParameterType == typeof(ISkillContext);
            }
            var minfos = package.GetType().GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            );

            foreach (var info in minfos)
            {
                var pmark = info.GetCustomAttribute<IsProfessionSkill>();
                var emark = info.GetCustomAttribute<IsEquipmentSkill>();

                if (pmark != null)
                {
                    if (IsValidSkillMethod(info))
                        ProfessionSkillNames.Add(info.Name.ToLower());
                }
                if (emark != null)
                {
                    if (IsValidSkillMethod(info))
                        EquipmentSkillNames.Add(info.Name.ToLower());
                }
            }
        }

        

        public static void RegistProfessionModifier(string targetName, SkillPackageBase modifier)
        {
            if (!_modifierTypes.TryGetValue(targetName, out var list))
            {
                _modifierTypes[targetName] = list = new();
            }
            list.Add(modifier.GetType());
        }

        public static void AddModOnInit(SkillPackageBase package)
        {
            if (_modifierTypes.TryGetValue(package.GetType().Name, out var types))
            {
                foreach (var type in types)
                {
                    var modifier = (SkillPackageBase)Activator.CreateInstance(type)!;
                    package.AvailableSkillNames.AddRange(modifier.AvailableSkillNames);
                    foreach (var kv in modifier.SkillChecker)
                    {
                        package.SkillChecker[kv.Key] = kv.Value;
                    }
                    foreach (var kv in modifier.SkillSourceFileGenerator)
                    {
                        package.SkillSourceFileGenerator[kv.Key] = kv.Value;
                    }
                }
            }
        }
    }
}
