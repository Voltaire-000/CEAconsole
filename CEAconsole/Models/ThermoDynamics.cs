using MathNet.Numerics;
using MathNet.Numerics.Differentiation;
using MathNet.Numerics.Integration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="T"></param>
        /// <param name="coefficients"></param>
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
    }
}
