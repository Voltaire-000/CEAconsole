using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAddNode
{
    public class Specie
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int TempIntervals { get; set; }
        public required string IdCode { get; set; }
        public ICollection<ChemicalFormula>? ChemicalFormula { get; set; }
        public int PhaseValue { get; set; }
        public double MolecularWeight { get; set; }
        public double HeatOfFormation { get; set; }
        public double BoilingPoint { get; set; }
        public ICollection<DataRecord>? DataRecords { get; set; }
    }
}
