

// See https://aka.ms/new-console-template for more information
using CEAconsole;
using CEAconsole.Models;
using CEAconsole.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;

ThermoService thermoService = new();
//ElementsService elementsService = new();
//TransportService transportService = new();

List<Reactant>? reactants = new(thermoService.GetReactants());
string json = JsonConvert.SerializeObject(reactants, Formatting.Indented);

JArray parsedJson = JArray.Parse(json);
// filter the object based on the conditions
JArray filteredReactants = new(parsedJson.Where(r => r["Name"].ToString() == "CH4"));
// convert back to a JSON string
string propertiesOfInputFilter = JsonConvert.SerializeObject(filteredReactants, Formatting.Indented);

JArray reactantProperties = JArray.Parse(propertiesOfInputFilter);
string? name = reactantProperties[0]?.SelectToken("Name")?.ToString();

JObject chemicalFormula = (JObject)reactantProperties[0].SelectToken("ChemicalFormula");
foreach (var element in chemicalFormula)
{
    string elementName = element.Key;
    int? elementValue = element.Value?.ToObject<int>();
    Console.WriteLine($"Element: {elementName}, Value: {elementValue}");
}

// get number of T_intervals
string mT_Intervals = reactantProperties[0].SelectToken("T_intervals").ToString();
dynamic parsedT_Intervals = JsonConvert.DeserializeObject(mT_Intervals);
int mNumberOfTempIntervals = (int)parsedT_Intervals;

// get temperature range_1
List<JToken>? temperatureRange = reactantProperties[0]?.SelectToken("TemperatureRange")?.ToList();
List<JToken> tempRange_1 = temperatureRange[0].ToList();
List<JToken> mChildren = tempRange_1.Children().ToList();
var childTempRange = mChildren[0].ToList();
// number of coefficients
var childNumberOfCoefficients = mChildren[1];
int numberOfCoefficients = childNumberOfCoefficients.ToObject<int>();

var TExponents = mChildren[2].ToList();
var tempExponentsList = TExponents[0];
double? t_1 = tempExponentsList[0]?.ToObject<double>();
double t_2 = tempExponentsList[1].ToObject<double>();
double t_3 = tempExponentsList[2].ToObject<double>();
double t_4 = tempExponentsList[3].ToObject<double>();
double t_5 = tempExponentsList[4].ToObject<double>();
double t_6 = tempExponentsList[5].ToObject<double>();
double t_7 = tempExponentsList[6].ToObject<double>();
double t_8 = tempExponentsList[7].ToObject<double>();

for (int i = 0; i < tempExponentsList.Count(); i++)
{
    double? tExp = tempExponentsList[i]?.ToObject<double>();
}

// list of coefficients
var Coefficients = mChildren[4].ToList();
var coefficientsList = Coefficients[0].ToList();
for (int i = 0; i < coefficientsList.Count(); i++)
{
    double? coefficient = coefficientsList[i]?.ToObject<double>();
}


//var filterResult = reactants.Where(d => d.Name == "CH4");
//var mtest = reactants.Where(d => d.Name == "CH4");
//List<Element>? elements = new(elementsService.GetElements());
//string json = JsonConvert.SerializeObject(elements, Formatting.Indented);

// this needs fixing thru the class
//List<TransportProperty>? transportProperties = new(transportService.GetTransportProperties());
//string json = JsonConvert.SerializeObject(transportProperties, Formatting.Indented);


Console.WriteLine(propertiesOfInputFilter);


int mtxet = 99;

//Console.WriteLine(BalancedEquation(1, 4)); // CH4 + 2O2 -> CO2 + 2H2O
//Combustion combustion = new(1, 4);
//combustion.BalanceEquation();
//Console.WriteLine(BalancedEquation(2, 6)); // C2H6 + 7/202 -> 2CO2 + 3H2O
//Console.WriteLine(BalancedEquation(3, 8)); // C3H8 + 5O2 -> 3CO2 + 4H2O
//Console.WriteLine(BalancedEquation(4, 10)); // C4H10 + 13/2O2 -> 4CO2 + 5H2O

//Console.WriteLine(json);
//static string BalancedEquation(int X, int Y)
//{
//    // calculate the coefficients of oxygen, carbon dioxide, and water
//    int O2 = (X + Y) / 4;
//    int CO2 = X;
//    int H2O = Y / 2;

//    // check if the coefficients are integers, if not multiply them by 2
//    //if ((X + Y) % 4 != 0)
//    //{
//    //    O2 = O2 * 2;
//    //    CO2 = CO2 * 2;
//    //    H2O = H2O * 2;
//    //}

//    // format the equation as a string
//    string equation = $"C{X}H{Y} + {O2}O2 -> {CO2}CO2 + {H2O}H2O";
//    // return the equation
//    return equation;
////}


