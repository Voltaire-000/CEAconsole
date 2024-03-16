using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;

namespace CEAconsole.Filters
{
    public static  class ReactantFilter
    {
        public static string FilteredReactant(string json, string reactantName)
        {
            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
            };
            List<JsonElement>? records = JsonSerializer.Deserialize<List<JsonElement>>(json, options);
            List<JsonElement> filteredRecords = (from record in records
                                                 where record.TryGetProperty("Name", out JsonElement nameElement) && nameElement.GetString() == reactantName
                                                 select record).ToList();
            return JsonSerializer.Serialize(filteredRecords, options);
        }
    }
}
