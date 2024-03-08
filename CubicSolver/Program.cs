// See https://aka.ms/new-console-template for more information
using MathNet.Numerics.Integration;
using MathNet.Numerics.RootFinding;
using System.Text.RegularExpressions;
using System.Numerics;
using MathNet.Symbolics;
using MathNet.Numerics;

// TODO derivatives, initial guess
// TODO integration constants
// for loop to create formula
// fix Hmol line

double result = SimpsonRule.IntegrateComposite(x => x * x, 0.0, 10.0, 4);

// f(x) = ax^3 + bx^2 + cx + d ;
// x^3 + -6x^2 + 11x + -6 = 0;
// x^3 + (-6x^2) + 11x + (-6) = 0

double a = 1;
double b = -6;
double c = 11;
double d = -6;

var roots = Cubic.RealRoots(d, c, b);
Console.WriteLine("\nusing Cubic.RealRoots :" + roots); // 3,2,1

(Complex root1, Complex root2, Complex root3) = Cubic.Roots(d, c, b, a);
Console.WriteLine("\nusing complex root : " + root1.Real + " " + root2.Real + " " + root3.Real); // 1,3,2

//var roots = System.Numerics.Complex.
//Func<double, double> f = x => Math.Exp(-x) - x;
//Func<double, double> f = x => Math.Pow(x, 3) + b * Math.Pow(x, 2) + c * x + d;
//Func<double, double> f = x => x * Math.Pow(x, 3) + Math.Pow(x, 2) - 4 * x - 4;
//Func<double, double> f = x => x* Math.Pow(x, 3) - (6 * Math.Pow(x, 2)) + 11 * x + (-6);

Func<double, double> f = x => a * Math.Pow(x, 3) + b * Math.Pow(x, 2) + c * x + d;
// define the derivative of the cubic function
Func<double, double> df = x => 3 * a * Math.Pow(x, 2) + 2 * b * x + c;
double lowerBound = -6;
double upperBound = 11;
double accuracy = 1e-6;
int maxIterations = 100;
double root = Bisection.FindRoot(f, lowerBound, upperBound, accuracy, maxIterations);
Console.WriteLine("\nBisection.FindRoot : " + root.Round(1)); // 3

double expandFactor = 1.6;
double expandRoot = Bisection.FindRootExpand(f, lowerBound, upperBound, accuracy, maxIterations, expandFactor);
Console.WriteLine("\nBisection.FindRootExpand : " + expandRoot.Round(1)); // 3

double mTryRoot;
bool tryRoot = Bisection.TryFindRoot(f, lowerBound, upperBound, accuracy, maxIterations, out mTryRoot);
Console.WriteLine("\nTryFindRoot : " + mTryRoot.Round(1)); // 3

double mNewton = NewtonRaphson.FindRoot(f, df, lowerBound, upperBound, accuracy, maxIterations);
Console.WriteLine("\nNewtonRaphson : " + mNewton.Round(1)); // 1

string equation = "f(x) = ax^3 + bx^2 + cx + d";
ParseEquation(equation);

static void ParseEquation(string equation)
{
    //var match = Regex.Match(equation, @"f\(x\) = (?<a>.+)x\^3 \+ (? <b>.+)x\^2 \+ (?<c>.+)x \+ (?<d>.+)");
    var match = Regex.Match(equation,   @"f\(x\) = (?<a>.+)x\^3 \+ (?<b>.+)x\^2 \+ (?<c>.+)x \+ (?<d>.+)");

    if (match.Success)
    {
        string a = match.Groups["a"].Value;
        string b = match.Groups["b"].Value;
        string c = match.Groups["c"].Value;
        string d = match.Groups["d"].Value;

        Console.WriteLine($"\na: { a}, b: { b}, c: { c}, d: { d}");

        // Now you can use these coefficients with MathNet as needed
        // For example, to create a symbolic expression:
        var x = SymbolicExpression.Variable("x");
        //var cubicExpression = SymbolicExpression.Parse($"{a} *x ^ 3 + {b}" + $" *x ^ 2 + {c} *x + {d}");
        var cubicExpression = a  + "x^3" + " + " + b + "x^2" + " + " + c + "*x" + " + " + d;
        Console.WriteLine($"\nExpression: {cubicExpression}");
    }
    else
    {
        Console.WriteLine("\nThe equation format is incorrect.");
    }
}

SymbolicExpression a_1 = SymbolicExpression.Variable("a_1");
SymbolicExpression a_2 = SymbolicExpression.Variable("a_2");
SymbolicExpression a_3 = SymbolicExpression.Variable("a_3");
SymbolicExpression a_4 = SymbolicExpression.Variable("a_4");
SymbolicExpression a_5 = SymbolicExpression.Variable("a_5");
SymbolicExpression a_6 = SymbolicExpression.Variable("a_6");
SymbolicExpression a_7 = SymbolicExpression.Variable("a_7");
SymbolicExpression a_8 = SymbolicExpression.Variable("a_8");
SymbolicExpression T = SymbolicExpression.Variable("T");
SymbolicExpression Cp = SymbolicExpression.Variable("Cp");
SymbolicExpression R = SymbolicExpression.Variable("R");

Console.WriteLine("\nThe Integral of x^2 from 0 to 10 is : " + result);
