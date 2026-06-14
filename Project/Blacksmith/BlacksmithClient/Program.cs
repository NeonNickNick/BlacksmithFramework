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
            ModLoader.Initialize(AppContext.BaseDirectory);

            List<IAIStrategy> strategies = new()
            {
                new GeneralStrategy(),
                new AdversarialStrategy(),
                new MetadataStrategy(),
            };
            Console.WriteLine("Welcome!\n");
            LocalHost.Start(strategies);
        }

    }
}
