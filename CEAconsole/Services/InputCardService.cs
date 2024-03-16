namespace CEAconsole.Services
{
    public static class InputCardService
    {
        private static readonly string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/H2AirInp.json");
        private static readonly string json = File.ReadAllText(path);

        public static string GetInputCard()
        {
            return json;
        }
    }
}
