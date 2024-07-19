using CEAconsole.Models;

namespace CEAconsole.Thermo
{
    /// <summary>
    /// TODO
    /// </summary>
    public static class HeatCapacity
    {
        /// <summary>
        /// Returns the Heat Capacity (Cp) in J/mol-k
        /// </summary>
        /// <param name="Temperature"></param>
        /// <param name="Specie"></param>
        /// <param name="GASCONSTANT"></param>
        /// <returns>Heat Capacity (Cp) in J/mol-k</returns>
        public static double Cp(double Temperature, IEnumerable<Specie> Specie, double GASCONSTANT = 8.31446261815324)
        {
            var T_specie = Utilities.GetRecordByTemperature(Temperature, Specie);
            var TemperatureExponents = T_specie.TExponents;
            var Coefficients = T_specie.Coefficients;

            double a1 = Coefficients[0];
            double a2 = Coefficients[1];
            double a3 = Coefficients[2];
            double a4 = Coefficients[3];
            double a5 = Coefficients[4];
            double a6 = Coefficients[5];
            double a7 = Coefficients[6];
            double Cp = GASCONSTANT * (a1 * Math.Pow(Temperature, TemperatureExponents[0])
                    + a2 * Math.Pow(Temperature, TemperatureExponents[1])
                    + a3
                    + a4 * Temperature
                    + a5 * Math.Pow(Temperature, TemperatureExponents[4])
                    + a6 * Math.Pow(Temperature, TemperatureExponents[5])
                    + a7 * Math.Pow(Temperature, TemperatureExponents[6]));
            if (double.IsNaN(Cp))
            {
                Cp = 0.0;
            }

            return Cp;
        }

        /// <summary>
        /// Heat Capacity (Cp) in J/mol-k
        /// Multiply by the Gas constant
        /// </summary>
        /// <param name="Temperature">Temperature in Kelvin</param>
        /// <returns>J/mol-k</returns>
        public static double Cp_R(double Temperature, IEnumerable<Specie> Specie)
        {
            var T_specie = Utilities.GetRecordByTemperature(Temperature, Specie);
            var TemperatureExponents = T_specie.TExponents;
            var Coefficients = T_specie.Coefficients;

            double a1 = Coefficients[0];
            double a2 = Coefficients[1];
            double a3 = Coefficients[2];
            double a4 = Coefficients[3];
            double a5 = Coefficients[4];
            double a6 = Coefficients[5];
            double a7 = Coefficients[6];

            double Cp = a1 * Math.Pow(Temperature, TemperatureExponents[0])
                                        + a2 * Math.Pow(Temperature, TemperatureExponents[1])
                                        + a3
                                        + a4 * Temperature
                                        + a5 * Math.Pow(Temperature, TemperatureExponents[4])
                                        + a6 * Math.Pow(Temperature, TemperatureExponents[5])
                                        + a7 * Math.Pow(Temperature, TemperatureExponents[6]);

            if (double.IsNaN(Cp))
            {
                Cp = 0.0;
            }
            return Cp;
        }

    }

}
