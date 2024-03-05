// See https://aka.ms/new-console-template for more information
using MathNet.Numerics.Integration;
using MathNet.Numerics.RootFinding;
using System.Text.RegularExpressions;
using System.Numerics;
using MathNet.Symbolics;


double result = SimpsonRule.IntegrateComposite(x => x * x, 0.0, 10.0, 4);

// f(x) = ax^3 + bx^2 + cx + d ;
// x^3 + -6x^2 + 11x + -6 = 0;
// x^3 + (-6x^2) + 11x + (-6) = 0

double a = 1;
double b = -6;
double c = 11;
double d = -6;

var roots = Cubic.RealRoots(d, c, b);
(Complex root1, Complex root2, Complex root3) = Cubic.Roots(d, c, b, a);
//var roots = System.Numerics.Complex.
//Func<double, double> f = x => Math.Exp(-x) - x;
//Func<double, double> f = x => Math.Pow(x, 3) + b * Math.Pow(x, 2) + c * x + d;
//Func<double, double> f = x => x * Math.Pow(x, 3) + Math.Pow(x, 2) - 4 * x - 4;
//Func<double, double> f = x => x* Math.Pow(x, 3) - (6 * Math.Pow(x, 2)) + 11 * x + (-6);
Func<double, double> f = x => a * Math.Pow(x, 3) + b * Math.Pow(x, 2) + c * x + d;
// define the derivative of the cubic function
Func<double, double> df = x => 3 * a * Math.Pow(x, 2) + 2 * b * x + c;
double lowerBound = -6;
double upperBound = 5;
double accuracy = 1e-6;
int maxIterations = 100;
double root = Bisection.FindRoot(f, lowerBound, upperBound, accuracy, maxIterations);
double expandFactor = 1.6;
double expandRoot = Bisection.FindRootExpand(f, lowerBound, upperBound, accuracy, maxIterations, expandFactor);
double mRoot = NewtonRaphson.FindRoot(f, df, lowerBound, upperBound, accuracy, maxIterations);
//(double root1, double root2, double root3) = Cubic.RealRoots(d, c, b);
//(MathNet.Numerics.LinearAlgebra.Complex root1, Complex root2, Complex root3) = MathNet.Numerics.RootFinding.Bisection(d, c, b);

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

        Console.WriteLine($"a: { a}, b: { b}, c: { c}, d: { d}");

        // Now you can use these coefficients with MathNet as needed
        // For example, to create a symbolic expression:
        var x = SymbolicExpression.Variable("x");
        //var cubicExpression = SymbolicExpression.Parse($"{a} *x ^ 3 + {b}" + $" *x ^ 2 + {c} *x + {d}");
        var cubicExpression = a  + "x^3" + " + " + b + "x^2" + " + " + c + "*x" + " + " + d;
        Console.WriteLine($"Expression: {cubicExpression}");
    }
    else
    {
        Console.WriteLine("The equation format is incorrect.");
    }
}

Console.WriteLine("The Integral of x^2 from 0 to 10 is : " + result);
