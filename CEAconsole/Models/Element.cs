namespace CEAconsole.Models
{
    public class Element
    {
        public string? Name { get; set; }
        public required string Symbol { get; set; }
        public double AtomicWeight { get; set; }
        public int Valence { get; set; }

        public Element(string name, string symbol, double atomicWeight, int valence)
        {
            // initialize properties
            Name = name;
            Symbol = symbol;
            AtomicWeight = atomicWeight;
            Valence = valence;
        }
    }
}
