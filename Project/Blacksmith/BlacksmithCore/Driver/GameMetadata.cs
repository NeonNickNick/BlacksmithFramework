using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.Attributes.SkillMetadata.Core;
using BlacksmithCore.Infra.Profession;

namespace BlacksmithCore.Driver
{
    public class GameMetadata : IGameMetadata
    {
        public class SkillMetadata
        {
            public string SkillName { get; private set; }
            public IReadOnlySet<ISkillMetadata> Classifications { get; private set; }
            public SkillMetadata(string skillName, IReadOnlySet<ISkillMetadata> classifications)
            {
                SkillName = skillName;
                Classifications = classifications;
            }
        }
        public SkillMetadata CurrentPlayerSkillMetadata = null!;
        public SkillMetadata CurrentEnemySkillMetadata = null!;
        public void UpdateCurrentSkill(string playerSkill, string enemySkill)
        {
            CurrentPlayerSkillMetadata = new(playerSkill, SkillMetadataDict[playerSkill]);
            CurrentEnemySkillMetadata = new(enemySkill, SkillMetadataDict[enemySkill]);
        }

        public IReadOnlySet<string> MainProfessionSkillNames
            => ProfessionRegistry.MainProfessionSkillNames;

        private HashSet<string> _equipmentSkillNames = null!;
        public IReadOnlySet<string> EquipmentSkillNames
        {
            get
            {
                if (_equipmentSkillNames == null)
                {
                    _equipmentSkillNames = new();
                    foreach (var skillName in ProfessionRegistry.SkillMetadataDict.Keys)
                    {
                        foreach (var isc in ProfessionRegistry.SkillMetadataDict[skillName])
                        {
                            if (isc.GetType() == typeof(IsEquipmentSkill))
                            {
                                _equipmentSkillNames.Add(skillName);
                                break;
                            }
                        }
                    }
                }
                return _equipmentSkillNames;
            }
        }
        private IReadOnlyDictionary<string, IReadOnlySet<ISkillMetadata>> _skillMetadataDict = null!;
        public IReadOnlyDictionary<string, IReadOnlySet<ISkillMetadata>> SkillMetadataDict
        {
            get
            {
                if (_skillMetadataDict == null)
                {
                    _skillMetadataDict = ProfessionRegistry.SkillMetadataDict
                        .ToDictionary(s => s.Key, s => (IReadOnlySet<ISkillMetadata>)s.Value);
                }
                return _skillMetadataDict;
            }
        }
    }
}
