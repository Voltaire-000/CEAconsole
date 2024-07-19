namespace CEAconsole.Models
{
    public class FunctionsCalculated
    {
        public Specie? Specie { get; set; }
        public SpecieType SpecieType { get; set; }
        public double Temperature { get; set; }
        public double Cp { get; set; }
        public double H298 { get; set; }
        public double Entropy { get; set; }
        public double GibbsH298 { get; set; }
        public double Enthalpy { get; set; }
        public double DeltaHf { get; set; }
        public double DeltaGibbsRxn { get; set; }
        public double LogK { get; set; }
    }
}
