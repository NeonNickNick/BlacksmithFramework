using BlacksmithClient.Frontend;
using BlacksmithCore.AI;
using BlacksmithCore.AI.Strategies;
using BlacksmithCore.Infra.Utils;
namespace BlacksmithClient
{
    public static class Program
    {
        public static void Main()
        {
            PluginLoader.Initialize(AppContext.BaseDirectory);

            GeneralStrategyParams? param = null;//暂时使用默认参数
            List<IAIStrategy> strategies = new()
            {
                new BloodSigilStrategy(),
                new GeneralStrategy(param)
            };
            Console.WriteLine("Welcome!\n");
            LocalHost.Start(strategies);
        }

    }
}
