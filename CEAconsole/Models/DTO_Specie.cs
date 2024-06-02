using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEAconsole.Models
{
    public class DTO_Specie
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int TempIntervals { get; set; }
        public string? IdCode { get; set; }
        public MoleculeCollection? Molecule { get; set; }
        public int PhaseValue { get; set; }
        public double MolecularWeight { get; set; }
        public double HeatOfFormation { get; set; }
        public double BoilingPoint { get; set; }
        public ICollection<DataRecord>? DataRecords { get; set; }
    }
}
