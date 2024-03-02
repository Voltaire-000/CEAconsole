namespace CEAconsole.Models
{
    public class Reactant
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public int T_intervals { get; set; }
        public required string Id_Code { get; set; }
        public required Dictionary<string, double> ChemicalFormula { get; set; }
        public bool Gaseous { get; set; }
        public double MolecularWeight { get; set; }
        public double HeatOfFormation { get; set; }
        public required Dictionary<string, Range> TemperatureRange { get; set; }
    }

    public class Range
    {
        public required List<double> TemperatureRange { get; set; }
        public int NumberOfCoefficients { get; set; }
        public required List<double> TExponents { get; set; }
        public double H_Jmol { get; set; }
        public required List<double> Coefficients { get; set; }
        public required List<double> IntegrationConstants { get; set; }
    }
}