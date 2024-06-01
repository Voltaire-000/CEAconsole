using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEAconsole.Models
{
    public class DataRecord
    {
        public List<double>? TemperatureRange { get; set; }
        public int NumberOfCoefficients { get; set; }
        public List<double>? TExponents { get; set; }
        public double EnthalpyRef { get; set; }
        public List<double>? Coefficients { get; set; }
        public List<double>? IntegrationConstants { get; set; }
    }
}

