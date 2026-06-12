using AIPVPPlatform;
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

            var strategys = ModLoader.LoadByType<IAIStrategy>();
            Console.WriteLine("Welcome!\n");
            for(int i = strategys.Count - 1; i >= 0; --i)
            {
                Console.WriteLine($"{i}.{strategys[i].GetType().Name}");
            }
            IAIStrategy s2;
            IAIStrategy s1 = new GeneralStrategy();
            while (true)
            {
                try
                {
                    Console.Write("选择一个序号：");
                    var indexs = Console.ReadLine();
                    var index = int.Parse(indexs!);
                    s2 = strategys[index];
                    break;
                }
                catch
                {

                }

            }
            var aipvp = new AIPVP(s2, s1);
            var winrate  = aipvp.Start();
            Console.WriteLine($"相对GeneralStrategy胜率{winrate}%");
        }

    }
}