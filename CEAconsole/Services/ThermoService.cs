using CEAconsole.Models;
using Newtonsoft.Json;

namespace CEAconsole.Services
{
    public static class ThermoService
    {
        private static readonly string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/shortThermo.json");
        private static readonly string json = File.ReadAllText(path);

        public static ICollection<Reactant> GetReactants()
        {
            List<Reactant>? reactantList = JsonConvert.DeserializeObject<List<Reactant>>(json);
            ICollection<Reactant>? reactantCollection = reactantList;
            return reactantCollection;
        }
    }
}
