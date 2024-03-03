

// See https://aka.ms/new-console-template for more information
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
//Console.WriteLine(json);

//Console.WriteLine("Hello, World!");
