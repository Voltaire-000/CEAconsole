using CEAconsole.Models;

namespace CEAconsole.Thermo
{
    public static class Entropy
    {
        public static double Entropy_S(double Temperature, IEnumerable<Specie> Specie, double ReferenceTemperature = 298.15, double GASCONSTANT = 8.31446261815324)
        {
            var T_specie = Utilities.GetRecordByTemperature(Temperature, Specie);
            var TemperatureExponents = T_specie.TExponents;
            var Coefficients = T_specie.Coefficients;
            var IntegrationConstants = T_specie.IntegrationConstants;

            double a1 = Coefficients[0];
            double a2 = Coefficients[1];
            double a3 = Coefficients[2];
            double a4 = Coefficients[3];
            double a5 = Coefficients[4];
            double a6 = Coefficients[5];
            double a7 = Coefficients[6];

            double integrationConstantZero = IntegrationConstants[0];
            double integrationConstantOne = IntegrationConstants[1];
            double entropy = GASCONSTANT * (-a1 * Math.Pow(Temperature, TemperatureExponents[0]) / 2
             - a2 * Math.Pow(Temperature, TemperatureExponents[1])
            + a3 * Math.Log(Temperature)
            + a4 * Temperature
            + a5 * Math.Pow(Temperature, TemperatureExponents[4]) / 2
            + a6 * Math.Pow(Temperature, TemperatureExponents[5]) / 3
            + a7 * Math.Pow(Temperature, TemperatureExponents[6]) / 4
            + integrationConstantOne);
            if (double.IsNaN(entropy))
            {
                entropy = 0;
            }
            return entropy;
        }

        public static double Entropy_S(double Temperature, List<double> TemperatureExponents, List<double> Coefficients, List<double> IntegrationConstants, double GASCONSTANT = 8.31446261815324)
        {
            double a1 = Coefficients[0];
            double a2 = Coefficients[1];
            double a3 = Coefficients[2];
            double a4 = Coefficients[3];
            double a5 = Coefficients[4];
            double a6 = Coefficients[5];
            double a7 = Coefficients[6];

            double integrationConstantZero = IntegrationConstants[0];
            double integrationConstantOne = IntegrationConstants[1];
            double entropy = GASCONSTANT * (-a1 * Math.Pow(Temperature, TemperatureExponents[0]) / 2
             - a2 * Math.Pow(Temperature, TemperatureExponents[1])
            + a3 * Math.Log(Temperature)
            + a4 * Temperature
            + a5 * Math.Pow(Temperature, TemperatureExponents[4]) / 2
            + a6 * Math.Pow(Temperature, TemperatureExponents[5]) / 3
            + a7 * Math.Pow(Temperature, TemperatureExponents[6]) / 4
            + integrationConstantOne);
            if (double.IsNaN(entropy))
            {
                entropy = 0.0;
            }
            return entropy;
        }

        public static double DeltaSrxn(double Temperature, IEnumerable<Specie> Specie, IEnumerable<Specie> ReferenceElements)
        {
            if (Specie.FirstOrDefault().HeatOfFormation != 0.0)
            {
                var MoleculeCoefficients = Utilities.BalanceEquationElements(Specie, ReferenceElements);

                Dictionary<string, ChemicalFormula> Reactants = new();
                Dictionary<string, Molecule> Products = new()
            {
                // Product Molecule
                {Specie.ElementAt(0).Name, Specie.ElementAt(0).Molecule }
            };
                // Add the individual elements to the Reactants Discionary
                double elementCount = Specie.ElementAt(0).Molecule.ChemicalFormula.Count;
                for (int i = 0; i < elementCount; i++)
                {
                    //Reactants.Add(Specie.ElementAt(0).Molecule.ChemicalFormula.ElementAt(i).Symbol, Specie.ElementAt(0).Molecule.ChemicalFormula.ElementAt(i));
                    var m_element = ReferenceElements.Where(x => x.Molecule.ChemicalFormula.ElementAt(0).Symbol == Specie.ElementAt(0).Molecule.ChemicalFormula.ElementAt(i).Symbol);
                    string element_symbol = m_element.First().Molecule.ChemicalFormula.ElementAt(0).Symbol;
                    Reactants.Add(element_symbol, m_element.First().Molecule.ChemicalFormula.ElementAt(0));
                }

                double Rsum_entropy = 0.0;
                double Psum_entropy = 0.0;

                for (int i = 0; i < Reactants.Count; i++)
                {
                    var elementData = from element in ReferenceElements
                                      where element.Molecule.ChemicalFormula.ElementAt(0).Symbol == Specie.ElementAt(0).Molecule.ChemicalFormula.ElementAt(i).Symbol
                                      select element;
                    List<double> tExpnts = elementData.First().DataRecords.ElementAt(0).TExponents;
                    List<double> coefficients = elementData.First().DataRecords.ElementAt(0).Coefficients;
                    List<double> integrationConstants = elementData.First().DataRecords.ElementAt(0).IntegrationConstants;
                    // number of moles
                    Rsum_entropy += MoleculeCoefficients[i] * Entropy_S(Temperature, tExpnts, coefficients, integrationConstants);

                }

                foreach (var product in Products)
                {
                    var tExpnts = Specie.First().DataRecords.ElementAt(0).TExponents;
                    var coeff = Specie.First().DataRecords.ElementAt(0).Coefficients;
                    var integratConstants = Specie.First().DataRecords.ElementAt(0).IntegrationConstants;
                    // number of moles
                    int MoleCoeffCount = MoleculeCoefficients.Count;
                    double moleCoeff = MoleculeCoefficients[MoleCoeffCount - 1];
                    Psum_entropy += moleCoeff * Entropy_S(Temperature, tExpnts, coeff, integratConstants);
                }

                double deltaSrxn = Psum_entropy - Rsum_entropy;
                return deltaSrxn;
            }
            else
            {
                return 0.0;
            }


        }
    }
}
