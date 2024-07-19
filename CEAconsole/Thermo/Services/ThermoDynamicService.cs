using CEAconsole.Models;
using CEAconsole.Services;

namespace CEAconsole.Thermo.Services
{
    public class ThermoDynamicService : IThermoDynamicService
    {
        public Task<double> CalculateCp(double temperature, IEnumerable<Specie> specie)
        {
            double result = Thermo.HeatCapacity.Cp(temperature, specie);
            return Task.FromResult(result);
        }

        public Task<double> CalculateDeltaHf(double temperature, IEnumerable<Specie> specie, IEnumerable<Specie> refElements)
        {
            double result = Thermo.Enthalpy.DeltaHrxn(temperature, specie, refElements);
            return Task.FromResult(result);
        }

        public Task<double> CalculateEnthalpy(double temperature, IEnumerable<Specie> specie)
        {
            double result = Thermo.Enthalpy.Enthalpy_H(temperature, specie);
            return Task.FromResult(result);
        }

        public Task<double> CalculateEnthalpyH298(double temperature, IEnumerable<Specie> specie)
        {
            double result = Thermo.Enthalpy.EnthalpyH298(temperature, specie);
            return Task.FromResult(result);
        }

        public Task<double> CalculateEntropy(double temperature, IEnumerable<Specie> specie)
        {
            double result = Thermo.Entropy.Entropy_S(temperature, specie);
            return Task.FromResult(result);
        }

        public Task<double> CalculateGibbsH298(double referenceTemperature,double temperature, IEnumerable<Specie> specie)
        {
            double referenceEntropy = Thermo.Entropy.Entropy_S(referenceTemperature, specie);
            double result = Thermo.Gibbs.Gibbs_RefH298_T(referenceTemperature, referenceEntropy, temperature, specie);
            return Task.FromResult(result);
        }

        public Task<double> CalculateGibbsRxn(double temperature, IEnumerable<Specie> specie, IEnumerable<Specie> refElements)
        {
            double deltaHrxn = Thermo.Enthalpy.DeltaHrxn(temperature, specie, refElements);
            double deltaSrxn = Thermo.Entropy.DeltaSrxn(temperature, specie, refElements);
            double result = Thermo.Gibbs.DeltaGibbsRxn(temperature, deltaHrxn, deltaSrxn);
            return Task.FromResult(result);
        }

        public Task<double> CalculateLogK(double temperature, IEnumerable<Specie> specie, IEnumerable<Specie> refElements)
        {
            double deltaHrxn = Thermo.Enthalpy.DeltaHrxn(temperature, specie, refElements);
            double deltaSrxn = Thermo.Entropy.DeltaSrxn(temperature, specie, refElements);
            double deltaGibbsRxn = Thermo.Gibbs.DeltaGibbsRxn(temperature, deltaHrxn, deltaSrxn);
            double result = Thermo.EquilibriumConstant.LogK(temperature, deltaGibbsRxn);
            return Task.FromResult(result);
        }

        public async Task<FunctionsCalculated> CalculateThermodynamicProperties(IEnumerable<Specie> species, IEnumerable<Specie> refElements, Specie specie, SpecieType specieType, double temperature, double ReferenceTemperature = 298.15)
        {

            var result = new FunctionsCalculated
            {
                Specie = specie,
                SpecieType = specieType,
                Temperature = temperature,
                Cp = await CalculateCp(temperature, species),
                H298 = await CalculateEnthalpyH298(temperature, species),
                Entropy = await CalculateEntropy(temperature, species),
                GibbsH298 = await CalculateGibbsH298(ReferenceTemperature, temperature, species),
                Enthalpy = await CalculateEnthalpy(temperature, species),
                DeltaHf = await CalculateDeltaHf(temperature, species, refElements),
                DeltaGibbsRxn = await CalculateGibbsRxn(temperature, species, refElements),
                LogK = await CalculateLogK(temperature, species, refElements),
            };
            
            return result;
        }
    }
}
