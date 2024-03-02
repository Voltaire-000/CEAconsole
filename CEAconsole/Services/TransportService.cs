using CEAconsole.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEAconsole.Services
{
    public class TransportService
    {
        public TransportService()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/shortTrans.json");
            string json = File.ReadAllText(path);
            JsonSerializerSettings serializerSettings = new()
            {
                TraceWriter = new ConsoleTraceWriter()
            };

            TransportProperties = JsonConvert.DeserializeObject<List<TransportProperty>>(json, serializerSettings);

        }

        public List<TransportProperty> GetTransportProperties()
        {
            return TransportProperties ?? new List<TransportProperty>();
        }

        public List<TransportProperty>? TransportProperties { get; }
    }
}
