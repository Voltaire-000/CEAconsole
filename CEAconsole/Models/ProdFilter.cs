using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CEAconsole.Models
{
    public static class ProdFilter
    {
        public static string[]? ExtractProducts(string json)
        {
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement root = doc.RootElement;
                JsonElement onlyItems = root.GetProperty("only");
                string[]? Prod = JsonSerializer.Deserialize<string[]>(onlyItems.GetRawText());
                return Prod;
            }
        }
    }
}
