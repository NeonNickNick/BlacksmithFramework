using System.Security.Cryptography;
using ClapInfra.ClapDSL;
using ClapInfra.ClapJudgement.Core;

namespace ClapInfra.ClapJudgement
{
    public class ClapJudger<TCommunity, TJudger, TJudgeRuleManager, TIntent, TIDSLSourceFile>
        where TJudger : ClapJudger<TCommunity, TJudger, TJudgeRuleManager, TIntent, TIDSLSourceFile>
        where TIDSLSourceFile : IClapDSLSourceFile<TCommunity, TJudger, TJudgeRuleManager, TIntent, TIDSLSourceFile>
        where TIntent : ClapIntent<TCommunity>
        where TJudgeRuleManager : ClapJudgeRuleManager<TCommunity>, new()
    {
        public TCommunity Player { get; protected set; }
        public TCommunity Enemy { get; protected set; }
        public TJudgeRuleManager JudgeRuleManager { get; }

        protected readonly Action _onJudge;

        protected List<TIntent> _playerIntents = new();
        protected List<TIntent> _enemyIntents = new();
        protected ClapJudger(TCommunity player, TCommunity enemy)
        {
            Player = player;
            Enemy = enemy;
            JudgeRuleManager = new();
            _onJudge = () =>
            {
                TranslateIntentsToResolutions();
                JudgeRuleManager.GetRule()(Player, Enemy);
            };
        }
        protected virtual List<TIntent> Compile(List<TIDSLSourceFile> sourceFiles)
        {
            return sourceFiles.Select(s => s.Compile((TJudger)this)).ToList();
        }
        public virtual void Judge(List<TIDSLSourceFile> playerSfs, List<TIDSLSourceFile> enemySfs)
        {
            _playerIntents = Compile(playerSfs);
            _enemyIntents = Compile(enemySfs);
            _onJudge();
        }
        protected void TranslateIntentsToResolutions()
        {
            foreach (var temp in _playerIntents)
            {
                temp.Execute(Player);
            }
            foreach (var temp in _enemyIntents)
            {
                temp.Execute(Enemy);
            }
        }
    }
}
