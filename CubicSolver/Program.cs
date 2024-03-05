// See https://aka.ms/new-console-template for more information
using MathNet.Numerics.Integration;
using MathNet.Numerics.RootFinding;
//using MathNet.Numerics.LinearAlgebra.Complex;
using System.Numerics;


double result = SimpsonRule.IntegrateComposite(x => x * x, 0.0, 10.0, 4);

// f(x) = ax^3 + bx^2 + cx + d ;
// x^3 + -6x^2 + 11x + -6 = 0;

double a = 1;
double b = -6;
double c = 11;
double d = -6;

var roots = Cubic.RealRoots(d, c, b);
(Complex root1, Complex root2, Complex root3) = Cubic.Roots(d, c, b, a);
//var roots = System.Numerics.Complex.
//Func<double, double> f = x => Math.Exp(-x) - x;
//Func<double, double> f = x => Math.Pow(x, 3) + b * Math.Pow(x, 2) + c * x + d;
Func<double, double> f = x => x * Math.Pow(x, 3) + Math.Pow(x, 2) - 4 * x - 4;
double lowerBound = -6;
double upperBound = 11;
double accuracy = 1e-6;
int maxIterations = 100;
double root = Bisection.FindRoot(f, lowerBound, upperBound, accuracy, maxIterations);
double expandFactor = 1.6;
double expandRoot = Bisection.FindRootExpand(f, lowerBound, upperBound, accuracy, maxIterations, expandFactor);
double mRoot = NewtonRaphson.FindRoot(f, )


//(double root1, double root2, double root3) = Cubic.RealRoots(d, c, b);
//(MathNet.Numerics.LinearAlgebra.Complex root1, Complex root2, Complex root3) = MathNet.Numerics.RootFinding.Bisection(d, c, b);

Console.WriteLine("The Integral of x^2 from 0 to 10 is : " + result);

//Console.WriteLine("Hello, Math Cubic Solver!");
