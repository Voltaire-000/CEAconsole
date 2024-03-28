using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEAconsole.Models
{


    public class Molecule(List<Element> elements)
    {
        public List<Element> Elements { get; set; } = elements;
        public required string MolecularFormula { get; set; }
        public double MolecularWeight { get; set; }
        public StateOfMatter State { get; set; }
        public List<string>? BondTypes { get; set; }
        public bool IsPolar { get; set; }
        public double MeltingPoint { get; set; }
        public double BoilingPoint { get; set; }
        public Dictionary<string, string>? Solubility { get; set; }

        public enum StateOfMatter
        {
            Solid,
            Liquid,
            Gas,
            Crystalline
        }
    }
}
