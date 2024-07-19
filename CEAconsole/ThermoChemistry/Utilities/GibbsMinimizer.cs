using Accord.Math.Optimization;

namespace CEAconsole.ThermoChemistry.Utilities
{
    public static class GibbsMinimizer
    {
        private static NonlinearObjectiveFunction CreateObjFunction(int NumberOfVariables, Func<double[], double> ObjectiveFunction, Func<double[], double[]> Gradient)
        {
            return new NonlinearObjectiveFunction(NumberOfVariables, ObjectiveFunction, Gradient);
        }

        public static double[] GibbsMinimizerInitial()
        {
            //  Gibbs energies
            //private const double G_CO2 = -394.4;
            double G_CO2 = -394.374;
            //private const double G_H2O = -237.1;
            double G_H2O = -255.333;
            int numberOfVariables = 2;

            NonlinearObjectiveFunction objFunction = CreateObjFunction(numberOfVariables, ObjectiveFunction, Gradient);

            double[] Gradient(double[] x)
            {
                return new double[]
                {
                    // @G/@x, @G/@y
                    G_CO2,
                    G_H2O
                };
            }

            double ObjectiveFunction(double[] x)
            {
                double xCO2 = x[0];
                double xH2O = x[1];
                return xCO2 * G_CO2 + xH2O * G_H2O;
            }

            var constraints = CreateConstraints();

            List<NonlinearConstraint> CreateConstraints()
            {
                var constraints = new List<NonlinearConstraint>();
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
            var solution = solver.Solution;
            return solution;
        }

        public static double[] GibbsMin(double[] ProductsDeltaGibbsRxn)
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
                m_fractions[i] = solution[i]/ m_sum;
            }
            return m_fractions;
        }
    }
}
