using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CEAconsole.Models
{
    public static class MoleculeOperations
    {
        public static IEnumerable<Dictionary<string, double>> SplitMolecule(string molecule)
        {
            // Regex that matches elements and numbers
            string pattern = @"([A-Z][a-z]?)(\d*)";

            // Find all matches in the molecule string
            MatchCollection matchCollection = Regex.Matches(molecule, pattern);

            // Dictionary to hold the split parts
            IEnumerable<Dictionary<string, double>> splitParts = (IEnumerable<Dictionary<string, double>>)matchCollection;
            foreach (Match match in matchCollection)
            {
                // Capture the element symbol
                string element = match.Groups[1].Value;
                // Capture the quantity if any
                string quantity = match.Groups[2].Value;

                //splitParts.Add(element + (string.IsNullOrEmpty(quantity) ? "" : ":" + quantity));
                
            }
            return null;
            //return splitParts;

        }
    }
}
