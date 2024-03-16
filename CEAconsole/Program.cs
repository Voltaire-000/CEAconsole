

// See https://aka.ms/new-console-template for more information
using CEAconsole.Models;
using CEAconsole.Services;
using MathNet.Numerics;
using MathNet.Numerics.Integration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScottPlot;

// Services Section
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
List<JToken> integrationConstantList = [.. integrationConstants[0]];
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
double increment = 25;
int digits = 3;

for (Kelvin = startTemp; Kelvin <= endTemp; Kelvin += increment)
{
    temperatureList.Add(Kelvin);
    double cp_value = GetHeatCapacity(Kelvin);
    heatCapacityList.Add(cp_value);
    //double enthalpy_value = GetEnthalpy(Kelvin);
    double enthalpy_value = CalcEnthalpyChange(startTemp, temperatureList.Last<double>());
    double entropy_value = GetEntropy(Kelvin);
    
    if (Kelvin == 998.15)
    {
        double lastCp_value = GetHeatCapacity(endTemp);
        heatCapacityList.Add(lastCp_value);
    }
    enthalpyList.Add(Math.Round(enthalpy_value, digits));
    if (Kelvin == 998.15)
    {
        double lastEnthalpy_value = GetEnthalpy(endTemp);
        enthalpyList.Add(lastEnthalpy_value);
    }
    entropyList.Add(entropy_value);
    if (Kelvin == 998.15)
    {
        double lastEntropy_value = GetEntropy(endTemp);
        entropyList.Add(lastEntropy_value);
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
    double mIndex = 0;
    int listCount = heatCapacityList.Count;

    if (listCount > 10)
    {
        mIndex = heatCapacityList[listCount - 10] / 1000; 
    }
    else
    {
        mIndex = heatCapacityList[listCount - 2] / 1000;
    }

    int numberOfPartitions = 20;
    //double enthalpyChange = Integrate.OnClosedInterval(x => heatCapacityList[listCount - 2] / 1000, initialTemp, finalTemp, 1e-8);
    //double enthalpyChange = SimpsonRule.IntegrateComposite(x => mIndex, initialTemp,finalTemp, numberOfPartitions);
    double enthalpyChange = NewtonCotesTrapeziumRule.IntegrateComposite(x => mIndex, initialTemp, finalTemp, numberOfPartitions);
    return enthalpyChange;
}

temperatureList.Add(endTemp);
int recordsLength = temperatureList.Count;
int skipPrint = 10;

ScottPlot.Plot plot = new();
double[] dataX = { 1, 2, 3, 4, 5 };
double[] dataY = { 1, 4, 9, 16, 25 };
plot.Add.Scatter( dataX, dataY);


ScottPlot.Plot lineChart = new();
Coordinates mstart = new Coordinates();
mstart.X = temperatureList[0];
mstart.Y = heatCapacityList[0];

ScottPlot.Plot enthalpyPlot = new();
double[] KelvinTemps = temperatureList.ToArray();
double[] EnthalpyValues = enthalpyList.ToArray();

//enthalpyPlot.Add.Scatter(KelvinTemps, EnthalpyValues);
enthalpyPlot.SavePng("enthalpy.png", 1600, 1200);

//Coordinates mend = new Coordinates();
//mend.X = temperatureList[30];
//mend.Y = heatCapacityList[30];
//lineChart.Add.Line(mstart, mend);

//plot.Add.Line(mstart, mend);
//lineChart.SavePng("enthalpy.png", 1600, 1200);

//int mtest = 99;

for (int i = 0; i < recordsLength; i++)
{
        Console.WriteLine("{0, -10} {1, -10} {2, -10} {3, -10}", "\t" 
                + temperatureList.ElementAt(i) + " :", "\t" 
                + heatCapacityList.ElementAt(i).Round(digits), enthalpyList.ElementAt(i).Round(digits), entropyList.ElementAt(i).Round(digits));
}

string inputCard = InputCardService.GetInputCard();
string[]? Prod = ProdFilter.ExtractProducts(inputCard);

Console.WriteLine("\nInput Card : " + inputCard);

Console.WriteLine("\nOnly List :");
for (int i = 0; i < Prod.Length; i++)
{
    Console.WriteLine("\t" + Prod[i]); 
}


