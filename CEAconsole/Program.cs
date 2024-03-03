

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
JArray filteredReactants = new JArray(parsedJson.Where(r => r["Name"].ToString() == "CH4"));
// convert back to a JSON string
string filteredJson = JsonConvert.SerializeObject(filteredReactants, Formatting.Indented);

JArray mParsedJson = JArray.Parse(filteredJson);
string name = mParsedJson[0].SelectToken("Name").ToString();

JObject chemicalFormula = (JObject)mParsedJson[0].SelectToken("ChemicalFormula");
foreach (var element in chemicalFormula)
{
    string elementName = element.Key;
    int elementValue = element.Value.ToObject<int>();
    Console.WriteLine($"Element: {elementName}, Value: {elementValue}");
}

//var filterResult = reactants.Where(d => d.Name == "CH4");
var mtest = reactants.Where(d => d.Name == "CH4");
//List<Element>? elements = new(elementsService.GetElements());
//string json = JsonConvert.SerializeObject(elements, Formatting.Indented);

// this needs fixing thru the class
//List<TransportProperty>? transportProperties = new(transportService.GetTransportProperties());
//string json = JsonConvert.SerializeObject(transportProperties, Formatting.Indented);

Console.WriteLine(filteredJson);
Console.WriteLine(BalancedEquation(1, 4)); // CH4 + 2O2 -> CO2 + 2H2O
Combustion combustion = new Combustion(1, 4);
combustion.BalanceEquation();
Console.WriteLine(BalancedEquation(2, 6)); // C2H6 + 7/202 -> 2CO2 + 3H2O
Console.WriteLine(BalancedEquation(3, 8)); // C3H8 + 5O2 -> 3CO2 + 4H2O
Console.WriteLine(BalancedEquation(4, 10)); // C4H10 + 13/2O2 -> 4CO2 + 5H2O

//Console.WriteLine(json);
static string BalancedEquation(int X, int Y)
{
    // calculate the coefficients of oxygen, carbon dioxide, and water
    int O2 = (X + Y) / 4;
    int CO2 = X;
    int H2O = Y / 2;

    // check if the coefficients are integers, if not multiply them by 2
    //if ((X + Y) % 4 != 0)
    //{
    //    O2 = O2 * 2;
    //    CO2 = CO2 * 2;
    //    H2O = H2O * 2;
    //}

    // format the equation as a string
    string equation = $"C{X}H{Y} + {O2}O2 -> {CO2}CO2 + {H2O}H2O";
    // return the equation
    return equation;
}


