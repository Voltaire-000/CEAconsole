

// See https://aka.ms/new-console-template for more information
using CEAconsole.Models;
using CEAconsole.Services;
using CEAconsole.ThermoChemistry;
using MathNet.Numerics;

// Constants
double REFERENCE_TEMPERATURE = 298.15;
double Gas_Constant_R = 8.31446261815324;

// Dummy Data
double[] DummyData = { -999.123, -999.123, -999.123, -999.123, -999.123, -999.123, -999.123, -999.123, -999.123, -999.123 };
//
// Services Section

ICollection<CPHSRef> cPHSRefs = InputServices.GetDefaultCPHS("Data/Ref_Defaults.json");
ICollection<Reactant> ReactantsList = InputServices.GetSpecies("Data/newShortThermo.json");

string fuelName = "CH4";
IEnumerable<CPHSRef> CPHSdefaults = from item in cPHSRefs.Where(r => r.Species_Name == fuelName) select item;
List<Reactant>? searchedFuel = ReactantsList?.Where(item => item.Name == fuelName).ToList();
string oxidizerName = "O2";
Reactant? searchedOxidizer = ReactantsList?.Where(item => item.Name == oxidizerName).FirstOrDefault();

//string equation = BalanceEquation.HydrocarbonAndOxygen(fuelName, oxidizerName);

var chemformula = (from compound in searchedFuel
                   select compound.Molecule.ChemicalFormula).FirstOrDefault();

foreach (var element in chemformula)
{
    Console.WriteLine($"Element : {element.Key}, Value : {element.Value}");
}

// get the temperature ranges
var tempRange = (from range in searchedFuel
                 select range.TemperatureRange).FirstOrDefault();
// get the first temperature range and associated values
bool hasKeyRange_1 = tempRange.ContainsKey("range_1");
CEAconsole.Models.DataRecord firstTemperatureRangeObject;
List<double> temperatureRange = new();
List<double> coefficients = new();
List<double> temperatureExponents = new();
List<double> integrationConstants = new();
double H_Enthalpy = 0;

if (hasKeyRange_1)
{
    var range1 = tempRange.TryGetValue("range_1", out firstTemperatureRangeObject);
    if (firstTemperatureRangeObject != null)
    {
        temperatureRange = firstTemperatureRangeObject.TemperatureRange;
        coefficients = firstTemperatureRangeObject.Coefficients;
        temperatureExponents = firstTemperatureRangeObject.TExponents;
        integrationConstants = firstTemperatureRangeObject.IntegrationConstants;
        H_Enthalpy = firstTemperatureRangeObject.EnthalpyRef;
    }
}

List<double> temperatureList = [];
List<double> heatCapacityList = [];
List<double> enthalpyChangeFromRefList = [];
List<double> entropyList = [];
List<double> gibbsList = [];
List<double> enthalpyList = [];
List<double> deltaHfList = [];
List<double> logKlist = [];

Console.WriteLine( "\nThermoDynamic Functions Calculated from Coefficients for CH4");
Console.WriteLine("\n{0, -16} {1, -15} {2, -20} {3, -20} {4, -20} {5, -20} {6, -20} {7, -20}",
    "\tTemp Kelvin", "Cp J/mol-k", "H-H298.15 kJ/mol", "S J/mol-K", "G-H298.15/T J/mol-K", "H kJ/mol", "delta Hf kJ/mol", "log K");

// add defaults and start up numbers
//temperatureList.Add(0.0);
//heatCapacityList.Add(0.0);
//double defaultEnthalpyRef = (double)CPHSdefaults.ElementAt(0).EnthalpyRef;
double defaultEntropyRef = (double)CPHSdefaults.ElementAt(0).Entropy_Ref;
//enthalpyChangeFromRefList.Add(-defaultEnthalpyRef); // 10.016 = this is from the CPHSdefaults
//entropyList.Add(0.0);
//gibbsList.Add(0.0); // Should say INFINITE TODO
//double defaultEnthalpy = (double)CPHSdefaults.ElementAt(0).Enthalpy; // -84.616 from CPHSdefaults
//enthalpyList.Add(defaultEnthalpy);
//double defaultDeltaHf = (double)CPHSdefaults.ElementAt(0).Delta_Enthalpy;
//deltaHfList.Add(defaultDeltaHf);
//logKlist.Add(0.0); // should say INFINITE TODO
////temperatureList.Add(298.15);
//double Kelvin = 298.15;
//heatCapacityList.Add(ThermoDynamics.HeatCapacity(Kelvin, temperatureExponents, coefficients));
//enthalpyChangeFromRefList.Add(ThermoDynamics.EnthalpyRefH298(REFERENCE_TEMPERATURE,Kelvin, coefficients, temperatureExponents));
//entropyList.Add(ThermoDynamics.Entropy(REFERENCE_TEMPERATURE, temperatureExponents, coefficients, integrationConstants));
//gibbsList.Add(ThermoDynamics.GibbsRef(REFERENCE_TEMPERATURE, defaultEntropyRef, Kelvin, coefficients, temperatureExponents));
double heatOfFormation = -74600.0;
//enthalpyList.Add(ThermoDynamics.Enthalpy(REFERENCE_TEMPERATURE,heatOfFormation, Kelvin, coefficients, temperatureExponents));

List<double> temperatureSchedule = [0.0, 298.15, 398.15, 498.15, 598.15, 698.15, 798.15, 898.15, 998.15, 1000.00];

foreach (double temperature in temperatureSchedule)
{
    temperatureList.Add(temperature);
    double cp_value = ThermoDynamics.HeatCapacity(temperature, temperatureExponents, coefficients);
    heatCapacityList.Add(cp_value);
    double enthalpy_change_from_ref_value = ThermoDynamics.EnthalpyRefH298(REFERENCE_TEMPERATURE, temperature, coefficients, temperatureExponents);
    enthalpyChangeFromRefList.Add(enthalpy_change_from_ref_value);
    double entropy_value = ThermoDynamics.Entropy(temperature, temperatureExponents, coefficients, integrationConstants);
    entropyList.Add(entropy_value);
    double gibbs_value = ThermoDynamics.GibbsRef(REFERENCE_TEMPERATURE, defaultEntropyRef, temperature, coefficients, temperatureExponents);
    gibbsList.Add(gibbs_value);
    double enthalpy_value = ThermoDynamics.Enthalpy(temperature, temperatureExponents, coefficients, integrationConstants);
    enthalpyList.Add(enthalpy_value);
}

int round = 3;
for (int i = 0; i < 10; i++)
{
    Console.WriteLine("{0, -10} {1, -15} {2, -20} {3, -20} {4, -20} {5, -20} {6, -20} {7, -20}", "\t"
            + temperatureList.ElementAt(i) + " :", "\t"
            + heatCapacityList.ElementAt(i).Round(round), 
            enthalpyChangeFromRefList.ElementAt(i).Round(round), 
            entropyList.ElementAt(i).Round(round),
            gibbsList.ElementAt(i).Round(round),
            enthalpyList.ElementAt(i).Round(round),
            DummyData[i],
            DummyData[i]);
}



