using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEAconsole.Models
{
    public class MoleculeCollection
    {
        public double Count { get; set; }
        public ICollection<ChemicalFormula>? ChemicalFormula { get; set; }
    }
}
