namespace CEAconsole.Services
{
    public static class ThermoService
    {
        private static readonly string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/shortThermo.json");
        private static readonly string json = File.ReadAllText(path);

        public static string GetReactants()
        {
            return json;
        }
    }
}
