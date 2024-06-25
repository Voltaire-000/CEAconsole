namespace CEAconsole.Models
{
    public class Element
    {
        public required string Symbol { get; set; }
        public required double AtomicWeight { get; set; }
        public required int Valence { get; set; }

        //public Element(string symbol, double atomicWeight, int valence)
        //{
        //    // initialize properties
        //    Symbol = symbol;
        //    AtomicWeight = atomicWeight;
        //    Valence = valence;
        //}
    }
}
