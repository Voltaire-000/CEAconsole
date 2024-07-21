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
        public string SpeciesName { get; set; }
        public string SecondSpeciesName { get; set; }
        public required string Comments { get; set; }
        public required ICollection<TransportDataRecord> DataRecords { get; set; }
    }
}
