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
    public static class ThermoDynamics
    {
        //private static readonly double[]? coefficients;
        static readonly double Gas_Constant_R = 8.31446261815324;

        public static double HeatCapacity(double T, List<double> coefficients, List<double> t_expnts)
        {
            double Cp = 0;
            for (int i = 0; i < coefficients.Count; i++)
            {
                Cp += coefficients[i] * Math.Pow(T, t_expnts[i]);
            }
            return Cp * Gas_Constant_R;
        }

        public static double Enthalpy(double ref_Temp, double T_1, List<double> coefficients, List<double> t_expnts)
        {
            double integrand(double T) => HeatCapacity(T, coefficients, t_expnts);
            double error;
            double L1Norm;
            return GaussKronrodRule.Integrate(integrand, ref_Temp, T_1, out error, out L1Norm, 1e-8)/1000;
        }

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
    }
}
