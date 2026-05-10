using XioClient.Frontend;
using XioCore.Infra.Utils;

namespace XioClient
{
    public static class Program
    {
        public static void Main()
        {
            ModLoader.Initialize(AppContext.BaseDirectory);
            Console.WriteLine("Welcome to Xio!\n");
            LocalHost.Start();
        }
    }
}
