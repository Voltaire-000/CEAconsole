using Accord.Math.Optimization;
using CEAconsole.Models;
using MathNet.Numerics.Integration;
using ConstraintType = Accord.Math.Optimization.ConstraintType;

namespace CEAconsole.Thermo
{
    public static class Gibbs
    {
        public static double Gibbs_RefH298_T(double ReferenceTemperature, double ReferenceEntropy, double Temperature, IEnumerable<Specie> Specie)
        {
            double enthalpyIntegrand(double T) => HeatCapacity.Cp(T, Specie);
            double entropyIntegrand(double T) => HeatCapacity.Cp(T, Specie) / T;
            double enthalpy = GaussKronrodRule.Integrate(enthalpyIntegrand, ReferenceTemperature, Temperature, out double error, out double L1Norm, 1e-8) / 1000;
            double entropy = GaussKronrodRule.Integrate(entropyIntegrand, ReferenceTemperature, Temperature, out error, out L1Norm, 1e-8);
            entropy += ReferenceEntropy;

            double gibbs = -(enthalpy * 1000 - Temperature * entropy) / Temperature;
            // TODO not what i want 
            if (double.IsNegativeInfinity(gibbs))
            {
                return gibbs = 0.0;
            }
            return gibbs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Temperature"></param>
        /// <param name="DeltaHrxn"></param>
        /// <param name="DeltaSrxn"></param>
        /// <returns></returns>
        public static double DeltaGibbsRxn(double Temperature, double DeltaHrxn, double DeltaSrxn)
        {
            return DeltaHrxn - (Temperature * (DeltaSrxn / 1000));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Temperature"></param>
        /// <param name="Molecule"></param>
        /// <returns></returns>
        public static double DeltaGibbsRxn(double Temperature, string Molecule)
        {
            throw new NotImplementedException();
        }

        public static double[] GibbsMinimizer(double[] ProductsDeltaGibbsRxn)
        {
            int numberOfVariables = ProductsDeltaGibbsRxn.Length;

            NonlinearObjectiveFunction objFunction = CreateObjFunction(numberOfVariables, ObjectiveFunction, Gradient);

            double[] Gradient(double[] x)
            {
                if (x.Length != ProductsDeltaGibbsRxn.Length)
                {
                    throw new ArgumentException("Gradiest function : Input array lengths do not match");
                }
                double[] gradient = new double[x.Length];
                for (int i = 0; i < x.Length; i++)
                {
                    gradient[i] = ProductsDeltaGibbsRxn[i];
                }
                return gradient;
            }

            double ObjectiveFunction(double[] x)
            {
                try
                {
                    if (x.Length != ProductsDeltaGibbsRxn.Length)
                    {
                        throw new ArgumentException("ObjectiveFunction : Input array lengths do not match");
                    }
                    double sum = 0;
                    for (int i = 0; i < x.Length; i++)
                    {
                        sum += x[i] * (i == 0 ? ProductsDeltaGibbsRxn[i] : ProductsDeltaGibbsRxn[i]);
                    }

                    return sum;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return double.NaN;
                }
            }

            List<NonlinearConstraint> constraints = CreateConstraints();

            List<NonlinearConstraint> CreateConstraints()
            {
                List<NonlinearConstraint> constraints = new List<NonlinearConstraint>();
                // Carbon Balance
                constraints.Add(new NonlinearConstraint
                    (
                    numberOfVariables: 2,
                    function: (x) => x[0] - 1,
                    shouldBe: ConstraintType.EqualTo,
                    value: 0,
                    CarbonGradient));
                // Hydrogen Balance
                constraints.Add(new NonlinearConstraint
                    (
                    numberOfVariables: 2,
                    function: (x) => 2 * x[1] - 4,
                    shouldBe: ConstraintType.EqualTo,
                    value: 0,
                    gradient: HydrogenGradient));
                // Oxygen Balance
                constraints.Add(new NonlinearConstraint
                    (
                    numberOfVariables: 2,
                    function: (x) => 2 * x[0] + x[1] - 4,
                    shouldBe: ConstraintType.EqualTo,
                    value: 0,
                    gradient: OxygenGradient));
                return constraints;
            }

            double[] OxygenGradient(double[] x)
            {
                return new double[] { 2, 1 };
            }

            double[] HydrogenGradient(double[] x)
            {
                return new double[] { 0, 2 };
            }

            double[] CarbonGradient(double[] x)
            {
                return new double[] { 1, 0 };
            }

            AugmentedLagrangian solver = new AugmentedLagrangian(objFunction, constraints);
            // set initial guess
            double[] initialGuess = [0.5, 0.5];
            solver.Solution = initialGuess;
            // solve the problem
            bool success = solver.Minimize();
            double[] solution = solver.Solution;
            double m_sum = solution.Sum();
            double[] m_fractions = new double[solution.Length];
            for (int i = 0; i < solution.Length; i++)
            {
                m_fractions[i] = solution[i] / m_sum;
            }
            return m_fractions;
        }

        private static NonlinearObjectiveFunction CreateObjFunction(int numberOfVariables, Func<double[], double> objectiveFunction, Func<double[], double[]> gradient)
        {
            return new NonlinearObjectiveFunction(numberOfVariables, objectiveFunction, gradient);
        }
    }
}
