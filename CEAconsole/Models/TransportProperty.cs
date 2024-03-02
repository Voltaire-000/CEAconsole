using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CEAconsole.Models
{
    public class TransportProperty
    {
        [JsonProperty("name")]
        public required List<string> Name { get; set; }
        [JsonProperty("description")]
        public required string Description { get; set; }
        [JsonProperty("RecordData")]
        public required List<RecordData> Data { get; set; }

        public class RecordData
        {
            [JsonProperty("viscosity_temperatureRange")]
            public List<double>? ViscosityTemperatureRange { get; set; }

            [JsonProperty("viscosity_coefficients")]
            public List<double>? ViscosityCoefficients {  get; set; }

            [JsonProperty("conductivity_temperatureRange")]
            public List<double>? ConductivityTemperatureRange { get; set; }

            [JsonProperty("thermal_conductivity_coefficients")]
            public List<double>? ThermalConductivityCoefficients { get; set; }
        }
    }
}
