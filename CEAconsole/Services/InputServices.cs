using CEAconsole.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEAconsole.Services
{
    public static class InputServices
    {
        private static readonly string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/moleculeJson.json");
        private static readonly string json = File.ReadAllText(path);

        public static ICollection<Reactant> GetJsonData()
        {
            string m_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/moleculeJson.json");
            string json = File.ReadAllText(m_path);
            ICollection<Reactant>? reactantCollection = JsonConvert.DeserializeObject<ICollection<Reactant>>(json);
            return reactantCollection;
        }
        public static ICollection<Reactant> GetJsonData(string path)
        {
            string m_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            string json = File.ReadAllText(m_path);
            ICollection<Reactant>? reactantCollection = JsonConvert.DeserializeObject<ICollection<Reactant>>(json);
            return reactantCollection;
        }

        public static ICollection<CPHSRef> GetDefaultCPHS(string path)
        {
            string m_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            string json = File.ReadAllText(m_path);
            ICollection<CPHSRef>? defaultsCollection = JsonConvert.DeserializeObject<ICollection<CPHSRef>>(json);
            return defaultsCollection;
        }

        public static string GetInputCard(string caseInp)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, caseInp);
            string json = File.ReadAllText(path);
            return json;
        }

        public static string GetElements(string elementsJson) 
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, elementsJson);
            string json = File.ReadAllText(path);
            return json;
        }

        public static ICollection<Reactant> GetReactants() 
        {
            ICollection<Reactant>? reactantCollection = JsonConvert.DeserializeObject<ICollection<Reactant>>(json);
            return reactantCollection;
        }

        public static string GetTransportProperties(string transportPropertiesJson)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, transportPropertiesJson);
            string json = File.ReadAllText(path);
            return json;
        }
    }
}
