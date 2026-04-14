namespace Blacksmith.DLC
{
    public static class ProfessionRegistry
    {
        public static readonly List<string> Professions = new();
        public static void Regist(string professionName)
        {
            if (Professions.Contains(professionName))
            {
                Console.WriteLine($"Profession \"{professionName}\" already exists! Expansion addition failed!");
            }
            else
            {
                Professions.Add(professionName);
                Console.WriteLine($"Successfully added the extended profession \"{professionName}\"!");
            }
        }
    }
}
