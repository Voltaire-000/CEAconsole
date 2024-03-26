

// See https://aka.ms/new-console-template for more information
using CEAconsole.Models;
using CEAconsole.Services;
using MathNet.Numerics;

// Constants
double REFERENCE_TEMPERATURE = 298.15;
double Gas_Constant_R = 8.31446261815324;
//
// Services Section
string ElementsList = InputServices.GetElements("Data/tableOfElements.json");

ICollection<Reactant> ReactantsList = InputServices.GetReactants();
string fuelName = "CH4";
List<Reactant>? searchedFuel = ReactantsList?.Where(item => item.Name == fuelName).ToList();
string oxidizerName = "O2";
Reactant? searchedOxidizer = ReactantsList?.Where(item => item.Name == oxidizerName).FirstOrDefault();

string TransportPropertiesList = InputServices.GetTransportProperties("Data/shortTrans.json");

//string equation = BalanceEquation.HydrocarbonAndOxygen(fuelName, oxidizerName);

var chemformula = (from compound in searchedFuel
                   select compound.ChemicalFormula).FirstOrDefault();

foreach (var element in chemformula)
{
    Console.WriteLine($"Element : {element.Key}, Value : {element.Value}");
}

// get the temperature ranges
var tempRange = (from range in searchedFuel
                 select range.TemperatureRange).FirstOrDefault();
// get the first temperature range and associated values
bool hasKeyRange_1 = tempRange.ContainsKey("range_1");
CEAconsole.Models.Range firstTemperatureRangeObject;
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
        H_Enthalpy = firstTemperatureRangeObject.H_Jmol;
    }
}
// list of temperature exponents
double texp_1 = temperatureExponents[0];
double texp_2 = temperatureExponents[1];
double texp_3 = temperatureExponents[2];
double texp_4 = temperatureExponents[3];
double texp_5 = temperatureExponents[4];
double texp_6 = temperatureExponents[5];
double texp_7 = temperatureExponents[6];
double texp_8 = temperatureExponents[7];

// list of coefficients
double a1 = coefficients[0];
double a2 = coefficients[1];
double a3 = coefficients[2];
double a4 = coefficients[3];
double a5 = coefficients[4];
double a6 = coefficients[5];
double a7 = coefficients[6];

// list of integration constants
double a8 = integrationConstants[0];
double a9 = integrationConstants[1];

List<double> temperatureList = [];
List<double> heatCapacityList = [];
List<double> enthalpyList = [];
List<double> entropyList = [];

Console.WriteLine("\n{0, -16} {1, -10} {2, -10} {3, -10}", "\tTemp Kelvin", "Cp", "H", "S");

heatCapacityList.Add(0.0);
enthalpyList.Add(-10.016);
entropyList.Add(0.0);
temperatureList.Add(0.0);
temperatureList.Add(298.15);
double Kelvin = 298.15;
heatCapacityList.Add(ThermoDynamics.HeatCapacity(Kelvin, coefficients, temperatureExponents));
enthalpyList.Add(ThermoDynamics.Enthalpy(REFERENCE_TEMPERATURE, Kelvin, coefficients, temperatureExponents));
entropyList.Add(ThermoDynamics.Entropy(REFERENCE_TEMPERATURE, Kelvin, coefficients, temperatureExponents));
//temperatureList.Add(Kelvin);
double startTemp = 398.15;
double endTemp = 1000;
double increment = 100;
int digits = 3;

for (Kelvin = startTemp; Kelvin <= endTemp; Kelvin += increment)
{
    temperatureList.Add(Kelvin);
    double cp_value = ThermoDynamics.HeatCapacity(Kelvin, coefficients, temperatureExponents);
    heatCapacityList.Add(cp_value);
    double enthalpy_value = ThermoDynamics.Enthalpy(REFERENCE_TEMPERATURE, Kelvin, coefficients, temperatureExponents);
    //enthalpyList.Add(enthalpy_value);
    //double enthalpy_value = CalcEnthalpyChange(startTemp, temperatureList.Last<double>());
    double entropy_value = ThermoDynamics.Entropy(REFERENCE_TEMPERATURE, Kelvin, coefficients, temperatureExponents);
    //entropyList.Add(entropy_value);

    if (Kelvin >= 998.15)
    {
        double lastCp_value = ThermoDynamics.HeatCapacity(endTemp, coefficients, temperatureExponents);
        heatCapacityList.Add(lastCp_value);
    }
    enthalpyList.Add(Math.Round(enthalpy_value, digits));
    if (Kelvin >= 998.15)
    {
        double lastEnthalpy_value = ThermoDynamics.Enthalpy(REFERENCE_TEMPERATURE, endTemp, coefficients, temperatureExponents);
        enthalpyList.Add(lastEnthalpy_value);
    }
    entropyList.Add(entropy_value);
    if (Kelvin >= 998.15)
    {
        double lastEntropy_value = ThermoDynamics.Entropy(REFERENCE_TEMPERATURE, endTemp, coefficients, temperatureExponents);
        entropyList.Add(lastEntropy_value);
    }
}

temperatureList.Add(endTemp);


////-------------- Matrix Gaussian substitution
//var matrixA = Matrix<double>.Build.DenseOfArray(new[,] { { 1.0, 2.0 }, { 3.0, 4.0 } });
//var vectorB = Vector<double>.Build.DenseOfArray(new[] { 5.0, 11.0 });

//var resultX = matrixA.Solve(vectorB);

////
int round = 3;
for (int i = 0; i < 10; i++)
{
    Console.WriteLine("{0, -10} {1, -10} {2, -10} {3, -10}", "\t"
            + temperatureList.ElementAt(i) + " :", "\t"
            + heatCapacityList.ElementAt(i).Round(round), enthalpyList.ElementAt(i).Round(round), entropyList.ElementAt(i).Round(round));
}

//string inputCard = InputCardService.GetInputCard();
//string[]? Prod = ProdFilter.ExtractProducts(inputCard);

//Console.WriteLine("\nInput Card : " + inputCard);

//Console.WriteLine("\nOnly List :");
//for (int i = 0; i < Prod.Length; i++)
//{
//    Console.WriteLine("\t" + Prod[i]); 
//}


