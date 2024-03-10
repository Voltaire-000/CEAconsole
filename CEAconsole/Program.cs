﻿

// See https://aka.ms/new-console-template for more information
using CEAconsole.Models;
using CEAconsole.Services;
using MathNet.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

ThermoService thermoService = new();
//ElementsService elementsService = new();
//TransportService transportService = new();

List<Reactant>? reactants = new(thermoService.GetReactants());
string json = JsonConvert.SerializeObject(reactants, Formatting.Indented);

JArray parsedJson = JArray.Parse(json);
// filter the object based on the conditions
JArray filteredReactants = new(parsedJson.Where(r => r[key: "Name"].ToString() == "CH4"));
// convert back to a JSON string
string propertiesOfInputFilter = JsonConvert.SerializeObject(filteredReactants, Formatting.Indented);

JArray reactantProperties = JArray.Parse(propertiesOfInputFilter);
string? name = reactantProperties[0]?.SelectToken("Name")?.ToString();

JObject? chemicalFormula = reactantProperties[0].SelectToken("ChemicalFormula") as JObject;
foreach (var element in chemicalFormula)
{
    string elementName = element.Key;
    int? elementValue = element.Value?.ToObject<int>();
    Console.WriteLine($"Element: {elementName}, Value: {elementValue}");
}

// get temperature range_1
List<JToken>? temperatureRange = reactantProperties[0]?.SelectToken("TemperatureRange")?.ToList();
List<JToken> tempRange_1 = [.. temperatureRange[0]];
List<JToken> mChildren = [.. tempRange_1.Children()];
List<JToken> childTempRange = [.. mChildren[0]];
// number of coefficients
JToken childNumberOfCoefficients = mChildren[1];
int numberOfCoefficients = childNumberOfCoefficients.ToObject<int>();

// list of temperature exponents : 8 exponents
List<JToken> TExponents = [.. mChildren[2]];
JToken tempExponentsList = TExponents[0];
double? texp_1 = tempExponentsList[0]?.ToObject<double>();
double? texp_2 = tempExponentsList[1]?.ToObject<double>();
double? texp_3 = tempExponentsList[2]?.ToObject<double>();
double? texp_4 = tempExponentsList[3]?.ToObject<double>();
double? texp_5 = tempExponentsList[4]?.ToObject<double>();
double? texp_6 = tempExponentsList[5]?.ToObject<double>();
double? texp_7 = tempExponentsList[6]?.ToObject<double>();
double? texp_8 = tempExponentsList[7]?.ToObject<double>();

// list of coefficients
List<JToken> Coefficients = mChildren[4].ToList();
List<JToken> coefficientsList = Coefficients[0].ToList();

double a1 = coefficientsList[0].ToObject<double>();
double a2 = coefficientsList[1].ToObject<double>();
double a3 = coefficientsList[2].ToObject<double>();
double a4 = coefficientsList[3].ToObject<double>();
double a5 = coefficientsList[4].ToObject<double>();
double a6 = coefficientsList[5].ToObject<double>();
double a7 = coefficientsList[6].ToObject<double>();

// list of integration constants
List<JToken> integrationConstants = mChildren[5].ToList();
List<JToken> integrationConstantList = integrationConstants[0].ToList();
double a_8 = integrationConstantList[0].ToObject<double>();
double a_9 = integrationConstantList[1].ToObject<double>();

double Kelvin = 0;
double Gas_Constant_R = 8.31446261815324;

List<double> temperatureList = [];
List<double> heatCapacityList = [];
List<double> enthalpyList = [];
List<double> entropyList = [];

Console.WriteLine("{0, -16} {1, -10} {2, -10} {3, -10}", "\tTemp Kelvin", "Cp", "H", "S");


heatCapacityList.Add(0.0);

//heatCapacityList.Add(GetHeatCapacity(Kelvin));
enthalpyList.Add(GetEnthalpy(Kelvin));
entropyList.Add(GetEntropy(Kelvin));
temperatureList.Add(Kelvin);
double startTemp = 298.15;
double endTemp = 1000;
double increment = 100;

for (Kelvin = startTemp; Kelvin <= endTemp; Kelvin += increment)
{
    temperatureList.Add(Kelvin);
    double cp_value = GetHeatCapacity(Kelvin);
    heatCapacityList.Add(Math.Round(cp_value, 3));
    //double enthalpy_value = GetEnthalpy(Kelvin);
    double enthalpy_value = CalcEnthalpyChange(startTemp, temperatureList.Last<double>());
    double entropy_value = GetEntropy(Kelvin);
    
    if (Kelvin == 998.15)
    {
        double lastCp_value = GetHeatCapacity(endTemp);
        heatCapacityList.Add(Math.Round(lastCp_value, 3));
    }
    enthalpyList.Add(Math.Round(enthalpy_value, 3));
    if (Kelvin == 998.15)
    {
        double lastEnthalpy_value = GetEnthalpy(endTemp);
        enthalpyList.Add(Math.Round(lastEnthalpy_value, 3));
    }
    entropyList.Add(Math.Round(entropy_value, 3));
    if (Kelvin == 998.15)
    {
        double lastEntropy_value = GetEntropy(endTemp);
        entropyList.Add(Math.Round(lastEntropy_value, 3));
    }
}

double GetEntropy(double Kelvin)
{
    double Entropy = Gas_Constant_R * (-a1 * Math.Pow(Kelvin, -2)
                                     - (a2 * Math.Pow(Kelvin, -1))
                                     + (a3 * Math.Log(Kelvin))
                                     + (a4 * Kelvin)
                                     + (a5 * Math.Pow(Kelvin, 2) / 2)
                                     + (a6 * Math.Pow(Kelvin, 3) / 3)
                                     + (a7 * Math.Pow(Kelvin, 4) / 4)
                                     + a_9);
    return Entropy;
}

double GetEnthalpy(double Kelvin)
{
    double Enthalpy = Gas_Constant_R * Kelvin * -((a1 * Math.Pow(Kelvin, -2))
                                            + (a2 * Math.Pow(Kelvin, -1) * Math.Log(Kelvin))
                                            + a3
                                            + (a4 * (Kelvin / 2))
                                            + (a5 * (Math.Pow(Kelvin, 2) / 3))
                                            + (a6 * (Math.Pow(Kelvin, 3) / 4))
                                            + (a7 * (Math.Pow(Kelvin, 4) / 5))
                                            + (a_8 / Kelvin));
    Enthalpy /= 1000;
    return Enthalpy;

}

//double GetEnthalpy(double Kelvin)
//{
//    Kelvin = Kelvin - 289.15;
//    double Enthalpy = Gas_Constant_R * Kelvin * (a1 + a2 * Kelvin / 2 + (a3 * Math.Pow(Kelvin, 2) / 3) + (a4 * Math.Pow(Kelvin, 3) / 4) + (a5 * Math.Pow(Kelvin, 4)/5) + a6/Kelvin);
//    Enthalpy = Enthalpy / 1000;
//    return Enthalpy;
//}

double GetHeatCapacity(double Kelvin)
{
    double heat_capacity = Gas_Constant_R * (a1
                                        * Math.Pow(Kelvin, -2)
                                        + a2
                                        * Math.Pow(Kelvin, -1)
                                        + a3
                                        + a4
                                        * Kelvin
                                        + a5
                                        * Math.Pow(Kelvin, 2)
                                        + a6
                                        * Math.Pow(Kelvin, 3)
                                        + a7
                                        * Math.Pow(Kelvin, 4));
    return heat_capacity;
}

double CalcEnthalpyChange(double initialTemp, double finalTemp)
{
    int listCount = heatCapacityList.Count;
    double mIndex = heatCapacityList[listCount - 2];
    double enthalpyChange = Integrate.OnClosedInterval(x => heatCapacityList[listCount - 2] / 1000, initialTemp, finalTemp, 1e-8);
    return enthalpyChange;
}

temperatureList.Add(endTemp);

for (int i = 0; i < 10; i++)
{
    Console.WriteLine("{0, -10} {1, -10} {2, -10} {3, -10}", "\t" + temperatureList.ElementAt(i) + " :", "\t" + heatCapacityList.ElementAt(i), enthalpyList.ElementAt(i), entropyList.ElementAt(i));
}
