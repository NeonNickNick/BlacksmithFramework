using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapJudgement;

namespace BlacksmithCore.Infra.Judgement
{
    public class Judger : ClapJudger<Community, Judger, JudgeRuleManager, Intent, IDSLSourceFile>
    {
        public Judger(Community player, Community enemy) : base(player, enemy)
        {

        }
        public void Reset()
        {
            _playerIntents = new List<Intent>();
            _enemyIntents = new List<Intent>();
            JudgeRuleManager.Reset();
        }
        protected override IEnumerable<Intent> Compile(IEnumerable<IDSLSourceFile> sourceFiles)
        {
            // 预估容量以减少 List 扩容
            int? totalCount = (sourceFiles as ICollection<IDSLSourceFile>)?.Count;
            var passives = new List<IDSLSourceFile>(totalCount ?? 4);
            var nonPassives = new List<IDSLSourceFile>(totalCount ?? 4);

            // 一次遍历完成分组，保留原始顺序
            foreach (var sf in sourceFiles)
            {
                if (sf.IsPassive)
                    passives.Add(sf);
                else
                    nonPassives.Add(sf);
            }

            int passiveCount = passives.Count;
            int total = passiveCount + nonPassives.Count;
            var result = new Intent[total];
            Intent temp = new() { Execute = null! };

            // 1. 先放入被动文件的占位符（与原来一样）
            for (int i = 0; i < passiveCount; i++)
            {
                result[i] = temp;
            }

            // 2. 编译所有非被动文件并顺序放入被动占位符之后
            int index = passiveCount;
            foreach (var sf in nonPassives)
            {
                result[index++] = sf.Compile(this);
            }

            // 3. 最后编译被动文件并替换原来的占位符
            for (int i = 0; i < passiveCount; i++)
            {
                result[i] = passives[i].Compile(this);
            }

            return result;
        }
    }
}
