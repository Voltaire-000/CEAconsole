

// See https://aka.ms/new-console-template for more information
using CEAconsole;
using CEAconsole.Models;
using CEAconsole.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using MathNet.Numerics;
using MathNet.Symbolics;


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

// list of temperature exponents : 8 exponents
var TExponents = mChildren[2].ToList();
var tempExponentsList = TExponents[0];
double texp_1 = tempExponentsList[0].ToObject<double>();
double texp_2 = tempExponentsList[1].ToObject<double>();
double texp_3 = tempExponentsList[2].ToObject<double>();
double texp_4 = tempExponentsList[3].ToObject<double>();
double texp_5 = tempExponentsList[4].ToObject<double>();
double texp_6 = tempExponentsList[5].ToObject<double>();
double texp_7 = tempExponentsList[6].ToObject<double>();
double texp_8 = tempExponentsList[7].ToObject<double>();

SymbolicExpression a_1 = SymbolicExpression.Variable("a_1");
SymbolicExpression a_2 = SymbolicExpression.Variable("a_2");
SymbolicExpression a_3 = SymbolicExpression.Variable("a_3");
SymbolicExpression a_4 = SymbolicExpression.Variable("a_4");
SymbolicExpression a_5 = SymbolicExpression.Variable("a_5");
SymbolicExpression a_6 = SymbolicExpression.Variable("a_6");
SymbolicExpression a_7 = SymbolicExpression.Variable("a_7");
//SymbolicExpression a_8 = SymbolicExpression.Variable("a_8");
SymbolicExpression T = SymbolicExpression.Variable("T");
SymbolicExpression Cp = SymbolicExpression.Variable("Cp");
SymbolicExpression R = SymbolicExpression.Variable("R");
SymbolicExpression H = SymbolicExpression.Variable("H");
SymbolicExpression S = SymbolicExpression.Variable("S");

// list of coefficients
var Coefficients = mChildren[4].ToList();
var coefficientsList = Coefficients[0].ToList();

a_1 = coefficientsList[0].ToObject<double>();
a_2 = coefficientsList[1].ToObject<double>();
a_3 = coefficientsList[2].ToObject<double>();
a_4 = coefficientsList[3].ToObject<double>();
a_5 = coefficientsList[4].ToObject<double>();
a_6 = coefficientsList[5].ToObject<double>();
a_7 = coefficientsList[6].ToObject<double>();

// list of integration constants
var integrationConstants = mChildren[5].ToList();
var integrationConstantList = integrationConstants[0].ToList();
double a_8 = integrationConstantList[0].ToObject<double>();
double a_9 = integrationConstantList[1].ToObject<double>();

//var filterResult = reactants.Where(d => d.Name == "CH4");
//var mtest = reactants.Where(d => d.Name == "CH4");
//List<Element>? elements = new(elementsService.GetElements());
//string json = JsonConvert.SerializeObject(elements, Formatting.Indented);

// this needs fixing thru the class
//List<TransportProperty>? transportProperties = new(transportService.GetTransportProperties());
//string json = JsonConvert.SerializeObject(transportProperties, Formatting.Indented);



double start = 298.15;
double end = 1000;
double increment = 100;
double mNaturalLog = Math.Log(1000);
double temperature = 0;
T = 0;
R = 8.31446261815324;
List<double> kelvins = new List<double>();
List<double> cpData = new List<double>();
List<double> hData = new List<double>();
List<double> sData = new List<double>();
Console.WriteLine("{0, -16} {1, -10} {2, -10} {3, -10}" , "\tTemp Kelvin", "Cp", "H", "S");
for (temperature = start; temperature <= end; temperature += increment)
{
    kelvins.Add(temperature);
    double loopCp = GetCp(temperature);
    double loopH = GetH(temperature);
    double loopS = GetS(temperature);
    cpData.Add(Math.Round(loopCp, 3));
    if (temperature == 998.15)
    {
        double lastValue = GetCp(end);
        cpData.Add(Math.Round(lastValue, 3));

    }
    hData.Add(Math.Round(loopH, 3));
    if (temperature == 998.15)
    {
        double lastValue = GetH(end);
        hData.Add(Math.Round(lastValue, 3));

    }
    sData.Add(Math.Round(loopS, 3));
    if (temperature == 998.15)
    {
        double lastValue = GetS(end);
        sData.Add(Math.Round(lastValue, 3));

    }


    
    //Console.WriteLine("{0, -10} {1, -10} {2, -10} {3, -10}" , "\t" + temperature + " :", "\t"+ cpData.Last(), hData.Last(), sData.Last());
}
    kelvins.Add(end);
    for (int i = 0; i < 9; i++)
    {
        Console.WriteLine("{0, -10} {1, -10} {2, -10} {3, -10}", "\t" + kelvins.ElementAt(i) + " :", "\t" + cpData.ElementAt(i), hData.ElementAt(i), sData.ElementAt(i));
    }


double GetS(double temperature)
{
    T = temperature;
    S = R * (-a_1
         * T.Pow(texp_1)
         - a_2
         * T.Pow(texp_2)
         + a_3
         * Math.Log(1000)
         + a_4
         * T
         + a_5
         * T.Pow(texp_5).Divide(2)
         + a_6
         * T.Pow(texp_6).Divide(3)
         + a_7
         * T.Pow(texp_7).Divide(4)
         + a_9);
    return S.RealNumberValue;
}

double GetH(double temperature)
{
    T = temperature;
    H = R * T * -(a_1
             * T.Pow(texp_1)
             + a_2
             * T.Pow(texp_2)
             * Math.Log(1000)
             + a_3
             + a_4
             * T.Divide(2)
             + a_5
             * T.Pow(texp_5).Divide(3)
             + a_6
             * T.Pow(texp_6).Divide(4)
             + a_7
             * T.Pow(texp_7).Divide(5)
             + a_8
             / T);
    H /= 1000;
    return H.RealNumberValue;
}

double GetCp(double temperature)
{
    T = temperature;
    Cp = R * (a_1
          * T.Pow(texp_1)
          + a_2
          * T.Pow(texp_2)
          + a_3
          + a_4
          * T
          + a_5
          * T.Pow(texp_5)
          + a_6
          * T.Pow(texp_6)
          + a_7
          * T.Pow(texp_7));
    return Cp.RealNumberValue;
}

Cp = R * (a_1
          * T.Pow(texp_1)
          + a_2
          * T.Pow(texp_2)
          + a_3
          + a_4
          * T
          + a_5
          * T.Pow(texp_5)
          + a_6
          * T.Pow(texp_6)
          + a_7
          * T.Pow(texp_7));
// Cp = 73.67604083
//      73.676040830041

var nxnx = Math.Log(T.RealNumberValue);

H = R * T * -(a_1
             * T.Pow(texp_1)
             + a_2
             * T.Pow(texp_2)
             * Math.Log(1000)
             + a_3
             + a_4
             * T.Divide(2)
             + a_5
             * T.Pow(texp_5).Divide(3)
             + a_6
             * T.Pow(texp_6).Divide(4)
             + a_7
             * T.Pow(texp_7).Divide(5)
             + a_8
             / T);
H /= 1000;
// H = 38.685
// -35.915
// -35915.212164060285

S = R * (-a_1
         * T.Pow(texp_1)
         - a_2
         * T.Pow(texp_2)
         + a_3
         * Math.Log(1000)
         + a_4
         * T
         + a_5
         * T.Pow(texp_5).Divide(2)
         + a_6
         * T.Pow(texp_6).Divide(3)
         + a_7
         * T.Pow(texp_7).Divide(4)
         + a_9);
// 248.331
// 249.06403282791564

//"temperatureRange": [ 200.000, 1000.000 ],
//                "numberOfCoefficients": 7,
//                "tExponents": [ -2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0 ],
//                "H^(298.15)-H^(0) J/mol": 10016.202,
//                "coefficients": [ -1.766850998e+05, 2.786181020e+03, -1.202577850e+01, 3.917619290e-02, -3.619054430e-05, 2.026853043e-08, -4.976705490e-12 ],
//                "integrationConstants": [ -2.331314360e+04, 8.904322750e+01 ]

//List<double> cpData = [Cp.RealNumberValue, H.RealNumberValue, S.RealNumberValue];

//Console.WriteLine("\nHeat capacity : " + Cp.RealNumberValue);
//Console.WriteLine("\nEnthalpy : " + H.RealNumberValue);
//Console.WriteLine("\nEntropy : " + S.RealNumberValue);

//Console.WriteLine("{0, -16} {1, -16} {2, -16}", "\tCp", "H", "S");

//Console.WriteLine("{0, -16} {1, -16} {2, -16}", cpData[0], cpData[1], cpData[2]);

//Console.WriteLine(propertiesOfInputFilter);


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


