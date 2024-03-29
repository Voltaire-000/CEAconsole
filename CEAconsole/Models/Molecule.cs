using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEAconsole.Models
{

    public class Molecule
    {
        public double Count { get; set; }
        public required Dictionary<string, double> ChemicalFormula { get; set; }
    }

}
