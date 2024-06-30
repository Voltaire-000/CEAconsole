using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CEAconsole.ThermoChemistry
{
    public class NewtonRaphsonWithLineSearch
    {
        public static MathNet.Numerics.LinearAlgebra.Vector<double> Minimize(
            MathNet.Numerics.LinearAlgebra.Vector<double> initialGuess, 
            Func<MathNet.Numerics.LinearAlgebra.Vector<double>, MathNet.Numerics.LinearAlgebra.Vector<double>> gradient,
            Func<MathNet.Numerics.LinearAlgebra.Vector<double>, double> objectiveFunction, double tolerance = 1e-6, int maxIterations = 100)
        {
            MathNet.Numerics.LinearAlgebra.Vector<double> x = initialGuess;
            int iterations = 0;
            while (iterations < maxIterations)
            {
                //  Compute the Gradient at the current iterate
                MathNet.Numerics.LinearAlgebra.Vector<double> g = gradient(x);
                //  Compute the Hessian at the current iterate
                Matrix<double> H = ComputeHessian(x, gradient);
                //  Compute the Newton direction
                MathNet.Numerics.LinearAlgebra.Vector<double> d = -H.Solve(g);
                // Perform a line search to find the step size
                double alpha = LineSearch(x, g, d, objectiveFunction);
                //  Update the iterate
                x += alpha * d;
                //  Check for convergence
                if (g.L2Norm() < tolerance)
                {
                    break;
                }
                iterations++;
            }
            return x;
        }

        private static double LineSearch(
            MathNet.Numerics.LinearAlgebra.Vector<double> x, 
            MathNet.Numerics.LinearAlgebra.Vector<double> g, 
            MathNet.Numerics.LinearAlgebra.Vector<double> d, 
            Func<MathNet.Numerics.LinearAlgebra.Vector<double>, 
            double> objectiveFunction)
        {
            //  Initial step size
            double alpha = 1.0;
            //  Sufficient decrease parameter
            double gamma = 0.5;

            double fx = objectiveFunction(x);
            double dfx = g.DotProduct(d);
            while (true)
            {
                MathNet.Numerics.LinearAlgebra.Vector<double> xNew = x + alpha * d;
                double fxNew = objectiveFunction(xNew);
                if (fxNew <= fx + gamma * alpha * dfx)
                {
                    return alpha;
                }
                // this may be wrong, Groq code has beta
                alpha *= gamma;
            }
        }

        private static Matrix<double> ComputeHessian(
            MathNet.Numerics.LinearAlgebra.Vector<double> x, 
            Func<MathNet.Numerics.LinearAlgebra.Vector<double>, 
            MathNet.Numerics.LinearAlgebra.Vector<double>> gradient)
        {
            // Compute the Hessian using finite difference
            int n = x.Count;
            Matrix<double> H = Matrix<double>.Build.Dense(n, n);
            for (int i = 0; i < n; i++)
            {
                MathNet.Numerics.LinearAlgebra.Vector<double> ei = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(n);
                ei[i] = 1.0;
                MathNet.Numerics.LinearAlgebra.Vector<double> giPlus = gradient(x + ei);
                MathNet.Numerics.LinearAlgebra.Vector<double> giMinus = gradient(x - ei);
                for (int j = 0; j < n; j++)
                {
                    H[i, j] = (giPlus[j] - giMinus[j]) / 2.0;
                }
            }
            return H;
        }


    }
}
