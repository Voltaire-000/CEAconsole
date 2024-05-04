using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEAconsole.Models
{
    public class DTO_Reactant
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int T_Intervals { get; set; }
        public string Id_Code { get; set; }
        public Molecule Molecule { get; set; }
        public bool Gaseous { get; set; }
        public double MolecularWeight { get; set; }
        public double HeatOfFormation { get; set; }
        public Dictionary<string, DataRecord> TemperatureRange { get; set; }
    }

}

