using CEAconsole.Models;
using MathNet.Numerics.Integration;

namespace CEAconsole.Thermo
{
    public static class Enthalpy
    {
        public static double EnthalpyH298(double Temperature, IEnumerable<Specie> Specie, double ReferenceTemperature = 298.15)
        {
            double integrand(double T) => HeatCapacity.Cp(T, Specie);
            double refH298 = GaussKronrodRule.Integrate(integrand, ReferenceTemperature, Temperature, out double error, out double L1Norm, 1e-8) / 1000;
            return refH298;
        }

        public static double Enthalpy_H(double Temperature, IEnumerable<Specie> Specie, double GASCONSTANT = 8.31446261815324)
        {
            DataRecord T_specie = Utilities.GetRecordByTemperature(Temperature, Specie);
            List<double> TemperatureExponents = T_specie.TExponents;
            List<double> Coefficients = T_specie.Coefficients;
            List<double> IntegrationConstants = T_specie.IntegrationConstants;

            double a1 = Coefficients[0];
            double a2 = Coefficients[1];
            double a3 = Coefficients[2];
            double a4 = Coefficients[3];
            double a5 = Coefficients[4];
            double a6 = Coefficients[5];
            double a7 = Coefficients[6];
            //double a8 = coefficients[7];

            double i8 = IntegrationConstants[0];
            double i9 = IntegrationConstants[1];

            double enthalpy = GASCONSTANT * Temperature * (-a1 * Math.Pow(Temperature, TemperatureExponents[0])
                               + a2 * Math.Pow(Temperature, TemperatureExponents[1]) * Math.Log(Temperature)
                               + a3
                               + a4 * (Temperature / 2)
                               + a5 * (Math.Pow(Temperature, TemperatureExponents[4]) / 3)
                               + a6 * (Math.Pow(Temperature, TemperatureExponents[5]) / 4)
                               + a7 * (Math.Pow(Temperature, TemperatureExponents[6]) / 5)
                               + (i8 / Temperature));
            if (double.IsNaN(enthalpy))
            {
                enthalpy = 0.0;
            }
            return enthalpy / 1000;
        }

        public static double DeltaHrxn(double Temperature, IEnumerable<Specie> Specie, IEnumerable<Specie> ReferenceElements)
        {
            if (Specie.FirstOrDefault().HeatOfFormation != 0)
            {
                var MoleculeCoefficients = Utilities.BalanceEquationElements(Specie, ReferenceElements);

                Dictionary<string, ChemicalFormula> Reactants = new();
                Dictionary<string, Molecule> Products = new()
            {
                // Product Molecule
                { Specie.ElementAt(0).Name, Specie.ElementAt(0).Molecule }
            };

                // Add the individual elements to the Reactants Dictionary
                //double elementCount = Specie.ElementAt(0).Molecule.ChemicalFormula.Count;
                List<int> elementCount = Specie.Select(selector: m => m.Molecule.ChemicalFormula.Count).ToList();
                for (int i = 0; i < elementCount.Count; i++)
                {
                    //Reactants.Add(Specie.ElementAt(0).Molecule.ChemicalFormula.ElementAt(i).Symbol, Specie.ElementAt(0).Molecule.ChemicalFormula.ElementAt(i));
                    var m_element = ReferenceElements.Where(x => x.Molecule.ChemicalFormula.ElementAt(0).Symbol == Specie.ElementAt(0).Molecule.ChemicalFormula.ElementAt(i).Symbol);
                    string element_symbol = m_element.First().Molecule.ChemicalFormula.ElementAt(0).Symbol;
                    Reactants.Add(element_symbol, m_element.First().Molecule.ChemicalFormula.ElementAt(0));
                }

                double Rsum_Hf = 0.0;
                double Psum_Hf = 0.0;
                double deltaHrxn = 0.0;

                for (int i = 0; i < Reactants.Count; i++)
                {

                    var elementSpecie = from element in ReferenceElements
                                      where element.Molecule.ChemicalFormula.ElementAt(0).Symbol == Specie.ElementAt(0).Molecule.ChemicalFormula.ElementAt(i).Symbol
                                      select element;
                    // number of moles
                    Rsum_Hf += MoleculeCoefficients[i] * EnthalpyH298(Temperature,elementSpecie);
                }

                foreach (var product in Products)
                {
                    // number of moles
                    int MoleCoeffCount = MoleculeCoefficients.Count;
                    double moleCoeff = MoleculeCoefficients[MoleCoeffCount - 1];
                    //Psum_Hf += moleCoeff * Enthalpy.Enthalpy_H(Temperature, Specie);
                    Psum_Hf += Enthalpy_H(Temperature, Specie);
                }

                deltaHrxn = Psum_Hf - Rsum_Hf;
                return deltaHrxn;
            }
            else
            {
                return 0.0;
            }
        }
    }
}
