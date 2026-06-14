using System.Runtime.Intrinsics.X86;
using System.Text.Json.Serialization;
using BlacksmithCore.Driver;
using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Profession;
using ClapInfra.ClapModels.Components;


namespace BlacksmithCore.AI.Strategies
{
    public class MetadataStrategy : IAIStrategy
    {
        private GameInstance _main = null!;
        private static Random _random = new();

        public string Name => "Metadata";

        public MetadataStrategy()
        {
        }

        public void Init(GameInstance gameInstance)
        {
            _main = gameInstance;
        }

        public (string skillName, int param, string stringParam) ChooseSkill()
        {
            var set = new HashSet<(string skillName, int param, string stringParam)>();
            var names = _main.Enemy.Focus.Get<Skill>().GetAvailableSkillNames();
            foreach (var n in names)
            {
                if (ProfessionRegistry.SkillMetadataDict[n].FirstOrDefault(s => s is IsInfinite _) != null)
                {
                    int layer = 1;
                    while(_main.ETryDeclare(n, layer) == SkillDeclareResult.Success)
                    {
                        layer++;
                    }
                    layer--;
                    set.Add((n, layer, ""));
                    if(layer > 1)
                    {
                        layer--;
                        set.Add((n, layer, ""));
                    }
                }
                else
                {
                    if (_main.ETryDeclare(n, 0) == SkillDeclareResult.Success)
                    {
                        set.Add((n, 0, ""));
                    }
                }
                
            }
            Dictionary<Labels, string> dict = new();
            foreach(var s in set)
            {
                dict[(Labels)ProfessionRegistry.SkillMetadataDict[s.skillName].FirstOrDefault(m => m is Labels _)!] = s.skillName;
            }

            var ls = dict.Keys.ToList();
            for(int i = ls.Count - 1; i >= 0; --i)
            {
                if (ProfessionRegistry.SkillMetadataDict[dict[ls[i]]].FirstOrDefault(m => m is HasAttack _) == null)
                {
                    if (_random.NextDouble() < 0.7)
                    {
                        dict.Remove(ls[i]);
                    }
                }
                if (ls[i].Impression == Impression.Aggressive)
                {
                    if(_random.NextDouble() < 0.5)
                    {
                        dict.Remove(ls[i]);
                    }
                }
                if (ls[i].Impression == Impression.Conservative)
                {
                    if (_random.NextDouble() < 0.4)
                    {
                        dict.Remove(ls[i]);
                    }
                }
            }

            var name = "iron";

            var ordinarys = dict.Keys.Where(k => k.Strength == Strength.Ordinary).ToList();
            if (ordinarys.Count > 0 && _random.NextDouble() < 0.3)
            {
                name = dict[ordinarys[_random.Next(ordinarys.Count)]];
            }

            var strongs = dict.Keys.Where(k => k.Strength == Strength.Strong).ToList();
            if (strongs.Count > 0 && _random.NextDouble() < 0.9)
            {
                name = dict[strongs[_random.Next(strongs.Count)]];
            }

            var supers = dict.Keys.Where(k => k.Strength == Strength.Super).ToList();
            if (supers.Count > 0 && _random.NextDouble() < 0.8)
            {
                name = dict[supers[_random.Next(supers.Count)]];
            }

            var all = set.Where(s => s.skillName == name).ToList();
            var param = all[_random.Next(all.Count)].param;
            return (name, param, "");
        }
    }
}