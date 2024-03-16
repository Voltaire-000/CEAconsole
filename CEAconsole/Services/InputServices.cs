using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEAconsole.Services
{
    public static class InputServices
    {
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

        public static string GetReactants(string reactantsJson) 
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reactantsJson);
            string json = File.ReadAllText(path);
            return json;
        }

        public static string GetTransportProperties(string transportPropertiesJson)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, transportPropertiesJson);
            string json = File.ReadAllText(path);
            return json;
        }
    }
}
