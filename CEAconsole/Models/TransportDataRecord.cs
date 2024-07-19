using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEAconsole.Models
{
    public class TransportDataRecord
    {
        public required List<double> ViscosityTemperatureRange { get; set; }
        public required List<double> ViscosityCoefficients { get; set; }
        public required List<double> ConductivityTemperatureRange { get; set; }
        public required List<double> ThermalConductivityCoefficients { get; set; }
    }
}
