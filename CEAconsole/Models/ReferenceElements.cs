using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEAconsole.Models
{
    public class ReferenceElements
    {
        public required string Species { get; set; }
        public required string Description { get; set; }
        public int Tintervals { get; set; }
        public required string IdCode { get; set; }
        public required ICollection<ChemicalFormula> ChemicalFormula { get; set; }
        public int PhaseValue { get; set; }
        public double MolecularWeight { get; set; }
        public double HeatOfFormation { get; set; }
        public required ICollection<Temperature_Range> DataRecords { get; set; }
    }
}
