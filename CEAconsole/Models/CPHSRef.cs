using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEAconsole.Models
{
    public class CPHSRef
    {
        public required string Species_Name { get; set; }
        public double Molecular_Weight { get; set; }

        [JsonConverter(typeof(StringDoubleConverter))]
        public required object Enthalpy { get; set; }
        [JsonConverter(typeof(StringDoubleConverter))]
        public required object Delta_Enthalpy { get; set; }

        public required double Delta_Enthalpy_Ref { get; set; }
        public required double CP_Ref { get; set; }

        [JsonConverter(typeof(StringDoubleConverter))]
        public required object Enthalpy_Ref { get; set; }

        public required double Entropy_Ref { get; set; }

    }
}
