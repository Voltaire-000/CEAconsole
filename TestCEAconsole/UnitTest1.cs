using CEAconsole.Models;
using CEAconsole.Services;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using CEAconsole.ViewModels;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using MathNet.Numerics.LinearAlgebra.Double;

namespace TestCEAconsole
{
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
            ICollection<Reactant> reactants = InputServices.GetJsonData("Data/newShortThermo.json");

            List<Reactant>? filteredCollection = reactants?.Where(item => item.Name == "CH4").ToList();
            var molecularWeight = (from item in filteredCollection
                                   select item.MolecularWeight).FirstOrDefault();
            double expected = 16.0424600;

            Dictionary<string, CEAconsole.Models.Temperature_Range>.ValueCollection? tempRange = (from item in filteredCollection
                                                                                                  select item.TemperatureRange.Values).FirstOrDefault();

            Dictionary<string, double>? chemFormula = (from item in filteredCollection
                                                       select item.Molecule.ChemicalFormula).FirstOrDefault();
            int? elementCount = chemFormula?.Count;
            elementCount ??= 0;
            string? symbol = chemFormula?.ElementAt(0).Key;
            symbol ??= string.Empty;
            double? atoms = chemFormula?.ElementAt(0).Value;
            atoms ??= 0;

            CEAconsole.Models.Temperature_Range? mx = tempRange?.ElementAt(0);

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
    public class TestMatrix
    {
        [TestMethod]
        public void TestMathNetMatrix()
        {
            // CH4 + O2 = CO2 + H2O
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new[,]{
                {1.0, 0.0,  -1.0,  0.0 }, // C balance
                {4.0, 0.0,   0.0, -2.0 }, // H balance
                {0.0, 2.0,  -2.0, -1.0 }, // O balance
                {1.0, 0.0,   0.0,  0.0 }   // Setting CH4
            });
            
            // set values of matrix
            //matrix[0, 0] = 1; matrix[0, 1] = 0; matrix[0, 2] = -1; matrix[0, 3] = 0;
            //matrix[1, 0] = 4; matrix[1, 1] = 0; matrix[1, 2] = 0;  matrix[1, 3] = -2;
            //matrix[2, 0] = 0; matrix[2, 1] = 2; matrix[2, 2] = -2; matrix[2, 3] = -1;
            // count matrix columns should equal 4
            int columnCount = matrix.ColumnCount;

            // create right hand side vector
            Vector<double> rightHandside = Vector<double>.Build.Dense(new[]
            {0.0, 0.0, 0.0, 1.0 });

            // solve the system using Gaussian elimination
            Vector<double> solution = matrix.Solve(rightHandside);

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
            ICollection<Reactant> reactants = InputServices.GetJsonData("Data/newShortThermo.json");
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
        public void TestShouldReturnMoleculeCountAndFormula()
        {
            // Arrange
            string molecule = "CH4";
            ICollection<Reactant> reactants = InputServices.GetJsonData("Data/newShortThermo.json");
            Assert.IsNotNull(reactants);
            var m_Molecule = from item in reactants
                             where item.Name == molecule
                             select item.Molecule;

            var expected = m_Molecule;

            // Act
            //var result = MoleculeOperations.SplitMolecule(molecule);

            // Assert
            Assert.IsNotNull(m_Molecule);

        }
    }

    [TestClass]
    public class MatrixSolvers
    {
        [TestMethod]
        public void TestShouldBuildMatrixAndLoadChemicalFormula()
        {
            // Arrange
            //Matrix<double> defaultMatrix = Matrix<double>.Build.Dense(10, 10, 0.0);
            var defaultMatrix = SparseMatrix.Create(10, 10, 0.0);
            double determinate = defaultMatrix.Determinant();
            bool isSymetric = defaultMatrix.IsSymmetric();
            Assert.IsNotNull(defaultMatrix);
            //
            // create rightHand side vector
            int m_VectorSize = 10;
            //Vector<double> rightHandSide = Vector<double>.Build.Dense(m_VectorSize, 0.0);
            Vector<double> rightHandside = Vector<double>.Build.Dense(new[]
            {0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0 });
            int m_rightHandSideCount = rightHandside.Count;
            rightHandside[m_rightHandSideCount - 1] = 1.0;
            Assert.IsNotNull(rightHandside);
            Assert.AreEqual(m_VectorSize, m_rightHandSideCount);
            //
            // Set the first column (column 0) to the values for the first reactant(Species)
            // Set the number of CH4 molecules to 1 in the last row of the defaultMatrix
            defaultMatrix[9, 0] = 1.0;

            // Reactants Section
            string m_firstReactantMolecule = "CH4";
            string m_secondReactantMolecule = "O2";
            ICollection<Reactant> reactants = InputServices.GetJsonData("Data/newShortThermo.json");
            Assert.IsNotNull(reactants);

            IEnumerable<Molecule> m_firstReactant = from item in reactants
                                                    where item.Name == m_firstReactantMolecule
                                                    select item.Molecule;
            Assert.IsNotNull(m_firstReactant);

            Dictionary<string, double>.KeyCollection keyCollection = m_firstReactant.FirstOrDefault().ChemicalFormula.Keys;
            Dictionary<string, double>.ValueCollection valuesCollection = m_firstReactant.FirstOrDefault().ChemicalFormula.Values;
            int m_keyCount = keyCollection.Count;
            IEnumerable<Molecule> m_secondReactant = from item in reactants
                                                     where item.Name == m_secondReactantMolecule
                                                     select item.Molecule;
            Assert.IsNotNull(m_secondReactant);
            Collection<IEnumerable<Molecule>> reactantsCollection = new();
            reactantsCollection.Add(m_firstReactant);
            reactantsCollection.Add(m_secondReactant);

            // Products Section
            // first product = "CO2
            // second product = H2O
            string m_firstProductMolecule =  "CO2";
            string m_secondProductMolecule = "H2O";

            var m_firstproduct = from item in reactants
                                 where item.Name == m_firstProductMolecule
                                 select item.Molecule;
            Assert.IsNotNull (m_firstproduct);
            var m_secondProduct = from item in reactants
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
                            defaultMatrix[row0, m_column] = Values.ElementAt(i);
                            break;
                        case "D":
                            // matrix row 1
                            int row1 = 1;
                            defaultMatrix[row1, m_column] = Values.ElementAt(i);
                            break;
                        case "He":
                            // matrix row 2
                            int row2 = 2;
                            defaultMatrix[row2, m_column] = Values.ElementAt(i);
                            break;
                        case "Li":
                            // matrix row 3
                            int row3 = 3;
                            defaultMatrix[row3, m_column] = Values.ElementAt(i);
                            break;
                        case "Be":
                            // matrix row 4
                            int row4 = 4;
                            defaultMatrix[row4, m_column] = Values.ElementAt(i);
                            break;
                        case "B":
                            // matrix row 5
                            int row5 = 5;
                            defaultMatrix[row5, m_column] = Values.ElementAt(i);
                            break;
                        case "C":
                            // matrix row 6
                            int row6 = 6;
                            defaultMatrix[row6, m_column] = Values.ElementAt(i);
                            break;
                        case "N":
                            // matrix row 7
                            int row7 = 7;
                            defaultMatrix[row7, m_column] = Values.ElementAt(i);
                            break;
                        case "O":
                            // matrix row 8 // row 9 is set previously
                            int row8 = 8;
                            defaultMatrix[row8, m_column] = Values.ElementAt(i);
                            break;

                        default:
                            break;
                    }
                }
            }

            // solve the system using Gaussian elimination
            Vector<double> m_solution = defaultMatrix.Solve(rightHandside);

            Assert.AreEqual(99, 0);
        }
    }

    [TestClass]
    public class TestServices
    {
        [TestMethod]
        public void Test_InputCardService()
        {
            string json = InputCardService.GetInputCard();
            int x_length = json.Length;

            Assert.AreEqual(589, x_length);
        }

        [TestMethod]
        public void Test_ElementsService()
        {
            string json = ElementsService.GetElements();
            int elementCount = json.Length;

            Assert.AreNotEqual(0, elementCount);
        }

        [TestMethod]
        public void Test_ThermoService()
        {
            ICollection<Reactant> reactants = ThermoService.GetReactants();
            int reactantCount = reactants.Count;

            Assert.AreNotEqual(0, reactantCount);
            Assert.AreEqual(15, reactantCount);
        }

        [TestMethod]
        public void TestInputServicesWithPath()
        {
            ICollection<Reactant> json = InputServices.GetJsonData("Data/moleculeJson.json");
            int reactantCount = json.Count;

            Assert.AreEqual(2, reactantCount);
        }

        [TestMethod]
        public void Test_Ref_DefaultServiceWithPath()
        {
            ICollection<CPHSRef> ref_defaults = InputServices.GetDefaultCPHS("Data/Ref_Defaults.json");
            int ref_defaultsCount = ref_defaults.Count;

            Assert.AreEqual(1258, ref_defaultsCount);
        }

    }

    [TestClass]
    public class TestThermodynamicMethods
    {

        [TestMethod]
        public void Test_New_HeatCapacity()
        {
            List<double> temperatureRange = [200.000, 1000.000];
            List<double> t_expnts = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> coefficients = [-1.766850998e+05, 2.786181020e+03, -1.202577850e+01, 3.917619290e-02, -3.619054430e-05, 2.026853043e-08, -4.976705490e-12];
            List<double> integrationConstants = [-2.331314360e+04, 8.904322750e+01];

            double delta = 0.005;

            double ref_temp = 298.15;
            double T = 1000;
            double Cp = ThermoDynamics.HeatCapacity(T, coefficients, t_expnts);

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
        public void Test_New_Enthalpy(double T, double expected)
        {
            List<double> temperatureRange = [200.000, 1000.000];
            List<double> t_expnts = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> coefficients = [-1.766850998e+05, 2.786181020e+03, -1.202577850e+01, 3.917619290e-02, -3.619054430e-05, 2.026853043e-08, -4.976705490e-12];
            List<double> integrationConstants = [-2.331314360e+04, 8.904322750e+01];

            double delta = 0.005;

            double ref_temp = 298.15;
            double enthalpy = ThermoDynamics.DeltaEnthalpyRef(ref_temp, T, coefficients, t_expnts);

            Assert.AreEqual(expected, enthalpy, delta);

        }

        [TestMethod]
        public void Test_New_Entropy()
        {
            List<double> temperatureRange = [200.000, 1000.000];
            List<double> t_expnts = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> coefficients = [-1.766850998e+05, 2.786181020e+03, -1.202577850e+01, 3.917619290e-02, -3.619054430e-05, 2.026853043e-08, -4.976705490e-12];
            List<double> integrationConstants = [-2.331314360e+04, 8.904322750e+01];

            double delta = 0.005;

            //double ref_Entropy = 186.371;
            double ref_temp = 298.15;

            double T = 398.15;

            double expected = 197.312;
            double Entropy = ThermoDynamics.Entropy(ref_temp, T, coefficients, t_expnts);

            Assert.AreEqual(expected: expected, Entropy, delta);
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
            List<double> temperatureRange = [200.000, 1000.000];
            List<double> t_expnts = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> coefficients = [-1.766850998e+05, 2.786181020e+03, -1.202577850e+01, 3.917619290e-02, -3.619054430e-05, 2.026853043e-08, -4.976705490e-12];
            List<double> integrationConstants = [-2.331314360e+04, 8.904322750e+01];

            double delta = 0.005;

            //double ref_Entropy = 186.371;
            double ref_temp = 298.15;

            double result = ThermoDynamics.Entropy(ref_temp, T, coefficients, t_expnts);
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
            List<double> temperatureRange = [200.000, 1000.000];
            List<double> t_expnts = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> coefficients = [-1.766850998e+05, 2.786181020e+03, -1.202577850e+01, 3.917619290e-02, -3.619054430e-05, 2.026853043e-08, -4.976705490e-12];
            List<double> integrationConstants = [-2.331314360e+04, 8.904322750e+01];

            double delta = 0.005;
            double ref_temp = 298.15;

            double enthalpy = ThermoDynamics.DeltaEnthalpyRef(ref_temp, T, coefficients, t_expnts);
            double entropy = ThermoDynamics.Entropy(ref_temp, T, coefficients, t_expnts);

            double gibbs = -((enthalpy * 1000) - T * entropy) / T;

            double thermoGibbs = ThermoDynamics.GibbsRef(ref_temp, T, coefficients, t_expnts);

            Assert.AreEqual(expected, gibbs, delta);
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
        public void TestEnthalpyNoRefTemperature(double T, double expected)
        {
            List<double> temperatureRange = [200.000, 1000.000];
            List<double> t_expnts = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> coefficients = [-1.766850998e+05, 2.786181020e+03, -1.202577850e+01, 3.917619290e-02, -3.619054430e-05, 2.026853043e-08, -4.976705490e-12];
            List<double> integrationConstants = [-2.331314360e+04, 8.904322750e+01];

            //double GASCONSTANT = 8.31446;
            double delta = 0.005;
            double ref_temp = 298.15;
            double heatOfFormation = -74600.0;
            double ref_enthalpy = heatOfFormation / 1000.0;
            double enthalpy = ThermoDynamics.DeltaEnthalpyRef(ref_temp, T, coefficients, t_expnts);

            double H_enthalpy = ref_enthalpy + enthalpy;

            Assert.AreEqual(expected, H_enthalpy, delta);
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
            List<double> temperatureRange = [200.000, 1000.000];
            List<double> t_expnts = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> coefficients = [-1.766850998e+05, 2.786181020e+03, -1.202577850e+01, 3.917619290e-02, -3.619054430e-05, 2.026853043e-08, -4.976705490e-12];
            List<double> integrationConstants = [-2.331314360e+04, 8.904322750e+01];
            double delta = 0.005;
            double ref_temp = 298.15;
            double enthalpy = ThermoDynamics.Enthalpy(ref_temp, T, coefficients, t_expnts);

            Assert.AreEqual(expected, enthalpy, delta);
        }

        [DataTestMethod]
        [DataRow(398.15, -74.600)]
        [DataRow(498.15, -77.635)]
        [DataRow(598.15, -80.457)]
        [DataRow(698.15, -82.932)]
        [DataRow(798.15, -85.023)]
        [DataRow(898.15, -86.726)]
        [DataRow(998.15, -88.059)]
        [DataRow(998.15, -89.053)]
        [DataRow(1000.00, -89.069)]
        public void TestDeltaHf(double T, double expected)
        {
            List<double> temperatureRange = [200.000, 1000.000];
            List<double> t_expnts = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> coefficients = [-1.766850998e+05, 2.786181020e+03, -1.202577850e+01, 3.917619290e-02, -3.619054430e-05, 2.026853043e-08, -4.976705490e-12];
            List<double> integrationConstants = [-2.331314360e+04, 8.904322750e+01];

            double delta = 0.005;
            double ref_temp = 298.15;

            double Hf = ThermoDynamics.EnthalpyFormation(ref_temp, T, coefficients, t_expnts);

            Assert.AreEqual(expected, Hf);

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

    [TestClass]
    public class TestModifyAddNode
    {
        [TestMethod]
        public void TestAddNode()
        {
            //ICollection<Reactant> reactants = InputServices.GetJsonData("Data/shortThermo.json");
            //int reactantCount = reactants.Count;

            //List<Reactant>? filteredCollection = reactants?.Where(item => item.Name == "CH4").ToList();
            //var molecularWeight = (from item in filteredCollection
            //                       select item.MolecularWeight).FirstOrDefault();
            //double expected = 16.0424600;

            //var elementv = reactants.ElementAt(0);

            //List<DTO_Reactant> dtoList = new();

            //for (int i = 0; i < reactantCount; i++)
            //{
            //    DTO_Reactant dTO_Reactant = new()
            //    {
            //        Molecule = new(),

            //    };

            //    dTO_Reactant.Name = reactants.ElementAt(i).Name;
            //    dTO_Reactant.Description = reactants.ElementAt(i).Description;
            //    dTO_Reactant.T_Intervals = reactants.ElementAt(i).T_Intervals;
            //    dTO_Reactant.Id_Code = reactants.ElementAt(i).Id_Code;
            //    dTO_Reactant.Molecule.Count = 1.0;
            //    dTO_Reactant.Molecule.ChemicalFormula = reactants.ElementAt(i).Molecule.ChemicalFormula;
            //    dTO_Reactant.Gaseous = reactants.ElementAt(i).Gaseous;
            //    dTO_Reactant.MolecularWeight = reactants.ElementAt(i).MolecularWeight;
            //    dTO_Reactant.HeatOfFormation = reactants.ElementAt(i).HeatOfFormation;
            //    dTO_Reactant.TemperatureRange = reactants.ElementAt(i).TemperatureRange;

            //    dtoList.Add(dTO_Reactant);
            //}

            //string serializedList = JsonConvert.SerializeObject(dtoList);
            ////string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "newShortThermo.json");
            ////File.WriteAllText(path, serializedList);

            Assert.AreEqual(0, 0);

        }
    }
}

