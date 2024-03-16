using CEAconsole.Models;

namespace CEAconsole.Services
{
    public static class TransportService
    {
        private static readonly string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/shortTrans.json");
        private static readonly string json = File.ReadAllText(path);

        public static string GetTransportProperties()
        {
            return json;
        }
    }
}
