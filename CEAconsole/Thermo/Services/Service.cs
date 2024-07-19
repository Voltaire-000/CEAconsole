using CEAconsole.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace CEAconsole.Thermo.DataService
{
    /// <summary>
    /// Loads the Data from the JSON files
    /// </summary>
    public class Service
    {

        public static async Task<ObservableCollection<Specie>> GetNASA(string path)
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync(path);
            using var reader = new StreamReader(stream);
            var contents = await reader.ReadToEndAsync();
            ObservableCollection<Specie> speciesCollection = JsonConvert.DeserializeObject<ObservableCollection<Specie>>(contents);
            return speciesCollection;
        }

        public static async Task<ObservableCollection<Specie>> GetModNASA(string path)
        {
            string m_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            var json = await File.ReadAllTextAsync(m_path);
            ObservableCollection<Specie> speciesCollection = JsonConvert.DeserializeObject<ObservableCollection<Specie>>(json);
            return speciesCollection;
        }

        public static async Task<TemperatureList> GetTempSchedule(string path)
        {
            string m_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            var json = await File.ReadAllTextAsync(m_path);
            var temperatureLists = JsonConvert.DeserializeObject<TemperatureList>(json);

            return temperatureLists;
        }


        //public async static Task<ObservableCollection<CPHSRef>> GetDefaultCPHS(string path)
        //{
        //    using var stream = await FileSystem.OpenAppPackageFileAsync(path);
        //    using var reader = new StreamReader(stream);
        //    var contents = await reader.ReadToEndAsync();
        //    ObservableCollection<CPHSRef> defaultsCollection = JsonConvert.DeserializeObject<ObservableCollection<CPHSRef>>(contents);
        //    return defaultsCollection;
        //}
        //public async static Task<ObservableCollection<Models.Element>> GetTableOfElements(string path)
        //{
        //    using var stream = await FileSystem.OpenAppPackageFileAsync(path);
        //    using var reader = new StreamReader(stream);
        //    var contents = await reader.ReadToEndAsync();
        //    ObservableCollection<Models.Element> tableOfElements = JsonConvert.DeserializeObject<ObservableCollection<Models.Element>>(contents);
        //    return tableOfElements;
        //}

    }
}
