using CEAconsole.Models;
using CEAconsole.Services;
using CEAconsole.ViewModels;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Solvers;
using MathNet.Numerics.LinearAlgebra.Double.Solvers;
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using MathNet.Numerics.Providers.SparseSolver;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Symbolics;
using System.Collections.Generic;
using MathNet.Numerics.Distributions;
using System.Xml.Linq;
using ScottPlot.Colormaps;
using System.Text.RegularExpressions;
using CEAconsole.ThermoChemistry;
using CEAconsole.ThermoChemistry.Utilities;
using MathNet.Numerics;
using System.Globalization;

namespace TestCEAconsole
{

    [TestClass]
    public class TestLearnChemE
    {
        [TestMethod]
        public void TestCO2GibbsRef()
        {
            //    "Species_Name": "CO2",
            //"Molecular_Weight": 44.0095,
            //"Enthalpy": -402.875,
            //"Delta_Enthalpy": -393.142,
            //"Delta_Enthalpy_Ref": -393.51,
            //"CP_Ref": 37.135,
            //"EnthalpyRef": 9.365,
            //"Entropy_Ref": 213.787

            // DeltaHf ref, DeltaS ref, Temperature
            double deltaHfref = -393.15;
            double deltaSref = 231.787;
            double deltaGref = deltaHfref - 298.15 / deltaSref;
            Assert.AreEqual(-394.4363, deltaGref, 0.001);
        }

        [TestMethod]
        public void TestNASACoeffVsShomate()
        {
            string NASAsearchString = "CH4";
            double TR = 298.15;
            double T = 298.15;
            double delta = 0.005;

            ICollection<Specie> nasaPolynomials = InputServices.GetNASA("Data/NASApolynomials.json");
            IEnumerable<Specie> NASA_specie = from NASAspecie in nasaPolynomials
                                              where NASAspecie.Name == "CO2" | NASAspecie.Name == "CO" | NASAspecie.Name == "H2O" | NASAspecie.Name == "H2"
                                              select NASAspecie;
            Dictionary<string, double> Cp_Specie = new();
            Dictionary<string, double> Href_Specie = new();
            Dictionary<string, double> Enthalpy_Specie = new();
            foreach (var item in NASA_specie)
            {
                var exponents = item.DataRecords.ElementAt(0).TExponents;
                var coeff = item.DataRecords.ElementAt(0).Coefficients;
                var integration = item.DataRecords.ElementAt(0).IntegrationConstants;
                double Cp = ThermoDynamics.HeatCapacity(T, exponents, coeff);
                Cp_Specie.Add(item.Name, Cp);
                double Href = ThermoDynamics.EnthalpyRefH298(TR, T, coeff, exponents);
                Href_Specie.Add(item.Name, Href);
                double enthalpy = ThermoDynamics.Enthalpy(T, exponents, coeff, integration);
                Enthalpy_Specie.Add(item.Name, enthalpy);
            }
            // Get the heat capacity of the Elements : C, H2, O2
            var NASA_Elements = from NASAelements in nasaPolynomials
                                where NASAelements.Name == "C(gr)" | NASAelements.Name == "O2" | NASAelements.Name == "H2"
                                select NASAelements;

            Dictionary<string, double> Cp_elements = new();
            Dictionary<string, double> Enthalpy_Element = new();
            foreach (var item in NASA_Elements)
            {
                var exponents = item.DataRecords.ElementAt(0).TExponents;
                var coeff = item.DataRecords.ElementAt(0).Coefficients;
                var integration = item.DataRecords.ElementAt(0).IntegrationConstants;
                double Cp = ThermoDynamics.HeatCapacity(T, exponents, coeff);
                Cp_elements.Add(item.Name, Cp);
                double enthalpy = ThermoDynamics.Enthalpy(T, exponents, coeff, integration);
                Enthalpy_Element.Add(item.Name, enthalpy);
            }
            // calculate deltaA 
            Dictionary<string, double> specie_A = new();
            foreach (var item in NASA_specie)
            {
                var coeff = item.DataRecords.ElementAt(0).Coefficients.ElementAt(0);
                specie_A.Add(item.Name, coeff);
            }
            Dictionary<string, double> element_A = new();
            foreach (var item in NASA_Elements)
            {
                var coeff = item.DataRecords.ElementAt(0).Coefficients.ElementAt(0);
                element_A.Add(item.Name, coeff);
            }
            double coefCO2;
            Enthalpy_Specie.TryGetValue("CO2", out coefCO2);
            double coefO2;
            Enthalpy_Element.TryGetValue("O2", out coefO2);
            double coefH2;
            Enthalpy_Element.TryGetValue("H2", out coefH2);
            double coefCgr;
            Enthalpy_Element.TryGetValue("C(gr)", out coefCgr);
            double deltaA = coefCO2 - coefO2 - coefCgr;
            double xx = (coefO2 + coefCgr) - 393.51;

            //Assert.AreEqual(37.152, 99);

        }

        [DataTestMethod]
        [DataRow(298.0, -28.6)]
        [DataRow(400.0, -24.3747)]
        [DataRow(500.0, -20.3842)]
        [DataRow(600.0, -16.5484)]
        [DataRow(700.0, -12.8566)]
        [DataRow(1000.0, -2.4416)]
        public void TestGibbsAsFunctionOfTemperature(double T, double expected)
        {
            // delta
            double delta = 0.015;
            // Arrange
            //double T = 298.0;       // Kelvin
            double TR = 298.0;      // Kelvin
            //double Rg = 8.31e-03;   // kJ/mol K
            double Rg = 8.31446261815324 / 1000;
            // Shomate Coefficients for molecules
            double A_CO2 = 22.243; double B_CO2 = 5.98e-02; double C_CO2 = -3.50e-05; double D_CO2 = 7.46e-09;
            double A_CO = 28.142; double B_CO = 1.67e-03; double C_CO = 5.37e-06; double D_CO = -2.22e-09;
            double A_H2O = 32.218; double B_H2O = 1.92e-03; double C_H2O = 1.06e-05; double D_H2O = 3.56e-09;
            double A_H2 = 29.088; double B_H2 = -1.92e-03; double C_H2 = 4.00e-06; double D_H2 = -8.70e-10;
            // Shomate coefficients for elements
            double A_O2 = 25.46; double B_O2 = 1.52e-02; double C_O2 = -7.15e-06; double D_O2 = 1.31e-09;
            double A_C = 8.43; double B_C = 0.00e+00; double C_C = 0.00e+00; double D_C = 0.00e+00;

            // Cp for Molecules
            double Cp_CO2 = A_CO2 + (B_CO2 * T) + (C_CO2 * Math.Pow(T, 2)) + (D_CO2 * Math.Pow(T, 3));
            //Assert.AreEqual(37.14, Cp_CO2, delta); // actual 37.152
            double Cp_CO = A_CO + (B_CO * T) + (C_CO * Math.Pow(T, 2)) + (D_CO * Math.Pow(T, 3));
            //Assert.AreEqual(29.06, Cp_CO, delta);  // actual 29.0577
            double Cp_H2O = A_H2O + (B_H2O * T) + (C_H2O * Math.Pow(T, 2)) + (D_H2O * Math.Pow(T, 3));
            //Assert.AreEqual(33.63, Cp_H2O, 0.2);  // actual 33.8256
            double Cp_H2 = A_H2 + (B_H2 * T) + (C_H2 * Math.Pow(T, 2)) + (D_H2 * Math.Pow(T, 3));
            //Assert.AreEqual(28.85, Cp_H2, delta);   // actual 28.8480

            // calculate Deltas for CO2
            double deltaA_CO2 = A_CO2 - A_C - A_O2; //Assert.AreEqual(-11.647, deltaA_CO2, delta);    // J8
            double deltaB_CO2 = B_CO2 - B_C - B_O2; //Assert.AreEqual(4.46e-02, deltaB_CO2);
            double deltaC_CO2 = C_CO2 - C_C - C_O2; //Assert.AreEqual(-2.78e-05, deltaC_CO2, delta);
            double deltaD_CO2 = D_CO2 - D_C - D_O2; // Assert.AreEqual(6.15e-09, deltaD_CO2, delta);
            // from Ref_Defaults
            double Delta_Enthalpy_Ref_CO2 = -393.51;
            // J = deltaH_R - deltaAT_R - deltaB/2 * T_R^2 - deltaC/3 * T_R^3 - deltaD/4 * T_R^4
            //  =N8+(-J8*TR-K8*TR^2/2-L8*TR^3/3-M8*TR^4/4)/1000
            // Important J is calculated at the Reference Temperature TR of 298
            double J_CO2 = Delta_Enthalpy_Ref_CO2 + (((-deltaA_CO2 * TR) - (deltaB_CO2 * Math.Pow(TR, 2) / 2) - (deltaC_CO2 * Math.Pow(TR, 3) / 3) - (deltaD_CO2 * Math.Pow(TR, 4) / 4)) / 1000);
            //Assert.AreEqual(-391.8, J_CO2, 0.2);
            // I = 
            // (1/Rg)*(-O8/TR+(J8*LN(TR)+K8*TR/2+L8*TR^2/6+M8*TR^3/12)/1000)
            // Important I is calculated at the Reference Temperature TR of 298
            double I_CO2 = (1 / Rg) * (-J_CO2 / TR + (deltaA_CO2 * Math.Log(TR) + deltaB_CO2 * TR / 2 + deltaC_CO2 * Math.Pow(TR, 2) / 6 + deltaD_CO2 * Math.Pow(TR, 3) / 12) / 1000);
            //Assert.AreEqual(150.97, I_CO2, delta);
            // deltaGref can be calculated from Ref_Defaults
            double deltaGref_CO2 = -394.4;
            double deltaGof_RTR_CO2 = deltaGref_CO2 / (Rg * TR);
            Assert.AreEqual(-159.19, deltaGof_RTR_CO2, 0.1);
            //  Q8+P8+(1/Rg)*(O8/T+(-J8*LN(T)-K8*T/2-L8*T^2/6-M8*T^3/12)/1000)
            double deltaGof_T_RT_CO2 = deltaGof_RTR_CO2 + I_CO2 + (1 / Rg) * (J_CO2 / T + (-deltaA_CO2 * Math.Log(T) - deltaB_CO2 * T / 2 - deltaC_CO2 * Math.Pow(T, 2) / 6 - deltaD_CO2 * Math.Pow(T, 3) / 12) / 1000);
            //Assert.AreEqual(-159.19, deltaGof_T_RT_CO2, 0.1);
            //  equal to "Delta_Enthalpy_Ref" CO2: -393.51 at standard Temperature
            //  =O8+(J8*T+K8*T^2/2+L8*T^3/3+M8*T^4/4)/1000
            double deltaHf_T_CO2 = J_CO2 + (deltaA_CO2 * T + deltaB_CO2 * Math.Pow(T, 2) / 2 + deltaC_CO2 * Math.Pow(T, 3) / 3 + deltaD_CO2 * Math.Pow(T, 4) / 4) / 1000;
            //Assert.AreEqual(-393.51, deltaHf_T_CO2, delta);
            double deltaGof_T_CO2 = deltaGof_T_RT_CO2 * Rg * T;
            //Assert.AreEqual(-394.40, deltaGof_T_CO2, delta);

            // Calculate deltas for CO, = C + 0.5O_2 -> C) =E9-0.5*E21-E22
            double deltaA_CO = A_CO - 0.5 * A_O2 - A_C; // expected 6.982
            //Assert.AreEqual(6.982, deltaA_CO, delta);
            double deltaB_CO = B_CO - 0.5 * B_O2 - B_C; // -5.93e-03
            //Assert.AreEqual(-5.93e-03, deltaB_CO, delta);
            double deltaC_CO = C_CO - 0.5 * C_O2 - C_C; // 8.95e-06
            double deltaD_CO = D_CO - 0.5 * D_O2 - D_C;  // -2.88e-09
            //  Delta_Enthalpy_Ref" CO : -110.535
            double Delta_Enthalpy_Ref_CO = -110.535;
            double J_CO = Delta_Enthalpy_Ref_CO + (((-deltaA_CO * TR) - (deltaB_CO * Math.Pow(TR, 2) / 2) - (deltaC_CO * Math.Pow(TR, 3) / 3) - (deltaD_CO * Math.Pow(TR, 4) / 4)) / 1000);     // -112.4
            double I_CO = 1 / Rg * (-J_CO / TR + (deltaA_CO * Math.Log(TR) + deltaB_CO * TR / 2 + deltaC_CO * Math.Pow(TR, 2) / 6 + deltaD_CO * Math.Pow(TR, 3) / 12) / 1000);    // 50.1
            // deltaGref can be calculated from Ref_Defaults
            double deltaGref_CO = -137.2;
            double deltaGof_RTR_CO = deltaGref_CO / (Rg * TR);  // -55.38
            double deltaGof_T_RT_CO = deltaGof_RTR_CO + I_CO + 1 / Rg * (J_CO / T + (-deltaA_CO * Math.Log(T) - deltaB_CO * T / 2 - deltaC_CO * Math.Pow(T, 2) / 6 - deltaD_CO * Math.Pow(T, 3) / 12) / 1000); //  -55.38
            double deltaHf_T_CO = J_CO + (deltaA_CO * T + deltaB_CO * Math.Pow(T, 2) / 2 + deltaC_CO * Math.Pow(T, 3) / 3 + deltaD_CO * Math.Pow(T, 4) / 4) / 1000;    // -110.5
            double deltaGof_T_CO = deltaGof_T_RT_CO * Rg * T;   // -137.20
            // H2O section start
            // calculate Deltas for H2O
            //  0.5 * A_O2 because only 1 atom of Oxygen per molecule of H2O
            double deltaA_H2O = A_H2O - A_H2 - (0.5 * A_O2); // -9.6
            double deltaB_H2O = B_H2O - B_H2 - (0.6 * B_O2);    //  -5.27e-03
            double deltaC_H2O = C_H2O - C_H2 - (0.5 * C_O2);    // 1.01e-05
            double deltaD_H2O = D_H2O - D_H2 - (0.5 * D_O2);    // - 3.35e-09
            //  "Delta_Enthalpy_Ref" H2O : -241.826,
            double Delta_Enthalpy_Ref_H2O = -241.826;
            double J_H2O = Delta_Enthalpy_Ref_H2O + (((-deltaA_H2O * TR) - (deltaB_H2O * Math.Pow(TR, 2) / 2) - (deltaC_H2O * Math.Pow(TR, 3) / 3) - (deltaD_H2O * Math.Pow(TR, 4) / 4)) / 1000);  // -238.8
            double I_H2O = 1 / Rg * ((-J_H2O / TR) + (((deltaA_H2O * Math.Log(TR)) + (deltaB_H2O * TR / 2) + (deltaC_H2O * Math.Pow(TR, 2) / 6) + (deltaD_H2O * Math.Pow(TR, 3) / 12)) / 1000));  // 89.7
            // deltaGref can be calculated from Ref_Defaults
            double deltaGref_H2O = -228.6;
            double deltaGof_RTR_H2O = deltaGref_H2O / (Rg * TR);    //  -92.27
            double deltaGof_T_RT_H2O = deltaGof_RTR_H2O + I_H2O + (1 / Rg * ((J_H2O / T) + (((-deltaA_H2O * Math.Log(T)) - (deltaB_H2O * T / 2) - (deltaC_H2O * Math.Pow(T, 2) / 6) - (deltaD_H2O * Math.Pow(T, 3) / 12)) / 1000)));// -92.27
            double deltaHf_T_H2O = J_H2O + (((deltaA_H2O * T) + (deltaB_H2O * Math.Pow(T, 2) / 2) + (deltaC_H2O * Math.Pow(T, 3) / 3) + (deltaD_H2O * Math.Pow(T, 4) / 4)) / 1000); // -241.8
            double deltaGof_T_H2O = deltaGof_T_RT_H2O * Rg * T; // -228.6
            // H2O section end
            // H2 section
            // calculate Deltas for H2 are all Zero because it is Element
            double deltaA_H2 = 0.0;
            double deltaB_H2 = 0.0;
            double deltaC_H2 = 0.0;
            double deltaD_H2 = 0.0;
            //  "Delta_Enthalpy_Ref" H2 : 0.0,
            double Delta_Enthalpy_Ref_H2 = 0.0;
            double J_H2 = 0.0;
            double I_H2 = 0.0;
            double deltaGref_H2 = 0.0;
            double deltaGof_RTR_H2 = 0.0;
            double deltaGof_T_RT_H2 = 0.0;
            double deltaHf_T_H2 = 0.0;
            double deltaGof_T_H2 = 0.0;
            // H2 section end
            // Cp for Elements
            double Cp_O2 = A_O2 + (B_O2 * T) + (C_O2 * Math.Pow(T, 2)) + (D_O2 * Math.Pow(T, 3));
            //Assert.AreEqual(29.39, Cp_O2, delta);   // 29.3893
            double Cp_C = A_C + (B_C * T) + (C_C * Math.Pow(T, 2)) + (D_C * Math.Pow(T, 3));
            //Assert.AreEqual(8.43, Cp_C, delta); // actual 8.43
            //  Reaction numbers
            double deltaH_rxn = deltaHf_T_CO2 - deltaHf_T_CO - deltaHf_T_H2O - deltaHf_T_H2;    // -41.2 at 298
            //Assert.AreEqual(expected, deltaH_rxn, delta);
            double deltaG_rxn = deltaGof_T_CO2 - deltaGof_T_CO - deltaGof_T_H2O - deltaGof_T_H2;    // -28.6 at 298
            Assert.AreEqual(expected, deltaG_rxn, delta);

        }
    }

    [TestClass]
    public class TestGaussianEliminationMethods
    {
        [TestMethod]
        public void TestAugmentedMatrixRowOperations()
        {
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new[,]{
                { 2.0, 1.0, -1.0, 5.0},
                { 4.0, -3.0, 2.0, 3.0},
                { 1.0, 2.0, 3.0, 10.0}
               });
            Assert.IsNotNull(matrix);

            Matrix<double> matrix2 = Matrix<double>.Build.DenseOfArray(new[,]{
                { 2.0, 1.0, -1.0},
                { 4.0, -3.0, 2.0},
                { 1.0, 2.0, 3.0}
               });

            var Row1 = matrix.Row(0);
            var Row2 = matrix.Row(1);
            var Row3 = matrix.Row(2);
            Row2 = Row2 - matrix.At(1, 0) / matrix.At(0, 0) * Row1;
            matrix.SetRow(1, Row2);
            Row3 = Row3 - matrix.At(2, 0) / matrix.At(0, 0) * Row1;
            matrix.SetRow(2, Row3);
            // diagonal
            var diagonal = matrix.Diagonal();
            // make pivot element
            Row2 = Row2 / -5;
            matrix.SetRow(1, Row2);
            diagonal = matrix.Diagonal();
            var lowerTriangleStrict = matrix.StrictlyLowerTriangle();
            Row3 = Row3 - 1.5 * Row2;
            matrix.SetRow(2, Row3);
            lowerTriangleStrict = matrix.StrictlyLowerTriangle();
            diagonal = matrix.Diagonal();

            double m_z = matrix.At(2, 3) / matrix.At(2, 2);
            double m_y = matrix.At(1, 3) + Math.Abs(matrix.At(1, 2)) * m_z;
            double m_x = (5 - (m_y - m_z)) / 2;
            Vector<double> result = Vector<double>.Build.Dense(3, 0.0);
            Vector<double> input = Vector<double>.Build.Dense([5.0, 3.0, 10.0]);
            var m_test = matrix2.Solve(input);

            Assert.IsNotNull(result);

        }
    }

    [TestClass]
    public class TestFilters
    {

        [TestMethod]
        public void ProdFilterShouldReturnOnlyList()
        {
            string json = InputCardService.GetInputCard();

            string[]? Prod = ProdFilter.ExtractProducts(json);
            string ar = Prod[0].ToString();

            Assert.AreEqual(20, Prod.Length);
            Assert.AreEqual("Ar", ar);
        }

        [TestMethod]
        public void TestICollection()
        {
            ICollection<Reactant> reactants = InputServices.GetSpecies("Data/newShortThermo.json");

            List<Reactant>? filteredCollection = reactants?.Where(item => item.Name == "CH4").ToList();
            var molecularWeight = (from item in filteredCollection
                                   select item.MolecularWeight).FirstOrDefault();
            double expected = 16.0424600;

            Dictionary<string, CEAconsole.Models.DataRecord>.ValueCollection? tempRange = (from item in filteredCollection
                                                                                           select item.TemperatureRange.Values).FirstOrDefault();

            Dictionary<string, double>? chemFormula = (from item in filteredCollection
                                                       select item.Molecule.ChemicalFormula).FirstOrDefault();
            int? elementCount = chemFormula?.Count;
            elementCount ??= 0;
            string? symbol = chemFormula?.ElementAt(0).Key;
            symbol ??= string.Empty;
            double? atoms = chemFormula?.ElementAt(0).Value;
            atoms ??= 0;

            CEAconsole.Models.DataRecord? mx = tempRange?.ElementAt(0);

            Assert.AreEqual(expected, molecularWeight);
            //Assert.AreEqual(99, tempRange.ElementAt(1));

        }

        [TestMethod]
        public void TestShouldReturnListOfPropertiesForOneType()
        {
            ICollection<CPHSRef> CPHSDefaults = InputServices.GetDefaultCPHS("Data/Ref_Defaults.json");
            var defList = from item in CPHSDefaults.Where(r => r.Species_Name == "CH4") select item;

            double molecularWeight = (from item in CPHSDefaults.Where(r => r.Species_Name == "CH4")
                                      select item.Molecular_Weight).FirstOrDefault();

            var enthalpy = (from item in CPHSDefaults.Where(r => r.Species_Name == "CH4")
                            select item.Enthalpy).FirstOrDefault();

            Assert.AreEqual(16.04246, molecularWeight);
            Assert.AreEqual(-84.616, enthalpy);
            Assert.AreEqual(1, defList.Count<CPHSRef>());
        }

    }

    [TestClass]
    public class Test_Matrix_Methods
    {
        [TestMethod]
        public void TestPerplexity_Compressed_SparseColumn_Method()
        {
            // create a sparse matrix
            //double[,] xvalues = new double[,]
            //{
            //    {1, 0, 3 },
            //    {0, 2, 0},
            //    {4, 0, 5 }
            //};

            // Compressed Sparse Column format
            // column major
            double[] values = new double[] { 1, 3, 4, 2, 5 };
            int[] rowIndices = new int[] { 0, 0, 2, 1, 2 };
            int[] columnPointers = new int[] { 0, 2, 3, 5 };

            Matrix<double> A = Matrix.Build.SparseFromCompressedSparseColumnFormat(3, 3, values.Length, rowIndices, columnPointers, values);
            // define the right hand side
            Vector<double> b = Vector.Build.Dense(new double[] { 6, 4, 10 });
            // solve the equation
            Vector<double> solution = A.Solve(b);   // 1.5, 0, 2
            Vector<double> expected = Vector.Build.Dense(new double[] { 1.5, 0.0, 2.0 });

            Assert.AreEqual(expected, solution);

            Assert.AreEqual(2, solution[2]);

        }
        [TestMethod]
        public void TestSparseMatrix_Compressed_Sparse_Row_Method()
        {
            double[] nonZeroValues = new double[] { 1, 3, 2, 4, 5 };
            // IA vector has a size of m + 1, where m is the number of rows in the matrix
            // it stores the cumulative number of non-zero elements up to ( but not including) the i-th row
            // IA[0] = 0, IA[i] = IA[i-1] + number of non-zero elements in the (i-1)-th row in the matrix
            int[] IA_rowPointers = new int[] { 0, 2, 3, 5 };
            int[] JA_columnIndices = new int[] { 0, 2, 1, 0, 2 }; // the column that holds value


            Matrix<double> A = Matrix.Build.SparseFromCompressedSparseRowFormat(3, 3, nonZeroValues.Length, IA_rowPointers, JA_columnIndices, nonZeroValues);
            // define the right hand side
            Vector<double> b = Vector.Build.Dense(new double[] { 6, 4, 10 });
            // solve the problem
            Vector<double> solution = A.Solve(b);

            Assert.AreEqual(2, solution[1]);
        }

        [TestMethod]
        public void Test_Thermodynamic_BalanceHydrocarbonEquation_Method()
        {
            // CH4 + O2 =  CO2 + H2O
            // CH4 + 2O2 = CO2 + 2H2O
            Matrix<double> matrix_CH4 = Matrix<double>.Build.DenseOfArray(new[,]{
                {1.0, 0.0,  -1.0,  0.0 }, // C balance
                {4.0, 0.0,   0.0, -2.0 }, // H balance
                {0.0, 2.0,  -2.0, -1.0 }, // O balance
                {1.0, 0.0,   0.0,  0.0 }   // Setting CH4
            });

            Vector<double> expected_CH4 = Vector<double>.Build.Dense(new double[] { 1, 2, 1, 2 });
            var solution = ThermoDynamics.BalanceHydrocarbonEquation(matrix_CH4);
            Assert.AreEqual(expected_CH4, solution);



        }
        [TestMethod]
        public void TestShouldPutNonZeroValuesInto_ValuesVector()
        {
            // CH4 + O2 =  CO2 + H2O
            // CH4 + 2O2 = CO2 + 2H2O
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new[,]{
                {1.0, 0.0,  -1.0,  0.0 }, // C balance
                {4.0, 0.0,   0.0, -2.0 }, // H balance
                {0.0, 2.0,  -2.0, -1.0 }, // O balance
                {1.0, 0.0,   0.0,  0.0 }   // Setting CH4
            });

            int numRows = matrix.RowCount;
            int nonZeroValues = 0;
            int ii = 0;

            IEnumerable<(int, Vector<double>)> m_enumeratedRows = matrix.EnumerateRowsIndexed();
            for (int i = 0; i < numRows; i++)
            {
                var m_elementAtRow = m_enumeratedRows.ElementAt(i);
                int m_rowNumber = m_elementAtRow.Item1;
                Vector<double> m_rowVector = m_elementAtRow.Item2;

                foreach (var item in m_rowVector)
                {
                    int m_compare = item.CompareTo(0.0);
                    if (m_compare != 0)
                    {
                        nonZeroValues++;
                    }
                }

            }

            double[] values = new double[nonZeroValues];
            //Vector<double> values = Vector<double>.Build.Dense(nonZeroValues);
            for (int i = 0; i < numRows; i++)
            {
                var m_elementAtRow = m_enumeratedRows.ElementAt(i);
                int m_rowNumber = m_elementAtRow.Item1;
                Vector<double> m_rowVector = m_elementAtRow.Item2;

                foreach (var item in m_rowVector)
                {
                    int m_compare = item.CompareTo(0.0);
                    if (m_compare != 0)
                    {
                        values[ii] = item;
                        ii++;
                        //nonZeroValues++;
                    }
                }

            }
            Assert.AreEqual(8, values.Length);

        }
        [TestMethod]
        public void TestGetNumberOfNonZeroValuesInMatrix()
        {
            // CH4 + O2 =  CO2 + H2O
            // CH4 + 2O2 = CO2 + 2H2O
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new[,]{
                {1.0, 0.0,  -1.0,  0.0 }, // C balance
                {4.0, 0.0,   0.0, -2.0 }, // H balance
                {0.0, 2.0,  -2.0, -1.0 }, // O balance
                {1.0, 0.0,   0.0,  0.0 }   // Setting CH4
            });

            int numRows = matrix.RowCount;
            int nonZeroValues = 0;

            IEnumerable<(int, Vector<double>)> m_enumeratedRows = matrix.EnumerateRowsIndexed();
            for (int i = 0; i < numRows; i++)
            {
                var m_elementAtRow = m_enumeratedRows.ElementAt(i);
                int m_rowNumber = m_elementAtRow.Item1;
                Vector<double> m_rowVector = m_elementAtRow.Item2;

                foreach (var item in m_rowVector)
                {
                    int m_compare = item.CompareTo(0.0);
                    if (m_compare != 0)
                    {
                        nonZeroValues++;
                    }
                }
            }

            Assert.AreEqual(8, nonZeroValues);

        }
        [TestMethod]
        public void Test_Should_ProperlySize_IA_JA_vectors()
        {
            // CH4 + O2 =  CO2 + H2O
            // CH4 + 2O2 = CO2 + 2H2O
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new[,]{
                {1.0, 0.0,  -1.0,  0.0 }, // C balance
                {4.0, 0.0,   0.0, -2.0 }, // H balance
                {0.0, 2.0,  -2.0, -1.0 }, // O balance
                {1.0, 0.0,   0.0,  0.0 }   // Setting CH4
            });
            Assert.IsNotNull(matrix);

            int numRows = matrix.RowCount;
            int numColumns = matrix.ColumnCount;
            Assert.AreEqual((int)numRows, matrix.RowCount);

            // Create IA vector = numrows + 1
            Vector<double> IA = Vector.Build.Dense(numRows + 1);
            Assert.AreEqual(IA.Count, matrix.RowCount + 1);

            // Create the JA vector = number of reactants + number of products
            // this is set manully here but will get count from input. TODO
            Vector<double> JA = Vector.Build.Dense(8);
            Assert.AreEqual(8, JA.Count);

            IEnumerable<(int, Vector<double>)> m_enumeratedRows = matrix.EnumerateRowsIndexed();
            IEnumerable<(int, Vector<double>)> m_enumeratedColumns = matrix.EnumerateColumnsIndexed();

            int totalNonZeroValues = 0;
            //
            Vector<double> expected_IA = Vector.Build.Dense(new double[] { 0, 2, 4, 7, 8 });
            Vector<double> expected_JA = Vector.Build.Dense(new double[] { 0, 2, 0, 3, 1, 2, 3, 0 });
            int jj = 0;

            for (int i = 0; i < numRows; i++)
            {
                var m_elementAtRow = m_enumeratedRows.ElementAt(i);
                int m_rowNumber = m_elementAtRow.Item1;
                Vector<double> m_rowVector = m_elementAtRow.Item2;
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
                totalNonZeroValues += Row_non_zero_values;

            }
            Assert.AreEqual(8, totalNonZeroValues);
            Assert.IsNotNull(m_enumeratedRows);
            Assert.AreEqual(expected_IA, IA);
            Assert.AreEqual(expected_JA, JA);

        }

        [TestMethod]
        public void Test_Should_Balance_Equation()
        {
            //    { 1.0, 0.0,  -1.0,  0.0 }, // C balance
            //    { 4.0, 0.0,   0.0, -2.0 }, // H balance
            //    { 0.0, 2.0,  -2.0, -1.0 }, // O balance
            //    { 1.0, 0.0,   0.0,  0.0 }   // Setting CH4
            int rows = 12;
            int columns = 12;
            var spm = new SparseMatrix(rows, columns);
            Assert.IsNotNull(spm);

            // CH4          O2                              CO2                 H2O
            // column 0     column 1        column 2        column 3            column 4
            // empty row
            spm[1, 0] = 1.0; spm[1, 3] = -1.0;                       // Carbon
            spm[2, 0] = 4.0; spm[2, 4] = -2.0;   // Hydrogen
            spm[3, 1] = 2.0; spm[3, 3] = -2.0; spm[3, 4] = -1.0;   // Oxygen
            //*/// empty row
            spm[5, 0] = 1.0;
            double[] values = new double[] { 1.0, -1.0, 4.0, -2.0, 2.0, -2.0, -1.0, 1.0 };

            int[] IA = new int[] { 0, 2, 4, 7, 8 };
            int[] IJ = new int[] { 0, 2, 0, 3, 1, 2, 3, 0 };
            Matrix<double> A = Matrix.Build.SparseFromCompressedSparseRowFormat(4, 4, values.Length, IA, IJ, values);
            // define the right hand side
            Vector<double> b = Vector.Build.Dense(new double[] { 0, 0, 0, 1.0 });
            // expected
            Vector<double> expected = Vector<double>.Build.Dense(new double[] { 1, 2, 1, 2 });

            // solution
            Vector<double> solution = A.Solve(b);

            Assert.AreEqual(expected, solution);

        }
        [TestMethod]
        public void TestMathNetMatrix()
        {
            // CH4 + O2 =  CO2 + H2O
            // CH4 + 2O2 = CO2 + 2H2O
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new[,]{
                {1.0, 0.0,  -1.0,  0.0 }, // C balance
                {4.0, 0.0,   0.0, -2.0 }, // H balance
                {0.0, 2.0,  -2.0, -1.0 }, // O balance
                {1.0, 0.0,   0.0,  0.0 }   // Setting CH4
            });

            var spm = SparseMatrix.Create(6, 6, 0.0);

            // empty row
            spm[1, 0] = 1.0; /*spm[1, 1] = 0.0;*/              spm[1, 3] = -1.0; /*spm[1, 4] =  0.0;*/
            spm[2, 0] = 4.0; /*spm[2, 1] = 0.0;*/              /*spm[2, 3] =  0.0;*/ spm[2, 4] = -2.0;
            /*spm[3, 0] = 0.0;*/
            spm[3, 1] = 2.0; spm[3, 3] = -2.0; spm[3, 4] = -1.0;
            //*/// empty row
            spm[5, 0] = 1.0; /*spm[5, 1] = 0.0;                  spm[5, 3] = 0.0;  spm[5, 4] = 0.0;*/ // set compound counts
                                                                                                      // empty column

            int non_zero = spm.NonZerosCount;
            Vector<double> rowAbsSums = spm.RowAbsoluteSums();
            Vector<double> rowSums = spm.RowSums();
            Vector<double> columnAbsSums = spm.ColumnAbsoluteSums();
            Vector<double> columnSums = spm.ColumnSums();
            double[] columnMajor = spm.ToColumnMajorArray();
            double[] rowMajor = spm.ToRowMajorArray();
            Matrix<double> lowerTri = spm.LowerTriangle();
            Matrix<double> lowerTriStrict = spm.StrictlyLowerTriangle();
            Matrix<double> upperTri = spm.UpperTriangle();
            Matrix<double> upperTriStrict = spm.StrictlyUpperTriangle();
            var transMul = lowerTriStrict.TransposeAndMultiply(lowerTri);
            // set values of matrix
            //matrix[0, 0] = 1; matrix[0, 1] = 0; matrix[0, 2] = -1; matrix[0, 3] = 0;
            //matrix[1, 0] = 4; matrix[1, 1] = 0; matrix[1, 2] = 0;  matrix[1, 3] = -2;
            //matrix[2, 0] = 0; matrix[2, 1] = 2; matrix[2, 2] = -2; matrix[2, 3] = -1;
            // count matrix columns should equal 4
            int columnCount = matrix.ColumnCount;

            // create right hand side vector
            Vector<double> rightHandside = Vector<double>.Build.Dense(new[]
            {0.0, 0.0, 0.0, 1.0 });

            // number of columns
            var spmSparceVector = SparseVector.Create(6, 0.0);
            spmSparceVector[5] = 1.0;
            // solve the system using Gaussian elimination
            Vector<double> solution = matrix.Solve(rightHandside); // 1,2,1,2
            Vector<double> m_result = spm.Solve(spmSparceVector);

            Assert.AreEqual(4, columnCount);
            Assert.AreEqual(4, solution.Count);
            Assert.AreEqual(1, solution[0]);
            Assert.AreEqual(2, solution[1]);
            Assert.AreEqual(1, solution[2]);
            Assert.AreEqual(2, solution[3]);

        }
        [TestMethod]
        public void TestBalancedEquationSolverWithReactantInput()
        {
            // Arrange
            ICollection<Reactant> reactants = InputServices.GetSpecies("Data/newShortThermo.json");
            // reactants
            string fuelName = "CH4";
            string oxidizerName = "O2";
            List<Reactant>? Fuel = reactants?.Where(item => item.Name == fuelName).ToList();
            List<Reactant>? Oxidizer = reactants?.Where(item => item.Name == oxidizerName).ToList();
            // products
            string CO2Name = "CO2";
            string H2OName = "H2O";
            List<Reactant>? CO2 = reactants?.Where(item => item.Name == CO2Name).ToList();
            List<Reactant>? H2O = reactants?.Where(item => item.Name == H2OName).ToList();

            Dictionary<string, double>? fuelElementsList = (from molecule in Fuel
                                                            select molecule.Molecule.ChemicalFormula).FirstOrDefault();

            Dictionary<string, double>? oxidizerElementsList = (from molecule in Oxidizer
                                                                select molecule.Molecule.ChemicalFormula).FirstOrDefault();

            Dictionary<string, double>? co2ElementsList = (from molecule in CO2
                                                           select molecule.Molecule.ChemicalFormula).FirstOrDefault();

            Dictionary<string, double>? h2oElementsList = (from molecule in H2O
                                                           select molecule.Molecule.ChemicalFormula).FirstOrDefault();

            // Reactants
            //Fuel
            fuelElementsList.TryGetValue("C", out double reactant_fuel_carbon_value);
            fuelElementsList.TryGetValue("H", out double reactant_fuel_hydrogen_value);
            fuelElementsList.TryGetValue("O", out double reactant_fuel_oxygen_value);
            // Oxidizer
            oxidizerElementsList.TryGetValue("C", out double reactant_oxidizer_carbon_value);
            oxidizerElementsList.TryGetValue("H", out double reactant_oxidizer_hydrogen_value);
            oxidizerElementsList.TryGetValue("O", out double reactant_oxidizer_oxygen_value);

            // Products
            //CO2
            co2ElementsList.TryGetValue("C", out double product_CO2_carbon_value);
            co2ElementsList.TryGetValue("H", out double product_CO2_hydrogen_value);
            co2ElementsList.TryGetValue("O", out double product_CO2_oxygen_value);
            // H2O
            h2oElementsList.TryGetValue("C", out double product_H2O_carbon_value);
            h2oElementsList.TryGetValue("H", out double product_H2O_hydrogen_value);
            h2oElementsList.TryGetValue("O", out double product_H2O_oxygen_value);

            double[,] matrixValues =
            {
                {reactant_fuel_carbon_value, reactant_oxidizer_carbon_value, - product_CO2_carbon_value, - product_H2O_carbon_value},
                {reactant_fuel_hydrogen_value, reactant_oxidizer_hydrogen_value ,  - product_CO2_hydrogen_value, - product_H2O_hydrogen_value},
                {reactant_fuel_oxygen_value,reactant_oxidizer_oxygen_value, - product_CO2_oxygen_value,  - product_H2O_oxygen_value },
                {1.0, 0.0, 0.0, 0.0 }
            };
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(matrixValues);
            // create right hand side vector
            Vector<double> rightHandside = Vector<double>.Build.Dense(new[]
            {0.0, 0.0, 0.0, 1.0 });
            // solve the system using Gaussian elimination
            Vector<double> solution = matrix.Solve(rightHandside);

            Matrix<double> defaultMatrix = Matrix<double>.Build.Dense(4, 4, 0.0);
            defaultMatrix[0, 0] = 1.0; defaultMatrix[0, 1] = 0.0; defaultMatrix[0, 2] = -1.0; defaultMatrix[0, 3] = 0.0;
            defaultMatrix[1, 0] = 4.0; defaultMatrix[1, 1] = 0.0; defaultMatrix[1, 2] = 0.0; defaultMatrix[1, 3] = -2.0;
            defaultMatrix[2, 0] = 0.0; defaultMatrix[2, 1] = 2.0; defaultMatrix[2, 2] = -2.0; defaultMatrix[2, 3] = -1.0;
            defaultMatrix[3, 0] = 1.0; defaultMatrix[3, 1] = 0.0; defaultMatrix[3, 2] = 0.0; defaultMatrix[3, 3] = 0.0;

            Vector<double> rightside = Vector<double>.Build.Dense(new[]
            {0.0, 0.0, 0.0, 1.0 });
            // solve the system using Gaussian elimination
            Vector<double> m_solution = matrix.Solve(rightside);

            List<Reactant> balancedProductH2O = H2O;
            balancedProductH2O.ForEach(r => r.Molecule.Count = m_solution[3]);

            Molecule balancedFuel = new()
            {
                ChemicalFormula = new()
                {
                    {"C", 1.0 },
                    {"H", 4.0 }
                },
                Count = m_solution[0]
            };
            Molecule balancedO2 = new()
            {
                ChemicalFormula = new()
                {
                    { "O", 2.0 },
                },
                Count = m_solution[1]
            };
            Molecule balancedCO2 = new()
            {
                ChemicalFormula = new()
                {
                    {"C", 1.0 },
                    {"O", 2.0 }
                },
                Count = m_solution[2]
            };
            Molecule balancedH2O = new()
            {
                ChemicalFormula = new()
                {
                    { "H", 2.0 },
                    { "O", 1.0 }
                },
                Count = m_solution[3]
            };

            Assert.AreEqual(99, 0);

        }
    }

    [TestClass]
    public class TestRegex
    {
        [TestMethod]
        public void TestCoPilotFormulaParser()
        {
            string equation = "2H2SO4";
            Dictionary<string, int> ParseChemicalEquation = new();
            // this Dictionary will hold the element symbols and their coefficients
            Dictionary<string, int> elements = new();
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

            Assert.IsNotNull(moleculePattern);

        }
        [TestMethod]
        public void TestParseChemicalEquationMethod()
        {
            string equation = "3C12H22O11";
            Dictionary<string, double> parsedEquationDict = ThermoDynamics.ParseChemicalEquation(equation);
            Assert.IsNotNull(parsedEquationDict);
        }
    }

    [TestClass]
    public class Test_NASA_Polynomials
    {
        [TestMethod]
        public void Test_NASAnotNull()
        {
            ICollection<Specie> species = InputServices.GetNASA("Data/NASApolynomials.json");
            Assert.IsNotNull(species);
        }

        [TestMethod]
        public void TestShouldReturnNamedSpecie()
        {
            string name = "CH4";
            ICollection<Specie> species = InputServices.GetNASA("Data/NASApolynomials.json");
            var m_specie = from item in species
                           where item.Name == name
                           select item;
            var expected = m_specie;

            Assert.IsNotNull(m_specie);

        }

        [TestMethod]
        public void TestShouldReturnPhasesGreaterThanZero()
        {
            ICollection<Specie> species = InputServices.GetNASA("Data/NASApolynomials.json");
            var m_specie = from item in species
                           where item.PhaseValue > 0
                           select item;
            Assert.AreNotEqual(0, m_specie.Count());

        }

    }

    [TestClass]
    public class MatrixSolvers
    {
        [TestMethod]
        public void TestShouldBuildMatrixAndLoadChemicalFormula()
        {
            // Arrange
            Matrix<double> coefficientMatrix = Matrix<double>.Build.Dense(11, 11, 0.0);
            double determinate = coefficientMatrix.Determinant();
            bool isSymetric = coefficientMatrix.IsSymmetric();
            Assert.IsNotNull(coefficientMatrix);
            //

            Matrix<double> variableMatrix = Matrix<double>.Build.DenseIdentity(11, 11);
            Assert.IsNotNull(variableMatrix);

            // constants matrix
            Matrix<double> constantMatrix = new DenseMatrix(11, 1);
            constantMatrix[0, 0] = 0;
            constantMatrix[1, 0] = 0;
            constantMatrix[2, 0] = 0;
            constantMatrix[3, 0] = 0;
            constantMatrix[4, 0] = 0;
            constantMatrix[5, 0] = 0;
            constantMatrix[6, 0] = 0;
            constantMatrix[7, 0] = 0;
            constantMatrix[8, 0] = 0;
            constantMatrix[9, 0] = 0;
            constantMatrix[10, 0] = 1.0;
            Assert.IsNotNull(constantMatrix);

            // create rightHand side vector
            int m_VectorSize = 11;
            //Vector<double> rightHandSide = Vector<double>.Build.Dense(m_VectorSize, 0.0);
            Vector<double> rightHandside = Vector<double>.Build.Dense(new[]
            {0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0 });
            int m_rightHandSideCount = rightHandside.Count;
            rightHandside[m_rightHandSideCount - 1] = 1.0;
            Assert.IsNotNull(rightHandside);
            Assert.AreEqual(m_VectorSize, m_rightHandSideCount);

            //
            // Set the first column (column 0) to the values for the first reactant(Species)
            // Set the number of CH4 molecules to 1 in the last row of the defaultMatrix
            coefficientMatrix[10, 0] = 1.0;
            coefficientMatrix[10, 1] = 1.0;
            coefficientMatrix[10, 2] = 1.0;
            coefficientMatrix[10, 3] = 1.0;
            coefficientMatrix[10, 4] = 1.0;
            coefficientMatrix[10, 5] = 1.0;
            coefficientMatrix[10, 6] = 1.0;
            coefficientMatrix[10, 7] = 1.0;
            coefficientMatrix[10, 8] = 1.0;
            coefficientMatrix[10, 9] = 1.0;
            coefficientMatrix[10, 10] = 1.0;

            // Get data for Species, TableOfElements, And Reference Values
            // Table of elements
            ICollection<Element> tableOfElements = ElementsService.GetElements("Data/tableOfElements.json");
            Assert.IsNotNull(tableOfElements);
            // Get the reference data
            ICollection<CPHSRef> cphs_reference = InputServices.GetDefaultCPHS("Data/Ref_Defaults.json");
            Assert.IsNotNull(cphs_reference);
            // Species data 
            // TODO Update to NASApolynomials
            ICollection<Reactant> AllSpecies = InputServices.GetSpecies("Data/newShortThermo.json");
            Assert.IsNotNull(AllSpecies);

            // Reactants and products section Section
            string m_firstReactantMolecule = "CH4";
            string m_secondReactantMolecule = "O2";

            // get the key collection of elements in reactants
            var inputKeyCollection = from item in AllSpecies
                                     where item.Name == m_firstReactantMolecule | item.Name == m_secondReactantMolecule
                                     select item.Molecule.ChemicalFormula?.Keys;

            Dictionary<string, Element?> elementTableOfElements = new();
            for (int i = 0; i < inputKeyCollection.Count(); i++)
            {
                var elementAt = inputKeyCollection.ElementAt(i);
                foreach (var item in elementAt)
                {
                    IEnumerable<Element> elementData = tableOfElements.Where(x => x.Symbol == item);
                    string key = item;
                    if (elementData.Any())
                    {
                        elementTableOfElements.Add(key: key, value: elementData.FirstOrDefault());
                    };
                }
            }

            //IEnumerable<KeyValuePair<string, Element>> elementProperties = from item in elementTableOfElements
            //                   where item.Key == "C"
            //                   select item;
            //IEnumerable<CPHSRef> element_cphs_reference_defaults = from item in cphs_reference
            //          where item.Species_Name == "C"
            //          select item;

            for (int i = 0; i < elementTableOfElements.Count; i++)
            {
                string key = elementTableOfElements.ElementAt(i).Key;
                IEnumerable<KeyValuePair<string, Element>> elementProperties = from item in elementTableOfElements
                                                                               where item.Key == key
                                                                               select item;

                IEnumerable<CPHSRef> element_cphs_reference_defaults = from item in cphs_reference
                                                                       where item.Species_Name == key
                                                                       select item;
            }
            string mx_key = "C";
            var keyValuePairsElements = elementTableOfElements.FirstOrDefault(x => x.Key == mx_key);

            IEnumerable<Molecule> m_firstReactant = from item in AllSpecies
                                                    where item.Name == m_firstReactantMolecule
                                                    select item.Molecule;
            Assert.IsNotNull(m_firstReactant);


            Dictionary<string, double>.KeyCollection keyCollection = m_firstReactant.FirstOrDefault().ChemicalFormula.Keys;
            Dictionary<string, double>.ValueCollection valuesCollection = m_firstReactant.FirstOrDefault().ChemicalFormula?.Values;
            int m_keyCount = keyCollection.Count;
            IEnumerable<Molecule> m_secondReactant = from item in AllSpecies
                                                     where item.Name == m_secondReactantMolecule
                                                     select item.Molecule;
            Assert.IsNotNull(m_secondReactant);
            Collection<IEnumerable<Molecule>> reactantsCollection = new();
            reactantsCollection.Add(m_firstReactant);
            reactantsCollection.Add(m_secondReactant);

            // Products Section
            // first product = "CO2
            // second product = H2O
            string m_firstProductMolecule = "CO2";
            string m_secondProductMolecule = "H2O";

            var m_firstproduct = from item in AllSpecies
                                 where item.Name == m_firstProductMolecule
                                 select item.Molecule;
            Assert.IsNotNull(m_firstproduct);
            var m_secondProduct = from item in AllSpecies
                                  where item.Name == m_secondProductMolecule
                                  select item.Molecule;
            Assert.IsNotNull(m_secondProduct);
            Collection<IEnumerable<Molecule>> productsCollection = new();
            productsCollection.Add(m_firstproduct);
            productsCollection.Add(m_secondProduct);

            int matrixColumnCount = 0;
            int m_reactantCount = reactantsCollection.Count;
            foreach (var item in reactantsCollection)
            {
                Dictionary<string, double>.KeyCollection c_keys = item.FirstOrDefault().ChemicalFormula.Keys;
                Dictionary<string, double>.ValueCollection c_values = item.FirstOrDefault().ChemicalFormula.Values;
                int c_keycount = c_keys.Count;

                LoadBalancedEquationMatrix(c_keys, c_values, c_keycount, matrixColumnCount);
                matrixColumnCount = matrixColumnCount + 1;
            }

            // since these are the products the values have to be set to negative
            foreach (var item in productsCollection)
            {
                //Dictionary<string, double>.KeyCollection c_keys = item.FirstOrDefault().ChemicalFormula.Keys;
                Dictionary<string, double>? keyValuePairs = item.FirstOrDefault().ChemicalFormula;
                //Dictionary<string, double>.ValueCollection c_values = item.FirstOrDefault().ChemicalFormula.Values;
                foreach (var kvp in keyValuePairs)
                {
                    keyValuePairs[kvp.Key] = -kvp.Value;
                }
                //int c_keycount = c_keys.Count;

                LoadBalancedEquationMatrix(keyValuePairs.Keys, keyValuePairs.Values, keyValuePairs.Count, matrixColumnCount);
                matrixColumnCount = matrixColumnCount + 1;
            }

            // TODO j is temp variable to reference column in defaultMatrix

            //(Matrix<double> solutionMatrix, Matrix<double> pivotMatrix) = coefficientMatrix.Solve(input: variableMatrix, constantMatrix);

            void LoadBalancedEquationMatrix(Dictionary<string, double>.KeyCollection Keys, Dictionary<string, double>.ValueCollection Values, int KeysCount, int ColumnNumber)
            {
                int m_column = ColumnNumber;

                for (int i = 0; i < KeysCount; i++)
                {
                    string elementKey = Keys.ElementAt(i);
                    switch (elementKey)
                    {
                        case "H":
                            // matrix row 0
                            int row0 = 0;
                            coefficientMatrix[row0, m_column] = Values.ElementAt(i);
                            break;
                        case "D":
                            // matrix row 1
                            int row1 = 1;
                            coefficientMatrix[row1, m_column] = Values.ElementAt(i);
                            break;
                        case "He":
                            // matrix row 2
                            int row2 = 2;
                            coefficientMatrix[row2, m_column] = Values.ElementAt(i);
                            break;
                        case "Li":
                            // matrix row 3
                            int row3 = 3;
                            coefficientMatrix[row3, m_column] = Values.ElementAt(i);
                            break;
                        case "Be":
                            // matrix row 4
                            int row4 = 4;
                            coefficientMatrix[row4, m_column] = Values.ElementAt(i);
                            break;
                        case "B":
                            // matrix row 5
                            int row5 = 5;
                            coefficientMatrix[row5, m_column] = Values.ElementAt(i);
                            break;
                        case "C":
                            // matrix row 6
                            int row6 = 6;
                            coefficientMatrix[row6, m_column] = Values.ElementAt(i);
                            break;
                        case "N":
                            // matrix row 7
                            int row7 = 7;
                            coefficientMatrix[row7, m_column] = Values.ElementAt(i);
                            break;
                        case "O":
                            // matrix row 8 
                            int row8 = 8;
                            coefficientMatrix[row8, m_column] = Values.ElementAt(i);
                            break;
                        case "F":
                            // matrix row 9
                            int row9 = 9;
                            coefficientMatrix[row9, m_column] = Values.ElementAt(i);
                            break;

                        default:
                            break;
                    }
                }
            }

            // solve the system using Gaussian elimination
            Vector<double> m_solution = coefficientMatrix.Solve(rightHandside);

            Assert.AreEqual(99, 0);
        }
    }

    [TestClass]
    public class TestServices
    {

        [TestMethod]
        public void Test_ElementsService()
        {
            ICollection<Element> json = InputServices.GetTableOfElements("Data/TableOfElements.json");
            int elementCount = json.Count;

            Assert.AreNotEqual(0, elementCount);
        }

        [TestMethod]
        public void TestInputServicesWithPath()
        {
            ICollection<Reactant> json = InputServices.GetSpecies("Data/thermoInp.json");
            int reactantCount = json.Count;

            Assert.AreEqual(2084, reactantCount);
        }

        [TestMethod]
        public void Test_Ref_DefaultServiceWithPath()
        {
            ICollection<CPHSRef> ref_defaults = InputServices.GetDefaultCPHS("Data/Ref_Defaults.json");
            int ref_defaultsCount = ref_defaults.Count;

            Assert.AreEqual(1258, ref_defaultsCount);
        }

        [TestMethod]
        public void Test_ShouldReturnReferenceElements()
        {
            ICollection<ReferenceElement> referenceElements = InputServices.GetReferenceElements("Data/refElements.json");
            int referenceElementsCount = referenceElements.Count;
            Assert.AreEqual(referenceElementsCount, referenceElements.Count);
        }

    }

    [TestClass]
    public class TestReferenceElements
    {
        [TestMethod]
        public void Test_ReferenceElementHeatCapacity()
        {
            // Update with the refElements.json file
            // Testing for Oxygen = O
            //               "temperatureRange": [ 200.000, 1000.000 ],
            //"numberOfCoefficients": 7,
            //"tExponents": [ -2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0 ],
            //"hJmol": 6725.403,
            //"coefficients": [ -7.953611300e+03, 1.607177787e+02, 1.966226438e+00, 1.013670310e-03, -1.110415423e-06, 6.517507500e-10, -1.584779251e-13 ],
            //"integrationConstants": [ 2.840362437e+04, 8.404241820e+00 ]
            List<double> temperatureRange = [200.0, 1000.0];
            List<double> t_exp = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> coeff = [-7.953611300e+03, 1.607177787e+02, 1.966226438e+00, 1.013670310e-03, -1.110415423e-06, 6.517507500e-10, -1.584779251e-13];
            List<double> integrationConstants = [2.840362437e+04, 8.404241820e+00];

            double delta = 0.005;
            double T = 398.15;
            double Cp = ThermoDynamics.HeatCapacity(T, t_exp, coeff);

            Assert.AreEqual(21.488, Cp, delta);
        }

        [DataTestMethod]
        [DataRow(298.15, 249.175)]
        [DataRow(398.15, 251.343)]
        [DataRow(498.15, 253.479)]
        [DataRow(598.15, 255.598)]
        [DataRow(698.15, 257.706)]
        [DataRow(798.15, 259.807)]
        [DataRow(898.15, 261.903)]
        [DataRow(998.15, 263.996)]
        [DataRow(1000.00, 264.035)]
        public void TestShouldReturnEnthaplyForElementOxygen(double Temperature, double expected)
        {
            string searchString = "O";
            ICollection<Specie> m_species = InputServices.GetNASA("Data/NASApolynomials.json");

            IEnumerable<Specie> m_reactant = from specie in m_species
                                             where specie.Name == searchString
                                             select specie;

            ICollection<ChemicalFormula> chemicalFormula = m_reactant.First().ChemicalFormula;
            List<double> temperatureRange = m_reactant.First().DataRecords.ElementAt(0).TemperatureRange;
            List<double> coefficients = m_reactant.First().DataRecords.ElementAt(0).Coefficients;
            List<double> t_expnts = m_reactant.First().DataRecords.ElementAt(0).TExponents;
            List<double> integrationConstants = m_reactant.First().DataRecords.ElementAt(0).IntegrationConstants;
            double delta = 0.005;
            double refTemp = 298.15;
            double heatOfFormation = m_reactant.First().HeatOfFormation;
            double Enthalpy_kJmol = ThermoDynamics.Enthalpy(Temperature, t_expnts, coefficients, integrationConstants);

            Assert.AreEqual(expected, Enthalpy_kJmol, delta);
        }

        [DataTestMethod]
        [DataRow(298.15, 0.0)]
        [DataRow(398.15, 2.168)]
        [DataRow(498.15, 4.304)]
        [DataRow(598.15, 6.423)]
        [DataRow(698.15, 8.531)]
        [DataRow(798.15, 10.632)]
        [DataRow(898.15, 12.728)]
        [DataRow(998.15, 14.821)]
        [DataRow(1000.00, 14.860)]
        public void Test_ReferenceElementEnthalpy(double T, double expected)
        {
            // TODO update with refElements.json
            // O coefficients
            List<double> temperatureRange = [200.0, 1000.0];
            List<double> t_exp = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> coeff = [-7.953611300e+03, 1.607177787e+02, 1.966226438e+00, 1.013670310e-03, -1.110415423e-06, 6.517507500e-10, -1.584779251e-13];
            List<double> integrationConstants = [2.840362437e+04, 8.404241820e+00];

            double delta = 0.005;
            double ref_temp = 298.15;
            //double T = 398.15;

            double enthalpy = ThermoDynamics.EnthalpyRefH298(ref_temp, T, coeff, t_exp);
            Assert.AreEqual(expected, enthalpy, delta);

        }

        [DataTestMethod]
        [DataRow(298.15, 0.0)]
        [DataRow(398.15, 2.168)]
        [DataRow(498.15, 4.304)]
        [DataRow(598.15, 6.423)]
        [DataRow(698.15, 8.531)]
        [DataRow(798.15, 10.632)]
        [DataRow(898.15, 12.728)]
        [DataRow(998.15, 14.821)]
        [DataRow(1000.00, 14.860)]
        public void Test_ReferenceElementH298(double T, double expected)
        {
            // O coefficients
            List<double> temperatureRange = [200.0, 1000.0];
            List<double> t_exp = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> coeff = [-7.953611300e+03, 1.607177787e+02, 1.966226438e+00, 1.013670310e-03, -1.110415423e-06, 6.517507500e-10, -1.584779251e-13];
            List<double> integrationConstants = [2.840362437e+04, 8.404241820e+00];

            double delta = 0.005;
            double ref_temp = 298.15;

            double enthalpy = ThermoDynamics.EnthalpyRefH298(ref_temp, T, coeff, t_exp);

            Assert.AreEqual(expected, enthalpy, delta);
        }
    }

    [TestClass]
    public class TestThermodynamicMethods
    {
        private static readonly string NASAsearchString = "CH4";
        private static readonly double referenceTemp = 298.15;
        private static readonly double delta = 0.005;

        private static readonly ICollection<Specie> nasaPolynomials = InputServices.GetNASA("Data/NASApolynomials.json");
        private static readonly IEnumerable<Specie> NASA_specie = from NASAspecie in nasaPolynomials
                                                                  where NASAspecie.Name == NASAsearchString
                                                                  select NASAspecie;
        private static ICollection<ChemicalFormula> NASAchemicalFormula = NASA_specie.First().ChemicalFormula;
        private static readonly List<double> NASAtemperatureRange = NASA_specie.First().DataRecords.ElementAt(0).TemperatureRange;
        private static readonly List<double> NASACoefficients = NASA_specie.First().DataRecords.ElementAt(0).Coefficients;
        private static readonly List<double> NASAIntegrationConstants = NASA_specie.First().DataRecords.ElementAt(0).IntegrationConstants;
        private static readonly List<double> NASAExponents = NASA_specie.First().DataRecords.ElementAt(0).TExponents;

        private static readonly string refSearchString = "O2";
        private static readonly ICollection<Specie> refElementPolynomials = InputServices.GetNASA("Data/refElements.json");
        private static readonly IEnumerable<Specie> O2_ref_specie = from refSpecie in refElementPolynomials
                                                                    where refSpecie.Name == refSearchString
                                                                    select refSpecie;

        private static ICollection<ChemicalFormula> refChemicalFormula = O2_ref_specie.First().ChemicalFormula;
        private static readonly List<double> refTemperatureRange = O2_ref_specie.First().DataRecords.ElementAt(0).TemperatureRange;
        private static readonly List<double> refCoefficients = O2_ref_specie.First().DataRecords.ElementAt(0).Coefficients;
        private static readonly List<double> refIntegrationConstants = O2_ref_specie.First().DataRecords.ElementAt(0).IntegrationConstants;
        private static readonly List<double> refExponents = O2_ref_specie.First().DataRecords.ElementAt(0).TExponents;

        private static readonly ICollection<CPHSRef> referenceCPHS = InputServices.GetDefaultCPHS("Data/Ref_Defaults.json");
        private static readonly IEnumerable<CPHSRef> m_referenceSpecie = (IEnumerable<CPHSRef>)(from m_specie in referenceCPHS
                                                                                                where m_specie.Species_Name == NASAsearchString
                                                                                                select m_specie);
        [TestMethod]
        public void TestDeltaHrxnReturnsHrxn()
        {
            double Temperature = 298.15;
            List<string> reactants = new();
            reactants.Add("Cl2");
            reactants.Add("H2");
            string products = "2HCl";
            double deltaHrxn = ThermoDynamics.DeltaHrxn(Temperature, reactants, products);

            double expected = -184.62;
            Assert.AreEqual(expected, deltaHrxn);
        }

        [DataTestMethod]
        [DataRow(298.15, -74.600)]
        [DataRow(398.15, -77.635)]
        [DataRow(498.15, -80.457)]
        [DataRow(598.15, -82.932)]
        [DataRow(698.15, -85.023)]
        [DataRow(798.15, -86.726)]
        [DataRow(898.15, -88.059)]
        [DataRow(998.15, -89.053)]
        [DataRow(1000.00, -89.069)]
        public void TestShouldReturnGibbsEnergyAsFunctionOfTemperature(double Temp, double expected)
        {
            ICollection<ReferenceElement> refElements = InputServices.GetReferenceElements("Data/refElements.json");
            ICollection<Specie> NASAspecies = InputServices.GetNASA("Data/NASApolynomials.json");
            Assert.IsNotNull(NASAspecies);

            //  Hydrogen
            IEnumerable<ReferenceElement> H2_elementData = from element in refElements
                                                           where element.Name == "H2"
                                                           select element;
            List<double> H2_tExpnts = H2_elementData.First().DataRecords.ElementAt(0).TExponents;
            List<double> H2_coefficients = H2_elementData.First().DataRecords.ElementAt(0).Coefficients;
            List<double> H2_integrationConstants = H2_elementData.First().DataRecords.ElementAt(0).IntegrationConstants;

            double H2_heatOfFormation = H2_elementData.First().HeatOfFormation;
            double H_heatOfFormation = H2_heatOfFormation - (double)(H2_elementData.First().DataRecords.ElementAt(0).EnthalpyRef / 1000);
            double H2_Cp = ThermoDynamics.HeatCapacity(Temp, H2_tExpnts, H2_coefficients);
            double H2_enthalpy = ThermoDynamics.Enthalpy(Temp, H2_tExpnts, H2_coefficients, H2_integrationConstants);
            double H2_entropy = ThermoDynamics.Entropy(Temp, H2_tExpnts, H2_coefficients, H2_integrationConstants);
            Assert.AreEqual(1, H2_elementData.Count());

            //  Carbon
            IEnumerable<ReferenceElement> Cg_elementData = from element in refElements
                                                           where element.Name == "C(gr)"
                                                           select element;
            List<double> Cg_expnts = Cg_elementData.First().DataRecords.ElementAt(0).TExponents;
            List<double> Cg_coeff = Cg_elementData.First().DataRecords.ElementAt(0).Coefficients;
            List<double> Cg_intConstants = Cg_elementData.First().DataRecords.ElementAt(0).IntegrationConstants;

            double Cg_heatofformation = Cg_elementData.First().HeatOfFormation;
            double Cg_Cp = ThermoDynamics.HeatCapacity(Temp, Cg_expnts, Cg_coeff);
            double Cg_enthalpy = ThermoDynamics.Enthalpy(Temp, Cg_expnts, Cg_coeff, Cg_intConstants);
            double Cg_entropy = ThermoDynamics.Entropy(Temp, Cg_expnts, Cg_coeff, Cg_intConstants);

            // Oxygen
            IEnumerable<ReferenceElement> O2_elementData = from element in refElements
                                                           where element.Name == "O2"
                                                           select element;
            List<double> O2_expnts = O2_elementData.First().DataRecords.ElementAt(0).TExponents;
            List<double> O2_coeff = O2_elementData.First().DataRecords.ElementAt(0).Coefficients;
            List<double> O2_intConstants = O2_elementData.First().DataRecords.ElementAt(0).IntegrationConstants;

            double O2_heatOfFormation = O2_elementData.First().HeatOfFormation;
            double O2_Cp = ThermoDynamics.HeatCapacity(Temp, O2_expnts, O2_coeff);
            double O2_enthalpy = ThermoDynamics.Enthalpy(Temp, O2_expnts, O2_coeff, O2_intConstants);
            double O2_entropy = ThermoDynamics.Entropy(Temp, O2_expnts, O2_coeff, O2_intConstants);

            var products = from compound in NASAspecies
                           where compound.Name == "CO" | compound.Name == "CO2" | compound.Name == "H2" | compound.Name == "H2O"
                           select compound;
            var CO2coeff = products.ElementAt(1).DataRecords.ElementAt(0).Coefficients;
            List<double> deltaGList = new();
            for (int i = 0; i < 4; i++)
            {
                double deltaG = CO2coeff.ElementAt(i) - O2_coeff.ElementAt(i) - Cg_coeff.ElementAt(i);
                deltaGList.Add(deltaG);
            }

            Dictionary<string, double> products_Cp = new();
            Dictionary<string, double> products_entropy = new();
            Dictionary<string, double> products_gibbs = new();
            foreach (var item in products)
            {
                double Cp = ThermoDynamics.HeatCapacity(Temp, item.DataRecords.ElementAt(0).TExponents, item.DataRecords.ElementAt(0).Coefficients);
                products_Cp.Add(item.Name, Cp);
                double Entropy = ThermoDynamics.Entropy(Temp, item.DataRecords.ElementAt(0).TExponents, item.DataRecords.ElementAt(0).Coefficients, item.DataRecords.ElementAt(0).IntegrationConstants);
                products_entropy.Add(item.Name, Entropy);
                double GibbsRef = ThermoDynamics.GibbsRef(Temp, Entropy, 298.15, item.DataRecords.ElementAt(0).Coefficients, item.DataRecords.ElementAt(0).TExponents);
                products_gibbs.Add(item.Name, GibbsRef);

            }

            double enthalpyCH4_product = ThermoDynamics.Enthalpy(Temp, NASAExponents, NASACoefficients, NASAIntegrationConstants);
            double deltaCH4Reaction = enthalpyCH4_product - (H2_enthalpy + H2_enthalpy + Cg_enthalpy);
            List<double> reactants = new();
            H2_enthalpy += H2_enthalpy;
            reactants.Add(H2_enthalpy);
            reactants.Add(Cg_enthalpy);
            // need refElements and NASApolynomials
            double m_deltaReaction = ThermoDynamics.DeltaHf(enthalpyCH4_product, reactants);

            Assert.AreEqual(expected, deltaCH4Reaction, delta);

        }
        [TestMethod]
        public void TestEnthalpyOfReaction()
        {
            // H2(g) + CL2(g) <--> 2HCL(g)
            // C(gr) + 2H2  <--> CH4


            var H2_reactant_1 = from item in referenceCPHS
                                where item.Species_Name == "H2"
                                select (item.Delta_Enthalpy_Ref, item.Entropy_Ref);

            var reactant_2 = from item in referenceCPHS
                             where item.Species_Name == "CL2"
                             select (item.Delta_Enthalpy_Ref, item.Entropy_Ref);

            var product_1 = from item in referenceCPHS
                            where item.Species_Name == "HCL"
                            select (item.Delta_Enthalpy_Ref, item.Entropy_Ref);

            var queryResult = from item in product_1
                              select (DeltaEnthalpyRef: item.Delta_Enthalpy_Ref, EntropyRef: item.Entropy_Ref);

            double productDeltaEnthalpyRef = 0.0;
            double productEntropyRef = 0.0;
            foreach ((double Delta_Enthalpy_Ref, double Entropy_Ref) in product_1)
            {
                productDeltaEnthalpyRef = Delta_Enthalpy_Ref;
                productEntropyRef = Entropy_Ref;
            }

            double reactant_1DeltaEnthalpyRef = 0.0;
            double reactant_1EntropyRef = 0.0;
            foreach (var item in H2_reactant_1)
            {
                reactant_1DeltaEnthalpyRef = item.Delta_Enthalpy_Ref;
                reactant_1EntropyRef = item.Entropy_Ref;
            }
            double reactant_2DeltaEnthalpyRef = 0.0;
            double reactant_2EntropyRef = 0.0;
            foreach (var item in reactant_2)
            {
                reactant_2DeltaEnthalpyRef = item.Delta_Enthalpy_Ref;
                reactant_2EntropyRef = item.Entropy_Ref;
            }

            // delta H_0_reaction = SUM n_p * deltaHf_0(p) - SUM n_r deltaHf_0(r)
            var delta_H_0reaction = 2 * productDeltaEnthalpyRef - 1 * (reactant_1DeltaEnthalpyRef + reactant_2DeltaEnthalpyRef);
            double expected_deltaH = -184.62;
            Assert.AreEqual(expected_deltaH, delta_H_0reaction, delta);
            // Entropy of reaction
            // delta_S_0reaction = SUM n_p * S_0(p) - SUM n(r) * S_0(r)


            double delta_S_0reaction = (2 * productEntropyRef) - (1 * (reactant_1EntropyRef + (1 * reactant_2EntropyRef)));

            //  delta_G = delta_H - T * delta_S
            double delta_G = delta_H_0reaction - 298.15 * delta_S_0reaction / 1000;
            double expected_deltaG = -190.59582045;
            Assert.AreEqual(expected_deltaG, delta_G, delta);
        }

        [TestMethod]
        public void Test_New_HeatCapacity()
        {
            double Temperature = 1000;
            double Cp = ThermoDynamics.HeatCapacity(Temperature, NASAExponents, NASACoefficients );
            Assert.AreEqual(73.676, Cp, delta);
        }

        [DataTestMethod]
        [DataRow(298.15, 0.0)]
        [DataRow(398.15, 3.794)]
        [DataRow(498.15, 8.139)]
        [DataRow(598.15, 13.093)]
        [DataRow(698.15, 18.647)]
        [DataRow(798.15, 24.768)]
        [DataRow(898.15, 31.416)]
        [DataRow(998.15, 38.548)]
        [DataRow(1000.00, 38.685)]
        public void Test_HeatCapacity_No_Library(double T, double expected)
        {

            static double GKIntegrate(Func<double, double> f, double a, double b, double targetRelativeError)
            {
                // Gauss nodes and weights (lower-order)
                double[] gaussNodes = { -0.7745966692, 0, 0.7745966692 };
                double[] gaussWeights = { 0.5555555556, 0.8888888889, 0.5555555556 };
                // Kronrod nodes and weights (higher-order)
                double[] kronrodNodes = { -0.8611363116, -0.3399810436, 0.3399810436, 0.8611363116 };
                double[] kronrodWeights = { 0.3478548451, 0.6521451549, 0.6521451549, 0.3478548451 };
                double integral = 0;
                // Combine Gauss and Kronrod contributions
                for (int i = 0; i < gaussNodes.Length; i++)
                {
                    double gaussPoint = 0.5 * (b - a) * gaussNodes[i] + 0.5 * (b + a);
                    double gaussWeight = 0.5 * (b - a) * gaussWeights[i];

                    double kronrodPoint = 0.5 * (b - a) * kronrodNodes[i] + 0.5 * (b + a);
                    double kronrodWeight = 0.5 * (b - a) * kronrodWeights[i];

                    double gaussValue = f(gaussPoint);
                    double kronrodValue = f(kronrodPoint);

                    integral += gaussWeight * gaussValue + kronrodWeight * kronrodValue;
                }
                // Estimate error
                double error = Math.Abs(integral - (kronrodWeights[0] * f(0.5 * (b - a) + 0.5 * (b + a))));
                // Check if error is within tolerance
                if (error < targetRelativeError * Math.Abs(integral))
                    return integral;
                else
                {
                    // Subdivide interval and recursively compute
                    double mid = 0.5 * (a + b);
                    double a_mid = GKIntegrate(f, a, mid, targetRelativeError);
                    double mid_b = GKIntegrate(f, mid, b, targetRelativeError);
                    return a_mid + mid_b;
                }
            }

            double Temp = 298.15;
            Func<double, double> heatCapacity = x => Math.Log(Temp);
            // integration interval [a, b]
            double a = 0;
            double b = 0.1;
            // set the desired reletive error tolerance
            double targetRelativeError = 0.1;
            // compute the integral using Gauss-Kronrod quadrature
            double result = GKIntegrate(heatCapacity, a, b, targetRelativeError);

            Assert.AreEqual(99, 0);

        }

        [DataTestMethod]
        [DataRow(298.15, 0.0)]
        [DataRow(398.15, 3.794)]
        [DataRow(498.15, 8.139)]
        [DataRow(598.15, 13.093)]
        [DataRow(698.15, 18.647)]
        [DataRow(798.15, 24.768)]
        [DataRow(898.15, 31.416)]
        [DataRow(998.15, 38.548)]
        [DataRow(1000.00, 38.685)]
        public void Test_EnthalpyRefH298(double T, double expected)
        {
            double enthalpy = ThermoDynamics.EnthalpyRefH298(referenceTemp, T, NASACoefficients, NASAExponents);
            Assert.AreEqual(expected, enthalpy, delta);
        }

        [DataTestMethod]
        [DataRow(298.15, 0.0)]
        //[DataRow(398.15, 0.0)]
        //[DataRow(498.15, 0.0)]
        //[DataRow(598.15, 0.0)]
        //[DataRow(698.15, 0.0)]
        //[DataRow(798.15, 0.0)]
        //[DataRow(898.15, 0.0)]
        //[DataRow(998.15, 0.0)]
        //[DataRow(1000.0, 0.0)]
        public void Test_MU(double Temperature, double expected)
        {
            // TODO update with refElements.json
            double NG = 1;
            double Pp = 1.0;
            double Enn = 0.1;
            double Enln = Math.Log(Enn / NG);
            double Tm = Math.Log(Pp / Enn);

            //List<string> m_formula = ["C"];
            List<string> m_formula = ["C", "H"];
            //List<string> m_formula = ["O"];
            //List<string> m_formula = ["CH4"];
            // CH4 coefficients
            //string searchString = "C";
            //string searchString = "O2";
            //string searchString = "CO2";
            //string searchString = "O";
            //string searchString = "CH4";
            //string searchString = "H2";
            string searchString = "";
            //ICollection<ChemicalFormula> chemicalFormula = null;
            List<double> temperatureRange = [];
            List<double> coefficients = [];
            List<double> integrationConstants = [];
            List<double> t_expnts = [];
            double Delta_Enthalpy_Ref = 0.0;
            double refTemperature = 298.15;
            double Cp_Ref = 0.0;
            double Enthalpy_kJmol = 0.0;
            double ref_entropy = 0.0;
            double Entropy_Ref = 0.0;
            double Gibbs_H298JmolK = 0.0;
            double MU = 0.0;

            //var referenceCPHS = InputServices.GetDefaultCPHS("Data/Ref_Defaults.json");
            //ICollection<Species> m_species = InputServices.GetNASA("Data/NASApolynomials.json");

            // refCPHS
            // species
            // refElements


            foreach (string formula in m_formula)
            {
                var m_refElement = from element in refElementPolynomials
                                   where element.ChemicalFormula.First().Symbol == formula
                                   select element;


                NASAchemicalFormula = m_refElement.First().ChemicalFormula;
                temperatureRange = m_refElement.First().DataRecords.ElementAt(0).TemperatureRange;
                coefficients = m_refElement.First().DataRecords.ElementAt(0).Coefficients;
                integrationConstants = m_refElement.First().DataRecords.ElementAt(0).IntegrationConstants;
                t_expnts = m_refElement.First().DataRecords.ElementAt(0).TExponents;

                string Species_Name = m_refElement.First().Name;
                double Molecular_Weight = m_refElement.First().MolecularWeight;
                double Enthalpy = m_refElement.First().HeatOfFormation - (m_refElement.First().DataRecords.ElementAt(0).EnthalpyRef / 1000);   // -8.68
                Delta_Enthalpy_Ref = m_refElement.First().HeatOfFormation;
                Cp_Ref = ThermoDynamics.HeatCapacity(Temperature, t_expnts, coefficients);
                Enthalpy_kJmol = ThermoDynamics.Enthalpy(Temperature, t_expnts, coefficients, integrationConstants);
                ref_entropy = 0.0;
                double EnthalpyRef = m_refElement.First().DataRecords.ElementAt(0).EnthalpyRef / 1000;
                Entropy_Ref = ThermoDynamics.Entropy(refTemperature, t_expnts, coefficients, integrationConstants);
                Gibbs_H298JmolK = ThermoDynamics.GibbsRef(refTemperature, ref_entropy, Temperature, coefficients, t_expnts);
                //MU = ThermoDynamics.Calculate_MU(Gibbs_H298JmolK, Temperature, 1);

                MU = MU + Enthalpy_kJmol - Entropy_Ref + Enln + Tm;

                //        "Species_Name": "CH4",
                //"Molecular_Weight": 16.04246,
                //"Enthalpy": -84.616,
                //"Delta_Enthalpy": -66.626,
                //"Delta_Enthalpy_Ref": -74.6,
                //"CP_Ref": 35.691,
                //"Enthalpy_Ref": 10.016,
                //"Entropy_Ref": 186.371
                double zz = -66.626 - Temperature * 186.371 / 1000;
                double deltaG = Enthalpy_kJmol - (Temperature * (Entropy_Ref / 1000));
                double mx = (Temperature * (Entropy_Ref / 1000));
                //MU = MU + Enthalpy_kJmol;
                MU += Gibbs_H298JmolK;
            }

            // CO2 reaction == -394.36
            // C = 558.579  
            // O2 = -205.149  == 353.43
            // CO2 = -607.297

            double delta = 0.005;
            Assert.AreEqual(expected, MU, delta);

        }

        [TestMethod]
        public void TestElementsReferenceCPHS()
        {
            List<string> ElementSymbols = ["C", "O"];
            //var Elements = refElementPolynomials;
            var referenceProperties = ThermoDynamics.ElementsReferenceCPHS(ElementSymbols, refElementPolynomials);
            Assert.IsNotNull(referenceProperties);
        }
        [TestMethod]
        public void TestCH4DeltaHf()
        {
            double tolerance = 0.001;
            const double TR = 298.15;
            List<string> ElementSymbols = ["C", "H"];
            var referenceProperties = ThermoDynamics.ElementsReferenceCPHS(ElementSymbols, refElementPolynomials);

            var CH4properties = from item in nasaPolynomials
                                where item.Name == "CH4"
                                select item;

            // Get HeatOfFormation of CH4 and Calculate Entropy of CH4
            double HofCH4 = CH4properties.First().HeatOfFormation / 1000;
            var expnts = CH4properties.First().DataRecords.ElementAt(0).TExponents;
            var coeff = CH4properties.First().DataRecords.ElementAt(0).Coefficients;
            var integrateC = CH4properties.First().DataRecords.ElementAt(0).IntegrationConstants;
            double Entropy_CH4 = ThermoDynamics.Entropy(TR, expnts, coeff, integrateC);

            //C(gr)
            referenceProperties.TryGetValue("C(gr)", out var C_gr);
            double C_gr_Delta_Enthalpy_Ref = C_gr.Delta_Enthalpy_Ref;
            double C_gr_Entropy_Ref = C_gr.Entropy_Ref;

            // H2
            referenceProperties.TryGetValue("H2", out var H2);
            double H2_Delta_Enthalpy_Ref = H2.Delta_Enthalpy_Ref;
            double H2_Entropy_Ref = H2.Entropy_Ref;

            // Product
            double CH4delta_Hrxn = HofCH4 - (C_gr_Delta_Enthalpy_Ref + (2 * H2_Delta_Enthalpy_Ref));
            double CH4delta_Srxn = Entropy_CH4 - (C_gr_Entropy_Ref + (2 * H2_Entropy_Ref));

            double deltaG = CH4delta_Hrxn - TR * CH4delta_Srxn / 1000;
            double expected = -50.53199;

            Assert.AreEqual(expected, deltaG, tolerance);
        }
        [TestMethod]
        public void TestOverLoadedElementsReferenceCPHS()
        {
            var referenceproperties = ThermoDynamics.ElementsReferenceCPHS(refElementPolynomials);
            Assert.IsNotNull(referenceproperties);
        }
        [TestMethod]
        public void TestShouldBuildReferencePropertiesAt298_15()
        {
            double T = 298.15;
            double TR = 298.15;
            List<string> m_formula = ["C"];
            List<double> temperatureRange = [];
            List<double> coefficients = [];
            List<double> integrationConstants = [];
            List<double> t_expnts = [];
            foreach (string formula in m_formula)
            {
                var m_refElementProperties = from element in refElementPolynomials
                                             where element != null && element.ChemicalFormula.First().Symbol == formula
                                             select element;
                NASAchemicalFormula = m_refElementProperties.First().ChemicalFormula;
                temperatureRange = m_refElementProperties.First().DataRecords.ElementAt(0).TemperatureRange;
                coefficients = m_refElementProperties.First().DataRecords.ElementAt(0).Coefficients;
                integrationConstants = m_refElementProperties.First().DataRecords.ElementAt(0).IntegrationConstants;
                t_expnts = m_refElementProperties.First().DataRecords.ElementAt(0).TExponents;

                string Species_Name = m_refElementProperties.First().Name;
                double Molecular_Weight = m_refElementProperties.First().MolecularWeight;
                double Enthalpy = m_refElementProperties.First().HeatOfFormation - (m_refElementProperties.First().DataRecords.ElementAt(0).EnthalpyRef / 1000);
                double Delta_Enthalpy = m_refElementProperties.First().HeatOfFormation - (m_refElementProperties.First().DataRecords.ElementAt(0).EnthalpyRef / 1000);
                double Delta_Enthalpy_Ref = m_refElementProperties.First().HeatOfFormation;
                double Cp_Ref = ThermoDynamics.HeatCapacity(TR, t_expnts, coefficients);
                double EnthalpyRef = m_refElementProperties.First().DataRecords.ElementAt(0).EnthalpyRef / 1000;
                double Entropy_Ref = ThermoDynamics.Entropy(TR, t_expnts, coefficients, integrationConstants);

                Assert.IsNotNull(Entropy_Ref);

            }
        }

        [DataTestMethod]
        [DataRow(298.15, 186.371)]
        [DataRow(398.15, 197.312)]
        [DataRow(498.15, 207.023)]
        [DataRow(598.15, 216.068)]
        [DataRow(698.15, 224.642)]
        [DataRow(798.15, 232.828)]
        [DataRow(898.15, 240.669)]
        [DataRow(998.15, 248.195)]
        [DataRow(1000.00, 248.331)]
        public void TestMultipleEntropyConditions(double T, double expected)
        {
            // CH4 NASApolynomials.json
            double result = ThermoDynamics.Entropy(T, NASAExponents, NASACoefficients, NASAIntegrationConstants);
            Assert.AreEqual(expected, result, delta);
        }
        [DataTestMethod]
        [DataRow(298.15, 205.1482)]
        public void TestElementWith8Coefficients(double T, double expected)
        {
            //  O2 refElement.json
            double result = ThermoDynamics.Entropy(T, refExponents, refCoefficients, refIntegrationConstants);
            Assert.AreEqual(expected, result, delta);
        }

        [DataTestMethod]
        [DataRow(298.15, 186.371)]
        [DataRow(398.15, 187.782)]
        [DataRow(498.15, 190.684)]
        [DataRow(598.15, 194.179)]
        [DataRow(698.15, 197.933)]
        [DataRow(798.15, 201.796)]
        [DataRow(898.15, 205.691)]
        [DataRow(998.15, 209.575)]
        [DataRow(1000.00, 209.646)]
        public void TestGibbs(double T, double expected)
        {
            double ref_entropy = (double)m_referenceSpecie.First().Entropy_Ref;
            double thermoGibbs = ThermoDynamics.GibbsRef(referenceTemp, ref_entropy, T, NASACoefficients, NASAExponents);
            Assert.AreEqual(expected, thermoGibbs, delta);
        }

        [DataTestMethod]
        [DataRow(298.15, -74.600)]
        [DataRow(398.15, -70.806)]
        [DataRow(498.15, -66.461)]
        [DataRow(598.15, -61.507)]
        [DataRow(698.15, -55.953)]
        [DataRow(798.15, -49.832)]
        [DataRow(898.15, -43.184)]
        [DataRow(998.15, -36.052)]
        [DataRow(1000.00, -35.915)]
        public void TestThermoEnthalpyMethod(double T, double expected)
        {
            double CH4HeatOfFormation = NASA_specie.ElementAt(0).HeatOfFormation;
            double enthalpy = ThermoDynamics.Enthalpy(T, NASAExponents, NASACoefficients, NASAIntegrationConstants);
            Assert.AreEqual(expected, enthalpy, delta);
        }

        [TestMethod]
        public void Test_MU_For_TempRanges()
        {
            List<double> temperatureRange = [200.000, 1000.000];
            List<double> t_expnts = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> coefficients = [-1.766850998e+05, 2.786181020e+03, -1.202577850e+01, 3.917619290e-02, -3.619054430e-05, 2.026853043e-08, -4.976705490e-12];
            List<double> integrationConstants = [-2.331314360e+04, 8.904322750e+01];

            // Arrange T = 298.15 = -50.72 kj/mol, 398.15 = -43.95 kj/mol, 498.15 = -37.75 kj/mol
            // get the Gibbs for CH4
            double m_Gibbs = ThermoDynamics.GibbsRef(298.15, 187.0, 298.15, coefficients, t_expnts);
            double expected_MU = -50.72;
            double temperature = 298.15;
            double pressure = 1.0;
            // Act
            double actual_MU = ThermoDynamics.Calculate_MU(m_Gibbs, temperature, pressure);

            // Assert
            Assert.AreEqual(expected: expected_MU, actual: actual_MU);
        }

    }

    [TestClass]
    public class TestViewModels
    {
        [TestMethod]
        public void TestBaseViewModel()
        {
            var viewModel = new ReactantsViewModel();
            Assert.IsNotNull(viewModel);
        }

        [TestMethod]
        public void TestBasePropellantsViewModel()
        {
            var viewModel = new PropellantViewModel();
            Assert.IsNotNull(viewModel);
        }

        [TestMethod]
        public void AddItem_ShouldIncreasePropellantsCount()
        {
            // Arrange
            ICollection<Reactant> reactants = ThermoService.GetReactants();
            Reactant? searchedItem = reactants?.Where(item => item.Name == "CH4").FirstOrDefault();

            var viewModel = new PropellantViewModel();
            var initialCount = viewModel.ReactantsCollection.Count;

            // Act
            viewModel.AddItem(searchedItem);

            // Assert
            Assert.AreEqual(initialCount + 1, viewModel.ReactantsCollection.Count);

        }

    }
}

