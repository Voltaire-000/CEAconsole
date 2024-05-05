using MathNet.Numerics;
using MathNet.Numerics.Differentiation;
using MathNet.Numerics.Integration;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vector = MathNet.Numerics.LinearAlgebra.Double.Vector;

namespace CEAconsole.ThermoChemistry
{

    // TODO add comments

    /// <summary>
    /// 
    /// </summary>
    public static class ThermoDynamics
    {

        //private static readonly double[]? coefficients;
        static readonly double Gas_Constant_R = 8.31446261815324;

        // TODO change T -> the temperature range or adjust the code to process based on temperature input by selecting temperature range
        /// <summary>
        /// Returns the Heat Capacity Cp for the given Temperature, Coefficients, and Temperature Exponents
        /// </summary>
        /// <param name="T">Temperature in Kelvin</param>
        /// <param name="temperatureExponents">List of Temperature exponents from the NASA polynomials</param>
        /// <param name="coefficients">List of Coefficients from the NASA polynomials</param>
        /// <returns>Heat capacity (Cp) in J/mol-K</returns>
        public static double HeatCapacity(double Temperature, List<double> temperatureExponents, List<double> coefficients, double GASCONSTANT = 8.31446261815324)
        {
            if (Temperature < 0)
            {
                throw new ArgumentException("Temperature must be positive");
            }
            double a1 = coefficients[0];
            double a2 = coefficients[1];
            double a3 = coefficients[2];
            double a4 = coefficients[3];
            double a5 = coefficients[4];
            double a6 = coefficients[5];
            double a7 = coefficients[6];

            double Cp = GASCONSTANT * (a1 * Math.Pow(Temperature, temperatureExponents[0])
                                        + a2 * Math.Pow(Temperature, temperatureExponents[1])
                                        + a3
                                        + a4 * Temperature
                                        + a5 * Math.Pow(Temperature, temperatureExponents[4])
                                        + a6 * Math.Pow(Temperature, temperatureExponents[5])
                                        + a7 * Math.Pow(Temperature, temperatureExponents[6]));
            if (double.IsNaN(Cp))
            {
                Cp = 0.0;
            }
            return Cp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceTemperature"></param>
        /// <param name="T1"></param>
        /// <param name="coefficients">List of Coefficients from the NASA polynomials</param>
        /// <param name="temperatureExponents">List of Temperature exponents from the NASA polynomials</param>
        /// <returns>Enthalpy (H-H298) in kJ/mol</returns>
        public static double EnthalpyRefH298(double referenceTemperature, double T1, List<double> coefficients, List<double> tExpnts)
        {
            double integrand(double T) => HeatCapacity(T, tExpnts, coefficients);
            double enthalpy = GaussKronrodRule.Integrate(integrand, referenceTemperature, T1, out double error, out double L1Norm, 1e-8) / 1000;
            return enthalpy;
        }

        /// <summary>
        /// Calculates Enthalpy (H) and returns the value in kJ/mol
        /// </summary>
        /// <param name="Temperature">Kelvin</param>
        /// <param name="temperatureExponents">List of Temperature exponents from the NASA polynomials</param>
        /// <param name="coefficients">List of Coefficients from the NASA polynomials</param>
        /// <param name="integrationConstants">List of integration constants from the NASA polynomials </param>
        /// <returns>(enthalpy)H kJ/mol</returns>
        public static double Enthalpy(double Temperature, List<double> temperatureExponents, List<double> coefficients, List<double> integrationConstants, double GASCONSTANT = 8.31446261815324)
        {
            double a1 = coefficients[0];
            double a2 = coefficients[1];
            double a3 = coefficients[2];
            double a4 = coefficients[3];
            double a5 = coefficients[4];
            double a6 = coefficients[5];
            double a7 = coefficients[6];
            //double a8 = coefficients[7];

            double i8 = integrationConstants[0];
            double i9 = integrationConstants[1];

            double enthalpy = GASCONSTANT * Temperature * (-a1 * Math.Pow(Temperature, temperatureExponents[0])
                               + a2 * Math.Pow(Temperature, temperatureExponents[1]) * Math.Log(Temperature)
                               + a3
                               + a4 * Temperature / 2
                               + a5 * Math.Pow(Temperature, 2) / 3
                               + a6 * Math.Pow(Temperature, 3) / 4
                               + a7 * Math.Pow(Temperature, 4) / 5
                               + i8 / Temperature);
            if (double.IsNaN(enthalpy))
            {
                enthalpy = 0.0;
            }
            return enthalpy / 1000;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Temperature">Temperature in Kelvin</param>
        /// <param name="exponents">List of Temperature exponents from the NASA polynomials</param>
        /// <param name="coefficients">List of Coefficients from the NASA polynomials</param>
        /// <param name="integrationConstants">List of integration constants from the NASA polynomials</param>
        /// <param name="GASCONSTANT">Optional Universal Gas Constant = 8.31446261815324</param>
        /// <returns>Entropy S J/mol-K</returns>
        public static double Entropy(double Temperature, List<double> exponents, List<double> coefficients, List<double> integrationConstants, double GASCONSTANT = 8.31446261815324)
        {
            double a1 = coefficients[0];
            double a2 = coefficients[1];
            double a3 = coefficients[2];
            double a4 = coefficients[3];
            double a5 = coefficients[4];
            double a6 = coefficients[5];
            double a7 = coefficients[6];

            double integrationConstantZero = integrationConstants[0];
            double integrationConstantOne = integrationConstants[1];

            double entropy = GASCONSTANT * (-a1 * Math.Pow(Temperature, exponents[0]) / 2
              - a2 * Math.Pow(Temperature, exponents[1])
              + a3 * Math.Log(Temperature)
              + a4 * Temperature
              + a5 * Math.Pow(Temperature, exponents[4]) / 2
              + a6 * Math.Pow(Temperature, exponents[5]) / 3
              + a7 * Math.Pow(Temperature, exponents[6]) / 4
              + integrationConstantOne);
            if (double.IsNaN(entropy))
            {
                entropy = 0.0;
            }
            return entropy;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceTemperature">298.15 Kelvin</param>
        /// <param name="T_1">Temperature in Kelvin being tested</param>
        /// <param name="coefficients">List of Temperature Coefficients from the NASA polynomials</param>
        /// <param name="t_expnts">List of coefficient exponents from the NASA polynomials</param>
        /// <returns></returns>
        public static double GibbsRef(double referenceTemperature, double referenceEntropy, double T_1, List<double> coefficients, List<double> t_expnts)
        {
            double enthalpyIntegrand(double T) => HeatCapacity(T, t_expnts, coefficients);
            double entropyIntegrand(double T) => HeatCapacity(T, t_expnts, coefficients) / T;
            double enthalpy = GaussKronrodRule.Integrate(enthalpyIntegrand, referenceTemperature, T_1, out double error, out double L1Norm, 1e-8) / 1000;
            double entropy = GaussKronrodRule.Integrate(entropyIntegrand, referenceTemperature, T_1, out error, out L1Norm, 1e-8);
            entropy += referenceEntropy;

            double gibbs = -(enthalpy * 1000 - T_1 * entropy) / T_1;
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
        /// <param name="refTemp">298.15 Kelvin</param>
        /// <param name="T1">Temperature in Kelvin</param>
        /// <param name="coefficients">List of Temperature Coefficients from the NASA polynomials</param>
        /// <param name="tExpnts">List of coefficient exponents from the NASA polynomials</param>
        /// <returns>Enthalpy H-H298 kJ/mol</returns>
        public static double EnthalpyFormation(double refTemp, double T1, List<double> coefficients, List<double> tExpnts)
        {
            double heatCapacityIntegrand(double T) => HeatCapacity(T, tExpnts, coefficients);
            return GaussKronrodRule.Integrate(heatCapacityIntegrand, refTemp, T1, out double error, out double L1Norm, 1e-8) / 1000;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static MathNet.Numerics.LinearAlgebra.Vector<double> BalanceHydrocarbonEquation(Matrix<double> matrix)
        {
            int numRows = matrix.RowCount;
            int numColumns = matrix.ColumnCount;
            int nonZeroValues = 0;
            int ii = 0;
            double[] values = new double[] { };
            IEnumerable<(int, MathNet.Numerics.LinearAlgebra.Vector<double>)> m_enumeratedRows = matrix.EnumerateRowsIndexed();
            for (int i = 0; i < numRows; i++)
            {
                var m_elementAtRow = m_enumeratedRows.ElementAt(i);
                int m_rowNumber = m_elementAtRow.Item1;
                MathNet.Numerics.LinearAlgebra.Vector<double> m_rowVector = m_elementAtRow.Item2;

                foreach (var item in m_rowVector)
                {
                    int m_compare = item.CompareTo(0.0);
                    if (m_compare != 0)
                    {

                        nonZeroValues++;
                    }
                }
            }

            //MathNet.Numerics.LinearAlgebra.Vector<double> m_values = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(nonZeroValues);
            double[] m_values = new double[nonZeroValues];
            for (int i = 0; i < numRows; i++)
            {
                var m_elementAtRow = m_enumeratedRows.ElementAt(i);
                int m_rowNumber = m_elementAtRow.Item1;
                MathNet.Numerics.LinearAlgebra.Vector<double> m_rowVector = m_elementAtRow.Item2;

                foreach (var item in m_rowVector)
                {
                    int m_compare = item.CompareTo(0.0);
                    if (m_compare != 0)
                    {
                        m_values[ii] = item;
                        ii++;
                    }
                }

            }

            //MathNet.Numerics.LinearAlgebra.Vector<int> IA = MathNet.Numerics.LinearAlgebra.Vector<int>.Build.Dense(numRows + 1);
            int[] IA = new int[numRows + 1];
            //MathNet.Numerics.LinearAlgebra.Vector<int> JA = MathNet.Numerics.LinearAlgebra.Vector<int>.Build.Dense(nonZeroValues);
            int[] JA = new int[nonZeroValues];
            int jj = 0;
            for (int i = 0; i < numRows; i++)
            {
                var m_elementAtRow = m_enumeratedRows.ElementAt(i);
                int m_rowNumber = m_elementAtRow.Item1;
                MathNet.Numerics.LinearAlgebra.Vector<double> m_rowVector = m_elementAtRow.Item2;
                int Row_non_zero_values = 0;
                int column_where_value_found = 0;

                foreach (var item in m_rowVector)
                {
                    var m_compare = item.CompareTo(0.0);
                    if (m_compare != 0)
                    {
                        // increment valuesCount
                        Row_non_zero_values++;
                        // what column was it found in
                        // { 0, 2, 0, 3, 1, 2, 3, 0 };
                        //int m_c = column_where_value_found;

                        JA[jj] = column_where_value_found;
                        jj++;

                    }
                    column_where_value_found++;

                }
                IA[i + 1] = IA[i] + Row_non_zero_values;
            }

            Matrix<double> A = Matrix.Build.SparseFromCompressedSparseRowFormat(numRows, numColumns, m_values.Length, IA, JA, m_values);

            // Define the right hand side
            MathNet.Numerics.LinearAlgebra.Vector<double> b = Vector.Build.Dense(new double[] { 0, 0, 0, 1.0 });
            // solution
            MathNet.Numerics.LinearAlgebra.Vector<double> solution = A.Solve(b);
            return solution;
        }

        public static double Calculate_MU(double gibbs, double temperature, double pressure)
        {
            // MU = G + 
            //double standardChemicalPotential = 0.0;
            double activity = 1.0;
            double standardActivity = 1.0;

            double chemicalpotential = gibbs + Gas_Constant_R * temperature * Math.Log(activity / standardActivity);

            return chemicalpotential;

        }
    }
}
