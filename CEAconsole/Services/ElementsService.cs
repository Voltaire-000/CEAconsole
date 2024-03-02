using CEAconsole.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEAconsole.Services
{
    public class ElementsService
    {
        public ElementsService() 
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/tableOfElements.json");
            string json = File.ReadAllText(path);
            JsonSerializerSettings serializerSettings = new()
            {
                TraceWriter = new ConsoleTraceWriter()
            };
            Elements = JsonConvert.DeserializeObject<List<Element>>(json, serializerSettings);

        }

        public List<Element> GetElements()
        {
            return Elements ?? new List<Element>();
        }

        public List<Element>? Elements { get; }
    }
}
