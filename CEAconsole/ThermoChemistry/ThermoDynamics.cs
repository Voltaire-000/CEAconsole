using CEAconsole.Models;
using CEAconsole.Services;
using MathNet.Numerics;
using MathNet.Numerics.Differentiation;
using MathNet.Numerics.Integration;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Newtonsoft.Json.Linq;
using ScottPlot.Colormaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vector = MathNet.Numerics.LinearAlgebra.Double.Vector;

namespace CEAconsole.ThermoChemistry
{

    // TODO add comments

    /// <summary>
    /// 
    /// </summary>
    public static class ThermoDynamics
    {

        //private static readonly double[]? coefficients;
        static readonly double Gas_Constant_R = 8.31446261815324;

        /// <summary>
        /// Returns the Log(Kf)
        /// </summary>
        /// <param name="deltaGibbsrxn"></param>
        /// <param name="Temperature"></param>
        /// <param name="GASCONSTANT"></param>
        /// <returns></returns>
        public static double Log_K(double deltaGibbsrxn, double Temperature, double GASCONSTANT = 8.31446261815324)
        {
            double result = -(deltaGibbsrxn * 1000) / (GASCONSTANT * Temperature);

            double x_ln = Math.Pow(Math.E, result);
            double ln = Math.Log10(x_ln);

            return ln;
        }

        public static double DeltaGibbsrxn(double ReferenceTemperature, double Temperature, IEnumerable<DTO_Specie> SpecieProperties, IEnumerable<DTO_Specie> ReferenceElements)
        {
            // TODO balance Equation
            var coeffMultipliers = ThermoDynamics.BalanceChemicalEquation(SpecieProperties, ReferenceElements);
            // fix elementAt(0) for temperature range
            double Hof = SpecieProperties.First().HeatOfFormation / 1000;
            var expnts = SpecieProperties.First().DataRecords.ElementAt(0).TExponents;
            var coeff = SpecieProperties.First().DataRecords.ElementAt(0).Coefficients;
            var integrateC = SpecieProperties.First().DataRecords.ElementAt(0).IntegrationConstants;

            double Entropy_specie = ThermoDynamics.Entropy(Temperature, expnts, coeff, integrateC);
            double Enthalpy_specie = ThermoDynamics.EnthalpyRefH298(ReferenceTemperature, Temperature, expnts, coeff);

            var chemicalFormulaCount = SpecieProperties.First().Molecule.ChemicalFormula.Count;
            for (int i = 0; i < chemicalFormulaCount; i++)
            {
                var reactantProperties = from item in ReferenceElements
                                         where item.Molecule.ChemicalFormula.ElementAt(0).Symbol
                                         == SpecieProperties.First().Molecule.ChemicalFormula.ElementAt(0).Symbol
                                         select item;
                var m_expnts = reactantProperties.First().DataRecords.ElementAt(0).TExponents;
                var m_coeff = reactantProperties.First().DataRecords.ElementAt(0).Coefficients;
                var m_integC = reactantProperties.First().DataRecords.ElementAt(0).IntegrationConstants;

                double m_enthalpy = EnthalpyRefH298(ReferenceTemperature, Temperature, m_expnts, m_coeff);

            }

            return 99.0;

        }

        public static MathNet.Numerics.LinearAlgebra.Vector<double> BalanceChemicalEquation(IEnumerable<DTO_Specie> specieProperties, IEnumerable<DTO_Specie> ReferenceElements)
        {
            List<double> e_S = new List<double>();
            List<double> e_Div = new List<double>();
            int elementCount = specieProperties.First().Molecule.ChemicalFormula.Count;
            // put the element into the list
            for (int i = 0; i < elementCount; i++)
            {
                double numAtoms = specieProperties.First().Molecule.ChemicalFormula.ElementAt(i).NumberOfAtoms;
                e_S.Add(numAtoms);
            }
            for (int i = 0; i < elementCount; i++)
            {
                var getDivisor = from reference in ReferenceElements
                                 where reference.Molecule.ChemicalFormula.ElementAt(0).Symbol
                                 == specieProperties.First().Molecule.ChemicalFormula.ElementAt(i).Symbol
                                 select reference;
                e_Div.Add(getDivisor.ElementAt(0).Molecule.ChemicalFormula.ElementAt(0).NumberOfAtoms);
            }

            Matrix<double> e_M = Matrix<double>.Build.Dense(elementCount + 1, elementCount + 1, 0.0);
            e_M[0, 0] = e_S[0] / e_Div[0]; e_M[0, 2] = -e_S[0]; // first atom
            e_M[1, 1] = e_S[1] / e_Div[1]; e_M[1, 2] = -e_S[1]; // second atom
            e_M[2, 2] = 1.0;
            // define the right hand side
            MathNet.Numerics.LinearAlgebra.Vector<double> b = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(new double[] { 0.0, 0.0, 1.0 });
            var solution = e_M.Solve(b);
            double min = solution.AbsoluteMinimum();
            if (min < 1.0)
            {
                double multiplier = 1.0 / min;
                solution.Multiply(multiplier, solution);
            }
            return solution;

        }

        public static double DeltaGibbsrxn(double Temperature, string Molecule)
        {
            var parsedMolecule = ParseChemicalEquation(Molecule);
            // TODO need to balance the equation before procedding
            // C(gr) + H^2 = CH4
            // C(gr) + 2 * H^2 = CH4

            // C(gr) + O^2 = CO2

            // C(gr) + O^2 = CO
            // C(gr) + 0.5 * O^2 = CO

            ICollection<ReferenceElement> refElements = InputServices.GetReferenceElements("Data/refElements.json");
            ICollection<DTO_Specie> NASAspecies = InputServices.GetNASA("Data/NASApolynomials.json");
            double Rsum_heatOfFormation = 0.0;
            double Psum_heatOfFormation = 0.0;
            double Rsum_entropy = 0.0;
            double Psum_Entropy = 0.0;
            Dictionary<string, double> element_Dict = new();

            for (int i = 0; i < parsedMolecule.Count; i++)
            {
                var elementData = from element in refElements
                                  where element.ChemicalFormula.ElementAt(0).Symbol == parsedMolecule.ElementAt(i).Key
                                  select element;
                List<double> tExpnts_ref = elementData.First().DataRecords.ElementAt(0).TExponents;
                List<double> coefficients_ref = elementData.First().DataRecords.ElementAt(0).Coefficients;
                List<double> integrationConstants_ref = elementData.First().DataRecords.ElementAt(0).IntegrationConstants;

                Rsum_heatOfFormation += EnthalpyFormation(Temperature, tExpnts_ref, coefficients_ref);
                Rsum_entropy = Entropy(Temperature, tExpnts_ref, coefficients_ref, integrationConstants_ref);
                double numberOfAtoms = elementData.First().ChemicalFormula.First().NumberOfAtoms;
                element_Dict.Add(elementData.First().Name, Rsum_entropy * numberOfAtoms);
                //Rsum_entropy = Rsum_entropy * elementData.First().ChemicalFormula.ElementAt(0).NumberOfAtoms;
            }
            var m_molecule = from specie in NASAspecies
                             where specie.Name == Molecule
                             select specie;
            var tExpnts = m_molecule.First().DataRecords.ElementAt(0).TExponents;
            var coeff = m_molecule.First().DataRecords.ElementAt(0).Coefficients;
            var integrationConstants = m_molecule.First().DataRecords.ElementAt(0).IntegrationConstants;
            var Enthalpy_Molecule = Enthalpy(Temperature, tExpnts, coeff, integrationConstants);

            Psum_heatOfFormation = Enthalpy_Molecule;
            double deltaHrxn = Psum_heatOfFormation - Rsum_heatOfFormation;

            // calculate deltaSrxn
            var Entropy_Molecule = Entropy(Temperature, tExpnts, coeff, integrationConstants);
            Psum_Entropy = Entropy_Molecule;
            double dictDum = element_Dict.Values.Sum();
            double deltaSrxn = Psum_Entropy - dictDum;

            double deltaGibbsRxn = deltaHrxn - (Temperature * (deltaSrxn / 1000));

            return deltaGibbsRxn;
        }
        // TODO need to change to Balanced equation??
        public static double DeltaHrxn(double Temperature, Dictionary<string, Molecule> reactants, Dictionary<string, Molecule> productChemicalFormula)
        {
            ICollection<ReferenceElement> refElements = InputServices.GetReferenceElements("Data/refElements.json");
            ICollection<DTO_Specie> NASAspecies = InputServices.GetNASA("Data/NASApolynomials.json");

            double Rsum_heatOfFormation = 0.0;
            double Psum_heatOfFormation = 0.0;

            foreach (var reactant in reactants)
            {
                var elementData = from element in refElements
                                  where element.Name == reactant.Key
                                  select element;
                List<double> tExpnts = elementData.First().DataRecords.ElementAt(0).TExponents;
                List<double> coefficients = elementData.First().DataRecords.ElementAt(0).Coefficients;
                List<double> integrationConstants = elementData.First().DataRecords.ElementAt(0).IntegrationConstants;

                // number of moles
                double moleculeCoefficient = reactant.Value.Count;
                Rsum_heatOfFormation += moleculeCoefficient * EnthalpyFormation(Temperature, tExpnts, coefficients);
            }

            foreach (var product in productChemicalFormula)
            {
                IEnumerable<DTO_Specie> productData = from specie in NASAspecies
                                                  where specie.Name == product.Key
                                                  select specie;
                List<double> tExpnts = productData.First().DataRecords.ElementAt(0).TExponents;
                List<double> coefficients = productData.First().DataRecords.ElementAt(0).Coefficients;
                List<double> integrationConstants = productData.First().DataRecords.ElementAt(0).IntegrationConstants;

                //  number of moles
                double moleculeCoefficient = product.Value.Count;
                Psum_heatOfFormation = moleculeCoefficient * Enthalpy(Temperature, tExpnts, coefficients, integrationConstants);
            }
            double deltaHrxn = Psum_heatOfFormation - Rsum_heatOfFormation;
            return deltaHrxn;
        }

        public static double DeltaSrxn(double Temperature, Dictionary<string, Molecule> reactants, Dictionary<string, Molecule> productChemicalFormula)
        {
            ICollection<ReferenceElement> refElements = InputServices.GetReferenceElements("Data/refElements.json");
            ICollection<DTO_Specie> NASAspecies = InputServices.GetNASA("Data/NASApolynomials.json");

            double Rsum_entropy = 0.0;
            double PsumEntropy = 0.0;

            foreach (var reactant in reactants)
            {
                var elementData = from element in refElements
                                  where element.Name == reactant.Key
                                  select element;
                List<double> tExpnts = elementData.First().DataRecords.ElementAt(0).TExponents;
                List<double> coefficients = elementData.First().DataRecords.ElementAt(0).Coefficients;
                List<double> integrationConstants = elementData.First().DataRecords.ElementAt(0).IntegrationConstants;
                // number of moles
                double moleculeCoefficient = reactant.Value.Count;
                Rsum_entropy += moleculeCoefficient * Entropy(Temperature, tExpnts, coefficients, integrationConstants);
            }

            foreach (var product in productChemicalFormula)
            {
                IEnumerable<DTO_Specie> productData = from specie in NASAspecies
                                                  where specie.Name == product.Key
                                                  select specie;
                List<double> tExpnts = productData.First().DataRecords.ElementAt(0).TExponents;
                List<double> coefficients = productData.First().DataRecords.ElementAt(0).Coefficients;
                List<double> integrationConstants = productData.First().DataRecords.ElementAt(0).IntegrationConstants;
                // number of moles
                double moleculeCoefficient = product.Value.Count;
                PsumEntropy += moleculeCoefficient * Entropy(Temperature, tExpnts, coefficients, integrationConstants);
            }
            return PsumEntropy - Rsum_entropy;
        }

        public static Dictionary<string, double> ParseChemicalEquation(string equation)
        {
            // this Dictionary will hold the element symbols and their coefficients
            Dictionary<string, double> elements = new();
            // REGEX to match the leading coefficient and elements with their coefficients
            Regex moleculePattern = new Regex("^(\\d+)([A-Z][a-z]*)");
            // Regex element pattern
            Regex elementPattern = new Regex("([A-Z][a-z]*)(\\d*)");
            // match the leading coefficient for the molecule
            Match moleculeMatch = moleculePattern.Match(equation);
            int moleculeCoefficient = moleculeMatch.Success ? int.Parse(moleculeMatch.Groups[1].Value) : 1;
            // remove the leading coeficient from the equation for further parsing
            equation = moleculePattern.Replace(equation, moleculeMatch.Groups[2].Value);
            // match all elements and coefficients
            foreach (Match match in elementPattern.Matches(equation))
            {
                string element = match.Groups[1].Value;
                int coefficient = match.Groups[2].Value == "" ? moleculeCoefficient : moleculeCoefficient * int.Parse(match.Groups[2].Value);
                // element is already in dictionary, add the coefficient, otherwise add the element to the dictionary
                if (elements.ContainsKey(element))
                {
                    elements[element] += coefficient;
                }
                else
                {
                    elements.Add(element, coefficient);
                }
            }
            return elements;
        }
                
        public static double Cp(double Temperature, IEnumerable<Specie> Specie, double GASCONSTANT = 8.31446261815324)
        {
            double Cp = 0.0;
            int recordCount = Specie.First().DataRecords.Count;
            for (int i = 0; i < recordCount; i++)
            {
                List<double> interval = Specie.First().DataRecords.ElementAt(i).TemperatureRange;
                double m_min = interval.Min();
                double m_max = interval.Max();
                if (Temperature >= m_min && Temperature <= m_max)
                {
                    int recordNumber = i;
                    var TemperatureExponents = Specie.First().DataRecords.ElementAt(recordNumber).TExponents;
                    var Coefficients = Specie.First().DataRecords.ElementAt(recordNumber).Coefficients;

                    double a1 = Coefficients[0];
                    double a2 = Coefficients[1];
                    double a3 = Coefficients[2];
                    double a4 = Coefficients[3];
                    double a5 = Coefficients[4];
                    double a6 = Coefficients[5];
                    double a7 = Coefficients[6];
                    Cp = GASCONSTANT * (a1 * Math.Pow(Temperature, TemperatureExponents[0])
                            + a2 * Math.Pow(Temperature, TemperatureExponents[1])
                            + a3
                            + a4 * Temperature
                            + a5 * Math.Pow(Temperature, TemperatureExponents[4])
                            + a6 * Math.Pow(Temperature, TemperatureExponents[5])
                            + a7 * Math.Pow(Temperature, TemperatureExponents[6]));
                    if (double.IsNaN(Cp))
                    {
                        Cp = 0.0;
                    }
                }
            }
            return Cp;
        }

        /// <summary>
        /// Returns the Heat Capacity Cp for the given Temperature, Coefficients, and Temperature Exponents
        /// </summary>
        /// <param name="T">Temperature in Kelvin</param>
        /// <param name="temperatureExponents">List of Temperature exponents from the NASA polynomials</param>
        /// <param name="coefficients">List of Coefficients from the NASA polynomials</param>
        /// <returns>Heat capacity (Cp) in J/mol-K</returns>
        public static double HeatCapacity(double Temperature, List<double> temperatureExponents, List<double> coefficients, double GASCONSTANT = 8.31446261815324)
        {
            if (Temperature < 0)
            {
                Temperature = 0;
            }
            double a1 = coefficients[0];
            double a2 = coefficients[1];
            double a3 = coefficients[2];
            double a4 = coefficients[3];
            double a5 = coefficients[4];
            double a6 = coefficients[5];
            double a7 = coefficients[6];

            double Cp = GASCONSTANT * (a1 * Math.Pow(Temperature, temperatureExponents[0])
                                        + a2 * Math.Pow(Temperature, temperatureExponents[1])
                                        + a3
                                        + a4 * Temperature
                                        + a5 * Math.Pow(Temperature, temperatureExponents[4])
                                        + a6 * Math.Pow(Temperature, temperatureExponents[5])
                                        + a7 * Math.Pow(Temperature, temperatureExponents[6]));
            if (double.IsNaN(Cp))
            {
                Cp = 0.0;
            }
            return Cp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceTemperature"></param>
        /// <param name="T1"></param>
        /// <param name="coefficients">List of Coefficients from the NASA polynomials</param>
        /// <param name="temperatureExponents">List of Temperature exponents from the NASA polynomials</param>
        /// <returns>Enthalpy (H-H298) in kJ/mol</returns>
        public static double EnthalpyRefH298(double referenceTemperature, double T1, List<double> tExpnts, List<double> coefficients)
        {
            double integrand(double T) => HeatCapacity(T, tExpnts, coefficients);
            double enthalpy = GaussKronrodRule.Integrate(integrand, referenceTemperature, T1, out double error, out double L1Norm, 1e-8) / 1000;
            return enthalpy;
        }

        /// <summary>
        /// Calculates Enthalpy (H) and returns the value in kJ/mol
        /// </summary>
        /// <param name="Temperature">Kelvin</param>
        /// <param name="temperatureExponents">List of Temperature exponents from the NASA polynomials</param>
        /// <param name="coefficients">List of Coefficients from the NASA polynomials</param>
        /// <param name="integrationConstants">List of integration constants from the NASA polynomials </param>
        /// <returns>(enthalpy)H kJ/mol</returns>
        public static double Enthalpy(double Temperature, List<double> temperatureExponents, List<double> coefficients, List<double> integrationConstants, double GASCONSTANT = 8.31446261815324)
        {
            double a1 = coefficients[0];
            double a2 = coefficients[1];
            double a3 = coefficients[2];
            double a4 = coefficients[3];
            double a5 = coefficients[4];
            double a6 = coefficients[5];
            double a7 = coefficients[6];
            //double a8 = coefficients[7];

            double i8 = integrationConstants[0];
            double i9 = integrationConstants[1];

            double enthalpy = GASCONSTANT * Temperature * (-a1 * Math.Pow(Temperature, temperatureExponents[0])
                               + a2 * Math.Pow(Temperature, temperatureExponents[1]) * Math.Log(Temperature)
                               + a3
                               + a4 * (Temperature / 2)
                               + a5 * (Math.Pow(Temperature, temperatureExponents[4]) / 3)
                               + a6 * (Math.Pow(Temperature, temperatureExponents[5]) / 4)
                               + a7 * (Math.Pow(Temperature, temperatureExponents[6]) / 5)
                               + (i8 / Temperature));
            if (double.IsNaN(enthalpy))
            {
                enthalpy = 0.0;
            }
            return enthalpy / 1000;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Temperature">Temperature in Kelvin</param>
        /// <param name="exponents">List of Temperature exponents from the NASA polynomials</param>
        /// <param name="coefficients">List of Coefficients from the NASA polynomials</param>
        /// <param name="integrationConstants">List of integration constants from the NASA polynomials</param>
        /// <param name="GASCONSTANT">Optional Universal Gas Constant = 8.31446261815324</param>
        /// <returns>Entropy S J/mol-K</returns>
        public static double Entropy(double Temperature, List<double> exponents, List<double> coefficients, List<double> integrationConstants, double GASCONSTANT = 8.31446261815324)
        {
            double a1 = coefficients[0];
            double a2 = coefficients[1];
            double a3 = coefficients[2];
            double a4 = coefficients[3];
            double a5 = coefficients[4];
            double a6 = coefficients[5];
            double a7 = coefficients[6];

            double integrationConstantZero = integrationConstants[0];
            double integrationConstantOne = integrationConstants[1];

            double entropy = GASCONSTANT * (-a1 * Math.Pow(Temperature, exponents[0]) / 2
              - a2 * Math.Pow(Temperature, exponents[1])
              + a3 * Math.Log(Temperature)
              + a4 * Temperature
              + a5 * Math.Pow(Temperature, exponents[4]) / 2
              + a6 * Math.Pow(Temperature, exponents[5]) / 3
              + a7 * Math.Pow(Temperature, exponents[6]) / 4
              + integrationConstantOne);
            if (double.IsNaN(entropy))
            {
                entropy = 0.0;
            }
            return entropy;

        }

        public static double DeltaGibbs(double Temperature, double DeltaHrxn, double DeltaSrxn)
        {
            return DeltaHrxn - (Temperature * (DeltaSrxn / 1000));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceTemperature">298.15 Kelvin</param>
        /// <param name="T_1">Temperature in Kelvin being tested</param>
        /// <param name="coefficients">List of Temperature Coefficients from the NASA polynomials</param>
        /// <param name="t_expnts">List of coefficient exponents from the NASA polynomials</param>
        /// <returns></returns>
        public static double GibbsRef(double referenceTemperature, double referenceEntropy, double T_1, List<double> coefficients, List<double> t_expnts)
        {
            double enthalpyIntegrand(double T) => HeatCapacity(T, t_expnts, coefficients);
            double entropyIntegrand(double T) => HeatCapacity(T, t_expnts, coefficients) / T;
            double enthalpy = GaussKronrodRule.Integrate(enthalpyIntegrand, referenceTemperature, T_1, out double error, out double L1Norm, 1e-8) / 1000;
            double entropy = GaussKronrodRule.Integrate(entropyIntegrand, referenceTemperature, T_1, out error, out L1Norm, 1e-8);
            entropy += referenceEntropy;

            double gibbs = -(enthalpy * 1000 - T_1 * entropy) / T_1;
            // TODO not what i want 
            if (double.IsNegativeInfinity(gibbs))
            {
                return gibbs = 0.0;
            }
            return gibbs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="refTemp">298.15 Kelvin</param>
        /// <param name="Temperature">Temperature in Kelvin</param>
        /// <param name="coefficients">List of Temperature Coefficients from the NASA polynomials</param>
        /// <param name="tExpnts">List of coefficient exponents from the NASA polynomials</param>
        /// <returns>Enthalpy H-H298 kJ/mol</returns>
        public static double EnthalpyFormation(double Temperature, List<double> tExpnts, List<double> coefficients)
        {
            double ReferenceTemperature = 298.15;
            double heatCapacityIntegrand(double T) => HeatCapacity(T, tExpnts, coefficients);
            return GaussKronrodRule.Integrate(heatCapacityIntegrand, ReferenceTemperature, Temperature, out double error, out double L1Norm, 1e-8) / 1000;
        }

        public static Dictionary<string, CPHSRef> ElementsReferenceCPHS(ICollection<DTO_Specie> Elements)
        {
            Dictionary<string, CPHSRef> keyValuePairs = new();
            double TR = 298.15;
            for (int i = 0; i < Elements.Count; i++)
            {
                var NASAchemicalFormula = Elements.ElementAt(i).Molecule.ChemicalFormula;
                var temperatureRange = Elements.ElementAt(i).DataRecords.ElementAt(0).TemperatureRange;
                var coefficients = Elements.ElementAt(i).DataRecords.ElementAt(0).Coefficients;
                var integrationConstants = Elements.ElementAt(i).DataRecords.ElementAt(0).IntegrationConstants;
                var t_expnts = Elements.ElementAt(i).DataRecords.ElementAt(0).TExponents;

                string Species_Name = Elements.ElementAt(i).Name;
                double Molecular_Weight = Elements.ElementAt(i).MolecularWeight;
                double Enthalpy = Elements.ElementAt(i).HeatOfFormation - (Elements.ElementAt(i).DataRecords.ElementAt(0).EnthalpyRef / 1000);
                double Delta_Enthalpy = Elements.ElementAt(0).HeatOfFormation - (Elements.ElementAt(0).DataRecords.ElementAt(0).EnthalpyRef / 1000);
                double Delta_Enthalpy_Ref = Elements.ElementAt(0).HeatOfFormation;
                double Cp_Ref = ThermoDynamics.HeatCapacity(TR, t_expnts, coefficients);
                double EnthalpyRef = Elements.ElementAt(i).DataRecords.ElementAt(0).EnthalpyRef / 1000;
                double Entropy_Ref = ThermoDynamics.Entropy(TR, t_expnts, coefficients, integrationConstants);

                CPHSRef cPHSRef = new()
                {
                    Species_Name = Species_Name,
                    Molecular_Weight = Molecular_Weight,
                    Enthalpy = Enthalpy,
                    Delta_Enthalpy = Delta_Enthalpy,
                    Delta_Enthalpy_Ref = Delta_Enthalpy_Ref,
                    CP_Ref = Cp_Ref,
                    EnthalpyRef = EnthalpyRef,
                    Entropy_Ref = Entropy_Ref
                };

                keyValuePairs.Add(Elements.ElementAt(i).Name, cPHSRef);

            }
            return keyValuePairs;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ElementSymbols"></param>
        /// <param name="Elements"></param>
        /// <returns></returns>
        //public static Dictionary<string, CPHSRef> ElementsReferenceCPHS(List<string> ElementSymbols, ICollection<DTO_Specie> Elements)
        //{
        //    Dictionary<string, CPHSRef> keyValuePairs = new();
        //    double TR = 298.15;
        //    foreach (var elementSymbol in ElementSymbols)
        //    {
        //        var elementProperties = from element in Elements
        //                                where element != null && element.Molecule.ChemicalFormula.First().Symbol == elementSymbol
        //                                select element;
        //        var NASAchemicalFormula = elementProperties.First().ChemicalFormula;
        //        var temperatureRange = elementProperties.First().DataRecords.ElementAt(0).TemperatureRange;
        //        var coefficients = elementProperties.First().DataRecords.ElementAt(0).Coefficients;
        //        var integrationConstants = elementProperties.First().DataRecords.ElementAt(0).IntegrationConstants;
        //        var t_expnts = elementProperties.First().DataRecords.ElementAt(0).TExponents;

        //        string Species_Name = elementProperties.First().Name;
        //        double Molecular_Weight = elementProperties.First().MolecularWeight;
        //        double Enthalpy = elementProperties.First().HeatOfFormation - (elementProperties.First().DataRecords.ElementAt(0).EnthalpyRef / 1000);
        //        double Delta_Enthalpy = elementProperties.First().HeatOfFormation - (elementProperties.First().DataRecords.ElementAt(0).EnthalpyRef / 1000);
        //        double Delta_Enthalpy_Ref = elementProperties.First().HeatOfFormation;
        //        double Cp_Ref = ThermoDynamics.HeatCapacity(TR, t_expnts, coefficients);
        //        double EnthalpyRef = elementProperties.First().DataRecords.ElementAt(0).EnthalpyRef / 1000;
        //        double Entropy_Ref = ThermoDynamics.Entropy(TR, t_expnts, coefficients, integrationConstants);

        //        CPHSRef cPHSRef = new()
        //        {
        //            Species_Name = Species_Name,
        //            Molecular_Weight = Molecular_Weight,
        //            Enthalpy = Enthalpy,
        //            Delta_Enthalpy = Delta_Enthalpy,
        //            Delta_Enthalpy_Ref = Delta_Enthalpy_Ref,
        //            CP_Ref = Cp_Ref,
        //            EnthalpyRef = EnthalpyRef,
        //            Entropy_Ref = Entropy_Ref
        //        };

        //        keyValuePairs.Add(Species_Name, cPHSRef);
        //    }
        //    return keyValuePairs;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static MathNet.Numerics.LinearAlgebra.Vector<double> BalanceHydrocarbonEquation(Matrix<double> matrix)
        {
            int numRows = matrix.RowCount;
            int numColumns = matrix.ColumnCount;
            int nonZeroValues = 0;
            int ii = 0;
            double[] values = new double[] { };
            IEnumerable<(int, MathNet.Numerics.LinearAlgebra.Vector<double>)> m_enumeratedRows = matrix.EnumerateRowsIndexed();
            for (int i = 0; i < numRows; i++)
            {
                var m_elementAtRow = m_enumeratedRows.ElementAt(i);
                int m_rowNumber = m_elementAtRow.Item1;
                MathNet.Numerics.LinearAlgebra.Vector<double> m_rowVector = m_elementAtRow.Item2;

                foreach (var item in m_rowVector)
                {
                    int m_compare = item.CompareTo(0.0);
                    if (m_compare != 0)
                    {

                        nonZeroValues++;
                    }
                }
            }

            //MathNet.Numerics.LinearAlgebra.Vector<double> m_values = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(nonZeroValues);
            double[] m_values = new double[nonZeroValues];
            for (int i = 0; i < numRows; i++)
            {
                var m_elementAtRow = m_enumeratedRows.ElementAt(i);
                int m_rowNumber = m_elementAtRow.Item1;
                MathNet.Numerics.LinearAlgebra.Vector<double> m_rowVector = m_elementAtRow.Item2;

                foreach (var item in m_rowVector)
                {
                    int m_compare = item.CompareTo(0.0);
                    if (m_compare != 0)
                    {
                        m_values[ii] = item;
                        ii++;
                    }
                }

            }

            //MathNet.Numerics.LinearAlgebra.Vector<int> IA = MathNet.Numerics.LinearAlgebra.Vector<int>.Build.Dense(numRows + 1);
            int[] IA = new int[numRows + 1];
            //MathNet.Numerics.LinearAlgebra.Vector<int> JA = MathNet.Numerics.LinearAlgebra.Vector<int>.Build.Dense(nonZeroValues);
            int[] JA = new int[nonZeroValues];
            int jj = 0;
            for (int i = 0; i < numRows; i++)
            {
                var m_elementAtRow = m_enumeratedRows.ElementAt(i);
                int m_rowNumber = m_elementAtRow.Item1;
                MathNet.Numerics.LinearAlgebra.Vector<double> m_rowVector = m_elementAtRow.Item2;
                int Row_non_zero_values = 0;
                int column_where_value_found = 0;

                foreach (var item in m_rowVector)
                {
                    var m_compare = item.CompareTo(0.0);
                    if (m_compare != 0)
                    {
                        // increment valuesCount
                        Row_non_zero_values++;
                        // what column was it found in
                        // { 0, 2, 0, 3, 1, 2, 3, 0 };
                        //int m_c = column_where_value_found;

                        JA[jj] = column_where_value_found;
                        jj++;

                    }
                    column_where_value_found++;

                }
                IA[i + 1] = IA[i] + Row_non_zero_values;
            }

            Matrix<double> A = Matrix.Build.SparseFromCompressedSparseRowFormat(numRows, numColumns, m_values.Length, IA, JA, m_values);

            // Define the right hand side
            MathNet.Numerics.LinearAlgebra.Vector<double> b = Vector.Build.Dense(new double[] { 0, 0, 1.0 });
            // solution
            MathNet.Numerics.LinearAlgebra.Vector<double> solution = A.Solve(b);
            return solution;
        }

        public static double Calculate_MU(double gibbs, double temperature, double pressure)
        {
            // MU = G + 
            //double standardChemicalPotential = 0.0;
            double activity = 1.0;
            double standardActivity = 1.0;

            double chemicalpotential = gibbs + Gas_Constant_R * temperature * Math.Log(activity / standardActivity);

            return chemicalpotential;

        }

        public static double DeltaHf(double productEnthalpy, List<double> reactants)
        {
            return productEnthalpy - (reactants[0] + reactants[1]);
        }

        //public static double DeltaHf(ICollection<ChemicalFormula> chemFormula)
        //{
        //    Dictionary<string, Molecule> Reactants = new();
        //    Dictionary<string, Molecule> Products = new();

        //    Products.Add(chemFormula);


        //    //Molecule productMolecule = new()
        //    //{
        //    //    Count = 1,
        //    //    ChemicalFormula = chemFormula
        //    //}
        //}
    }
}
