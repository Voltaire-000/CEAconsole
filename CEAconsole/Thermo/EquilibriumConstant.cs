namespace CEAconsole.Thermo
{
    public class EquilibriumConstant
    {
        public static double LogK(double Temperature, double DeltaGibbsRxn, double GASCONSTANT = 8.31446261815324)
        {
            double result = -(DeltaGibbsRxn * 1000) / (GASCONSTANT * Temperature);

            double x_ln = Math.Pow(Math.E, result);
            double ln = Math.Log10(x_ln);

            return ln;
        }
    }
}
