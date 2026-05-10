using BlacksmithCore.Infra.Utils;
using BlacksmithServer.Server;

namespace BlacksmithServer
{
    public static class Program
    {
        public static void Main()
        {
            ModLoader.Initialize(AppContext.BaseDirectory);
            Console.WriteLine("Blacksmith Multiplayer Server\n");
            WebGameServer.Start();
        }
    }
}
