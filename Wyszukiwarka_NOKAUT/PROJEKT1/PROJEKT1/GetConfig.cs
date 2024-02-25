namespace PROJEKT1
{
    //Wykorzystywane do wyviągania z JSONa nazw i folderów - szybki config ze strony np. serwisanta
    internal class GetConfig
    {
        private static string config = "Config";
        private static string GetData(string pole)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"{config}.json")
                .Build();

            return configuration[$"{config}:{pole}"];
        }

        public static string EnovaFolder => GetData("Folder").Trim();

        public static string APIKey => GetData("APIKey");


        public static string Database => GetData("Database");

        public static string User => GetData("User");

        public static string Pass => GetData("Pass");

        }
    }
