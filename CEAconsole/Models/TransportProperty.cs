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
        public required ICollection<string> Name { get; set; }
        public required string Description { get; set; }
        public required ICollection<TransportDataRecord> DataRecords { get; set; }
    }
}
