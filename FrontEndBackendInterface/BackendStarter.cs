using Blacksmith.FrontendBackendInterface;

namespace Blacksmith.FrontEndBackendInterface
{
    public class BackendStarter
    {
        public GameContext StartBackend()
        {
            BackendEntry entry = new();
            var p = new TestActorSetState();
            p.As = entry.Player;
            var e = new TestActorSetState();
            e.As = entry.Enemy;
            GameContext context = new()
            {
                SkillChoose = entry,
                PlayerActorSetState = p,
                EnemyActorSetState = e
            };
            return context;
        }
    }
}
