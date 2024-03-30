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

    }

    [TestClass]
    public class TestMatrix
    {
        [TestMethod]
        public void TestMathNetMatrix()
        {
            // CH4 + O2 = CO2 + H2O
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new[,]{
                {1.0, 0.0, -1.0,  0.0 }, // C balance
                {4.0, 0.0,  0.0, -2.0 }, // H balance
                {0.0, 2.0, -2.0, -1.0 }, // O balance
                {1.0, 0.0,  0.0,  0.0 }   // Setting CH4
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
            oxidizerElementsList.TryGetValue("C", out double reactant_oxidizer_carbon_value) ;
            oxidizerElementsList.TryGetValue("H", out double reactant_oxidizer_hydrogen_value);
            oxidizerElementsList.TryGetValue("O", out double reactant_oxidizer_oxygen_value) ;

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
            //balancedH2O.Count = m_solution[3];
            //balancedH2O.ChemicalFormula.Add("H", 2.0);
            //balancedH2O.ChemicalFormula.Add("O", 1.0);

            //Molecule balancedH2OMolecule = new()
            //{
            //    Count = m_solution[3],
            //    ChemicalFormula = new Dictionary<string, double>()

            //};

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

        [TestMethod]
        public void Test_New_Enthalpy()
        {
            List<double> temperatureRange = [200.000, 1000.000];
            List<double> t_expnts = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> coefficients = [-1.766850998e+05, 2.786181020e+03, -1.202577850e+01, 3.917619290e-02, -3.619054430e-05, 2.026853043e-08, -4.976705490e-12];
            List<double> integrationConstants = [-2.331314360e+04, 8.904322750e+01];

            double delta = 0.005;

            double ref_temp = 298.15;
            double T = 0.0;

            double Enthalpy_298 = ThermoDynamics.Enthalpy(ref_Temp: ref_temp, 298.15, coefficients, t_expnts);
            double Enthalpy_398 = ThermoDynamics.Enthalpy(ref_temp, 398.15, coefficients, t_expnts);
            double Enthalpy_498 = ThermoDynamics.Enthalpy(ref_temp, 498.15, coefficients, t_expnts);
            double Enthalpy_598 = ThermoDynamics.Enthalpy(ref_temp, 598.15, coefficients, t_expnts);
            double Enthalpy_698 = ThermoDynamics.Enthalpy(ref_temp, 698.15, coefficients, t_expnts);
            double Enthalpy_798 = ThermoDynamics.Enthalpy(ref_temp, 798.15, coefficients, t_expnts);
            double Enthalpy_898 = ThermoDynamics.Enthalpy(ref_temp, 898.15, coefficients, t_expnts);
            double Enthalpy_998 = ThermoDynamics.Enthalpy(ref_temp, 998.15, coefficients, t_expnts);
            double Enthalpy_1000 = ThermoDynamics.Enthalpy(ref_temp, 1000, coefficients, t_expnts);

            Assert.AreEqual(0, Enthalpy_298, delta);
            Assert.AreEqual(3.794, Enthalpy_398, delta);
            Assert.AreEqual(8.139, Enthalpy_498, delta);
            Assert.AreEqual(13.093, Enthalpy_598, delta);
            Assert.AreEqual(18.647, Enthalpy_698, delta);
            Assert.AreEqual(24.768, Enthalpy_798, delta);
            Assert.AreEqual(31.416, Enthalpy_898, delta);
            Assert.AreEqual(38.548, Enthalpy_998, delta);
            Assert.AreEqual(38.685, Enthalpy_1000, delta);
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

