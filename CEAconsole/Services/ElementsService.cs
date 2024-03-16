namespace CEAconsole.Services
{
    public static class ElementsService
    {
        private static readonly string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/tableOfElements.json");
        private static readonly string json = File.ReadAllText(path);

        public static string GetElements()
        {
            return json;
        }
    }
}
