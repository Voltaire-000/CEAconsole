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

namespace CEAconsole.Models
{
    // TODO add comments

    /// <summary>
    /// 
    /// </summary>
    public static class ThermoDynamics
    {
        //private static readonly double[]? coefficients;
        static readonly double Gas_Constant_R = 8.31446261815324;

        // TODO change T -> the temperature range or adjust the code to process based on temperature input by se;ecting temperature range
        /// <summary>
        /// Returns the Heat Capacity Cp for the given Temperature, Coefficients, and Temperature Exponents
        /// </summary>
        /// <param name="T">Temperature in Kelvin</param>
        /// <param name="coefficients">List of Temperature Coefficients</param>
        /// <param name="t_expnts"></param>
        /// <returns></returns>
        public static double HeatCapacity(double T, List<double> coefficients, List<double> t_expnts)
        {
            double Cp = 0;
            for (int i = 0; i < coefficients.Count; i++)
            {
                Cp += coefficients[i] * Math.Pow(T, t_expnts[i]);
            }
            return Cp * Gas_Constant_R;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ref_Temp"></param>
        /// <param name="T_1"></param>
        /// <param name="coefficients"></param>
        /// <param name="t_expnts"></param>
        /// <returns></returns>
        public static double DeltaEnthalpyRef(double ref_Temp, double T_1, List<double> coefficients, List<double> t_expnts)
        {
            double integrand(double T) => HeatCapacity(T, coefficients, t_expnts);
            double error;
            double L1Norm;
            return GaussKronrodRule.Integrate(integrand, ref_Temp, T_1, out error, out L1Norm, 1e-8)/1000;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ref_Temp"></param>
        /// <param name="T_1"></param>
        /// <param name="coefficients"></param>
        /// <param name="t_expnts"></param>
        /// <returns></returns>
        public static double Enthalpy(double ref_Temp, double T_1, List<double> coefficients, List<double> t_expnts)
        {
            // TODO fix magic number
            double heatOfFormation = -74600.0;
            double ref_enthalpy = heatOfFormation / 1000.0;
            double integrand(double T) => HeatCapacity(T, coefficients, t_expnts);
            double error;
            double L1Norm;
            double deltaEnthalpy = GaussKronrodRule.Integrate(integrand, ref_Temp, T_1, out error, out L1Norm, 1e-8) / 1000;

            double enthalpy = ref_enthalpy + deltaEnthalpy;
            return enthalpy;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ref_Temp"></param>
        /// <param name="T_1"></param>
        /// <param name="coefficients"></param>
        /// <param name="t_expnts"></param>
        /// <returns></returns>
        public static double Entropy(double ref_Temp, double T_1 , List<double> coefficients, List<double> t_expnts)
        {
            double integrand(double T) => HeatCapacity(T, coefficients, t_expnts)/T;
            
            double error;
            double L1Norm;
            
            // TODO get base entropy at 298.15
            double integral = GaussKronrodRule.Integrate(integrand, ref_Temp, T_1, out error, out L1Norm, 1e-8);
            //double delta_T = ref_Temp - T_1;
            return integral + 186.371;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ref_Temp"></param>
        /// <param name="T_1"></param>
        /// <param name="coefficients"></param>
        /// <param name="t_expnts"></param>
        /// <returns></returns>
        public static double GibbsRef(double ref_Temp, double T_1, List<double> coefficients, List<double> t_expnts)
        {
            double enthalpyIntegrand(double T) => HeatCapacity(T, coefficients, t_expnts);
            double entropyIntegrand(double T) => HeatCapacity(T, coefficients, t_expnts) / T;
            double error;
            double L1Norm;
            double enthalpy = GaussKronrodRule.Integrate(enthalpyIntegrand, ref_Temp, T_1, out error, out L1Norm, 1e-8) / 1000;
            double entropy = GaussKronrodRule.Integrate(entropyIntegrand, ref_Temp, T_1, out error, out L1Norm, 1e-8);
            double ref_entropy = 186.371;
            entropy = entropy + ref_entropy;

            double gibbs = -((enthalpy * 1000) - T_1 * entropy) / T_1;
            return gibbs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ref_Temp"></param>
        /// <param name="T_1"></param>
        /// <param name="coefficients"></param>
        /// <param name="t_expnts"></param>
        /// <returns></returns>
        public static double EnthalpyFormation(double ref_Temp, double T_1, List<double> coefficients, List<double> t_expnts)
        {

            //Func<double, double> cpFunction = HeatCapacity;
            double heatCapacityIntegrand(double T) => HeatCapacity(T, coefficients, t_expnts);
            double error;
            double L1Norm;
            //double enthalpyFormation = GaussKronrodRule.Integrate(heatCapacityIntegrand, ref_Temp, T_1, out error, out L1Norm, 1e-8);
            double changeInHeatCapacity = GaussKronrodRule.Integrate(heatCapacityIntegrand, ref_Temp, T_1, out error, out L1Norm, 1e-8) / 1000;
            double xv = SimpsonRule.IntegrateComposite(heatCapacityIntegrand, 298.15, 398.15, 4);
            return xv;

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
            double activity = 0.90710;
            double standardActivity = 1.0;

            double chemicalpotential = gibbs + (Gas_Constant_R * temperature  * Math.Log(activity / standardActivity));

            return chemicalpotential;

        }
    }
}
