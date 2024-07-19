
using CEAconsole.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace CEAconsole.Thermo.Services
{
    public class DataService
    {
        private static readonly Lazy<DataService> _instance = new(() => new DataService());
        public static DataService Instance => _instance.Value;

        public List<Specie> AllSpecies { get; private set; }
        //public List<Specie> GaseousSpecies { get; private set; }

        private DataService() { }
        public async Task LoadDataAsync()
        {
            AllSpecies = await DataService.LoadJsonDataAsync<List<Specie>>("ModNASAspecies.json");
        }
        public static async Task<TemperatureList> GetTempSchedule(string path)
        {
            string m_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            var json = await File.ReadAllTextAsync(m_path);
            var temperatureLists = JsonConvert.DeserializeObject<TemperatureList>(json);

            return temperatureLists;
        }

        private static async Task<T> LoadJsonDataAsync<T>(string fileName)
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
