using CEAconsole.Models;

namespace CEAconsole.Services
{
    public interface IThermoDynamicService
    {
        Task<FunctionsCalculated> CalculateThermodynamicProperties(IEnumerable<Specie> species, IEnumerable<Specie> refElements, Specie specie, SpecieType specieType, double temperature, double ReferenceTemperature = 298.15);
        Task<double> CalculateCp(double temperature, IEnumerable<Specie> specie);
        Task<double> CalculateEnthalpyH298(double temperature, IEnumerable<Specie> specie);
        Task<double> CalculateEntropy(double temperature, IEnumerable<Specie> specie);
        Task<double> CalculateGibbsH298(double ReferenceTemperature, double temperature, IEnumerable<Specie> specie);
        Task<double> CalculateEnthalpy(double temperature, IEnumerable<Specie> specie);
        Task<double> CalculateDeltaHf(double temperature, IEnumerable<Specie> specie, IEnumerable<Specie> refElements);
        Task<double> CalculateGibbsRxn(double temperature, IEnumerable<Specie> specie, IEnumerable<Specie> refElements);
        Task<double> CalculateLogK(double temperature, IEnumerable<Specie> specie, IEnumerable<Specie> refElements);
    }
}
