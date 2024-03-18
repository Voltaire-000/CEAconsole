using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CEAconsole.Models
{
    public static class BalanceEquation
    {
        public static string HydrocarbonAndOxygen(string json, string Fuel, string Oxidizer)
        {
            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
            };
            List<JsonElement>? records = System.Text.Json.JsonSerializer.Deserialize<List<JsonElement>>(json, options);
            List<JsonElement> fuelComponent = (from record in records
                                                 where record.TryGetProperty("Name", out JsonElement nameElement) &&
                                                 nameElement.GetString() == Fuel
                                                 select record).ToList();

            List<JsonElement> oxidizerComponent = (from record in records
                                               where record.TryGetProperty("Name", out JsonElement nameElement) &&
                                               nameElement.GetString() == Oxidizer
                                               select record).ToList();
            string fuelElement = System.Text.Json.JsonSerializer.Serialize(fuelComponent, options);
            // NewtonSoft
            JArray fuelProperties = JArray.Parse(fuelElement);
            JObject? ch4 = fuelProperties[0]?.SelectToken("chemicalFormula") as JObject;
            // get the value of each key
            var ttt = ch4.Children();
            int ttCount = ttt.Count();
            var first = ttt.First();
            var firstValue = first.AfterSelf();

            string oxidizerElement = System.Text.Json.JsonSerializer.Serialize(oxidizerComponent, options);
            // NewtonSoft
            JArray oxidizerProperties = JArray.Parse(oxidizerElement);
            JObject? O2 = oxidizerProperties[0]?.SelectToken("chemicalFormula") as JObject;

            var xx = O2.ContainsKey("O");
            int? oxidizerCount = (int?)O2.GetValue("O");
 
            foreach (KeyValuePair<string, JToken?> element in O2)
            {
                 int? ox = element.Value?.ToObject<int>();
            }

            return "a";
            //return JsonSerializer.Serialize(equation, options);
        }

    }
}
