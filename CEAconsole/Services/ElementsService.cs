using CEAconsole.Models;
using Newtonsoft.Json;

namespace CEAconsole.Services
{
    public static class ElementsService
    {
        //private static readonly string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/tableOfElements.json");
        //private static readonly string json = File.ReadAllText(path);

        public static ICollection<Element> GetElements(string path)
        {
            string m_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            string json = File.ReadAllText(m_path);
            ICollection<Element>? elements = JsonConvert.DeserializeObject<ICollection<Element>>(json);

            return elements;
        }
    }
}
