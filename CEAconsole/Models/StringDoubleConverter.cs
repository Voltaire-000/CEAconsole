using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CEAconsole.Models
{
    public class StringDoubleConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(double) || objectType == typeof(string);

        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Float)
            {
                return token.ToObject<double>();
            }
            if (token.Type == JTokenType.String)
            {
                return token.ToObject<string>();
            }
            throw new JsonSerializationException("Unexpected token type : " + token.Type.ToString());
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
