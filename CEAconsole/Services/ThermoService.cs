using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CEAconsole.Models;
using Newtonsoft.Json;

namespace CEAconsole.Services
{
    public class ThermoService
    {
        public List<Reactant>? Reactants { get; set; }
        public ThermoService()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/shortThermo.json");
            string json = File.ReadAllText(path);
            JsonSerializerSettings serializerSettings = new()
            {
                TraceWriter = new ConsoleTraceWriter()
            };

            Reactants = JsonConvert.DeserializeObject<List<Reactant>>(json, serializerSettings);

        }

        public List<Reactant> GetReactants()
        {
            return Reactants ?? new List<Reactant>();
        }
    }
}
