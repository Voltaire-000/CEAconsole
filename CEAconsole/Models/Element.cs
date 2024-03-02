using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEAconsole.Models
{
    public class Element
    {
        public required string Symbol { get; set; }
        public double AtomicWeight { get; set; }
        public int Valence { get; set; }
    }
}
