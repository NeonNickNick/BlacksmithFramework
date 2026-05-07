namespace BlacksmithCore.Infra.Profession
{
    public static class ProfessionRegistry
    {
        public static readonly List<string> Professions = new();
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
