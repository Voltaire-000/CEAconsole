using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEAconsole.Models
{
    public static class ThermoDynamics
    {
        static readonly double Gas_Constant_R = 8.31446261815324;
        public static double GetEnthalpy(List<JToken> coefficientsList, List<JToken> integrationConstantList, double Kelvin)
        {
            double a1 = coefficientsList[0].ToObject<double>();
            double a2 = coefficientsList[1].ToObject<double>();
            double a3 = coefficientsList[2].ToObject<double>();
            double a4 = coefficientsList[3].ToObject<double>();
            double a5 = coefficientsList[4].ToObject<double>();
            double a6 = coefficientsList[5].ToObject<double>();
            double a7 = coefficientsList[6].ToObject<double>();
            // integration constants
            double a_8 = integrationConstantList[0].ToObject<double>();
            double a_9 = integrationConstantList[1].ToObject<double>();

            double enthalpy = Gas_Constant_R * Kelvin * -((a1 * Math.Pow(Kelvin, -2))
                                        + (a2 * Math.Pow(Kelvin, -1) * Math.Log(Kelvin))
                                        + a3
                                        + (a4 * (Kelvin / 2))
                                        + (a5 * (Math.Pow(Kelvin, 2) / 3))
                                        + (a6 * (Math.Pow(Kelvin, 3) / 4))
                                        + (a7 * (Math.Pow(Kelvin, 4) / 5))
                                        + (a_8 / Kelvin));
            enthalpy /= 1000;
            return enthalpy;
        }

        public static double GetEntropy(List<JToken> coefficientsList, List<JToken> integrationConstantList, double Kelvin)
        {
            double a1 = coefficientsList[0].ToObject<double>();
            double a2 = coefficientsList[1].ToObject<double>();
            double a3 = coefficientsList[2].ToObject<double>();
            double a4 = coefficientsList[3].ToObject<double>();
            double a5 = coefficientsList[4].ToObject<double>();
            double a6 = coefficientsList[5].ToObject<double>();
            double a7 = coefficientsList[6].ToObject<double>();
            // integration constants
            double a_8 = integrationConstantList[0].ToObject<double>();
            double a_9 = integrationConstantList[1].ToObject<double>();

            double Entropy = Gas_Constant_R * (-a1 * Math.Pow(Kelvin, -2)
                                 - (a2 * Math.Pow(Kelvin, -1))
                                 + (a3 * Math.Log(Kelvin))
                                 + (a4 * Kelvin)
                                 + (a5 * Math.Pow(Kelvin, 2) / 2)
                                 + (a6 * Math.Pow(Kelvin, 3) / 3)
                                 + (a7 * Math.Pow(Kelvin, 4) / 4)
                                 + a_9);
            return Entropy;
        }

        public static double GetHeatCapacity(List<JToken> coefficientsList, List<JToken> integrationConstantList, double Kelvin)
        {
            double a1 = coefficientsList[0].ToObject<double>();
            double a2 = coefficientsList[1].ToObject<double>();
            double a3 = coefficientsList[2].ToObject<double>();
            double a4 = coefficientsList[3].ToObject<double>();
            double a5 = coefficientsList[4].ToObject<double>();
            double a6 = coefficientsList[5].ToObject<double>();
            double a7 = coefficientsList[6].ToObject<double>();
            // integration constants
            double a_8 = integrationConstantList[0].ToObject<double>();
            double a_9 = integrationConstantList[1].ToObject<double>();

            double heat_capacity = Gas_Constant_R * (a1
                                    * Math.Pow(Kelvin, -2)
                                    + a2
                                    * Math.Pow(Kelvin, -1)
                                    + a3
                                    + a4
                                    * Kelvin
                                    + a5
                                    * Math.Pow(Kelvin, 2)
                                    + a6
                                    * Math.Pow(Kelvin, 3)
                                    + a7
                                    * Math.Pow(Kelvin, 4));
            return heat_capacity;
        }
    }
}
