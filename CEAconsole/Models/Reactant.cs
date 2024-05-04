namespace CEAconsole.Models
{
    public class Reactant
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public int T_Intervals { get; set; }
        public required string Id_Code { get; set; }
        public required Molecule Molecule { get; set; }
        public bool Gaseous { get; set; }
        public double MolecularWeight { get; set; }
        public double HeatOfFormation { get; set; }
        public required Dictionary<string, DataRecord> TemperatureRange { get; set; }
    }

}