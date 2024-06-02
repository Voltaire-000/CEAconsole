using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAddNode
{

    public class Molecule
    {
        public double Count { get; set; }
        public  ICollection<ChemicalFormula>? ChemicalFormula { get; set; }
    }

}
