using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEAconsole.Models
{
    public class Species
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public int TempIntervals { get; set; }
        public required string IdCode { get; set; }
        //public required Molecule Molecule { get; set; }
        public required ICollection<ChemicalFormula> ChemicalFormula { get; set; }
        //public bool Gaseous { get; set; }
        public int PhaseValue { get; set; }
        public double MolecularWeight { get; set; }
        public double HeatOfFormation { get; set; }
        //public required Dictionary<string, Temperature_Range> TemperatureRange { get; set; }
        public required ICollection<Temperature_Range> DataRecords { get; set; }
    }
}
