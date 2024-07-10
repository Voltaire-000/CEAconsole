using Accord.Math.Optimization;

namespace GibbsMin
{
    public class GibbsMinimizer
    {
        //  Gibbs energies
        //private const double G_CO2 = -394.4;
        private const double G_CO2 = -394.374;
        //private const double G_H2O = -237.1;
        private const double G_H2O = -255.333;

        public NonlinearObjectiveFunction CreateObjFunc()
        {
            return new NonlinearObjectiveFunction(2, ObjectiveFunction, Gradient);
        }

        private double[] Gradient(double[] x)
        {
            return new double[]
            {
                // @G/@x, @G/@y
                G_CO2,
                G_H2O
            };
        }

        private double ObjectiveFunction(double[] x)
        {
            double xCO2 = x[0];
            double xH2O = x[1];
            return xCO2 * G_CO2 + xH2O * G_H2O;
        }

        public List<NonlinearConstraint> CreateConstraints()
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

        private double[] OxygenGradient(double[] x)
        {
            return new double[] { 2, 1 };
        }

        //private double OxygenBalance(double[] x)
        //{
        //    return 2 * x[0] + x[1] - 4; // x[0] is CO2, x[1] is H2O
        //}

        private double[] HydrogenGradient(double[] x)
        {
            return new double[] { 0, 2 };
        }

        //private double HydrogenBalance(double[] x)
        //{
        //    return 2 * x[1] - 4;    // x[1] is number of moles of H2O
        //}

        private double[] CarbonGradient(double[] x)
        {
            return new double[] { 1, 0 };
        }

        //private Func<double>[] CarbonBalance(double[] x)
        //{
        //    return new double[] x[0] - 1;
        //}
        //private double CarbonBalance(double[] x)
        //{
        //    return x[0] - 1;    // x[0] is the number of moles of CO2
        //}

    }
}
