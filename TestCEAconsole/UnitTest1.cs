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
            ICollection<Reactant> reactants = ThermoService.GetReactants();

            List<Reactant>? filteredCollection = reactants?.Where(item => item.Name == "CH4").ToList();
            var molecularWeight = (from item in filteredCollection
                                   select item.MolecularWeight).FirstOrDefault();
            double expected = 16.0424600;

            Dictionary<string, CEAconsole.Models.Range>.ValueCollection? tempRange = (from item in filteredCollection
                                                                                      select item.TemperatureRange.Values).FirstOrDefault();

            Dictionary<string, double>? chemFormula = (from item in filteredCollection
                                                       select item.Molecule.ChemicalFormula).FirstOrDefault();
            int? elementCount = chemFormula?.Count;
            elementCount ??= 0;
            string? symbol = chemFormula?.ElementAt(0).Key;
            symbol ??= string.Empty;
            double? atoms = chemFormula?.ElementAt(0).Value;
            atoms ??= 0;

            CEAconsole.Models.Range? mx = tempRange?.ElementAt(0);

            Assert.AreEqual(expected, molecularWeight);
            //Assert.AreEqual(99, tempRange.ElementAt(1));

        }

    }

    [TestClass]
    public class TestMatrix
    {
        //[TestMethod]
        //public void Test_Matrix_Should_Work()
        //{
        //    Matrix4x4 matrix = new Matrix4x4(
        //        1, 2, 3, 4,
        //        5, 6, 7, 8,
        //        9, 10, 11, 12,
        //        13, 14, 15, 16);

        //    bool isIdenty = matrix.IsIdentity;
        //    Matrix4x4 invertedMatrix;
        //    bool isSuccesfull = Matrix4x4.Invert(matrix, out invertedMatrix);
        //    if (isSuccesfull)
        //    {
        //        Console.WriteLine("The inverted matrix is : ");
        //        Console.WriteLine(invertedMatrix);
        //    }
        //    else
        //    {
        //        Console.WriteLine("The matrix is not invertable.");
        //    }

        //    Assert.IsFalse(isIdenty);
        //    Assert.IsFalse(isSuccesfull);
        //    Assert.IsFalse(matrix.IsIdentity);
        //}

        [TestMethod]
        public void TestMathNetMatrix()
        {
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new[,]{
                {1.0, 0.0, -1.0, 0.0 }, // C balance
                {4.0, 0.0, 0.0, -2.0 }, // H balance
                {0.0, 2.0, -2.0, -1.0 }, // O balance
                {1.0, 0.0, 0.0, 0.0 }   // Setting CH4
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
        //    [TestMethod]
        //    //public void TestBalancedEquationSolverWithReactantInput()
        //    //{
        //    //    // Arrange
        //    //    string fuelName = "CH4";
        //    //    string oxidizerName = "O2";
        //    //    ICollection<Reactant> reactants = ThermoService.GetReactants();
        //    //    List<Reactant>? Fuel = reactants?.Where(item => item.Name == fuelName).ToList();
        //    //    List<Reactant>? Oxidizer = reactants?.Where(item => item.Name == oxidizerName).ToList();

        //    //    var fuelElementsList = (from molecule in Fuel
        //    //                            select molecule.ChemicalFormula).FirstOrDefault();
        //    //    int fuelElementsCount = fuelElementsList.Count;

        //    //    var oxidizerElementsList = (from molecule in Oxidizer
        //    //                                select molecule.ChemicalFormula).FirstOrDefault();
        //    //    int oxidizerElementsCount = fuelElementsList.Count;
        //    //    // make the fuel list

        //    //    double fuel_carbon_value = 0.0;
        //    //    double fuel_hydrogen_value = 0.0;
        //    //    double fuel_oxygen_value = 0.0;
        //    //    double oxidizer_carbon_value = 0.0;
        //    //    double oxidizer_hydrogen_value = 0.0;
        //    //    double oxidizer_oxygen_value = 0.0;

        //    //    int firstFuelElement = fuelElementsCount - fuelElementsCount;
        //    //    string carbon_key = fuelElementsList.ElementAt(fuelElementsCount - fuelElementsCount).Key;
        //    //    fuel_carbon_value = fuelElementsList.ElementAt(fuelElementsCount - fuelElementsCount).Value;
        //    //    fuel_hydrogen_value = fuelElementsList.ElementAt(fuelElementsCount - 1).Value;
        //    //    oxidizer_oxygen_value = oxidizerElementsList.ElementAt(oxidizerElementsCount - oxidizerElementsCount).Value;

        //    //    var fuelKeys = fuelElementsList.Keys;
        //    //    double value = 0.0;
        //    //    double[] fuelArray = [];


        //    //    double[,] matrixValues =
        //    //    {
        //    //        {fuel_carbon_value, oxidizer_carbon_value, -1.0,0.0},
        //    //        {fuel_hydrogen_value, oxidizer_hydrogen_value , 0.0, -2.0},
        //    //        { fuel_oxygen_value,oxidizer_oxygen_value, -2.0, -1.0 },
        //    //        {1.0, 0.0, 0.0, 0.0 }
        //    //    };
        //    //    Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(matrixValues);
        //    //    // create right hand side vector
        //    //    Vector<double> rightHandside = Vector<double>.Build.Dense(new[]
        //    //    {0.0, 0.0, 0.0, 1.0 });
        //    //    // solve the system using Gaussian elimination
        //    //    Vector<double> solution = matrix.Solve(rightHandside);

        //    //    Matrix<double> defaultMatrix = Matrix<double>.Build.Dense(4,4,0.0);
        //    //    defaultMatrix[0, 0] = 1.0; defaultMatrix[0, 1] = 0.0; defaultMatrix[0, 2] = -1.0; defaultMatrix[0, 3] = 0.0;
        //    //    defaultMatrix[1, 0] = 4.0; defaultMatrix[1, 1] = 0.0; defaultMatrix[1, 2] = 0.0;  defaultMatrix[1, 3] = -2.0;
        //    //    defaultMatrix[2, 0] = 0.0; defaultMatrix[2, 1] = 2.0; defaultMatrix[2, 2] = -2.0; defaultMatrix[2, 3] = -1.0;
        //    //    defaultMatrix[3, 0] = 1.0; defaultMatrix[3, 1] = 0.0; defaultMatrix[3, 2] = 0.0;  defaultMatrix[3, 3] = 0.0;

        //    //    Vector<double> rightside = Vector<double>.Build.Dense(new[]
        //    //    {0.0, 0.0, 0.0, 1.0 });
        //    //    // solve the system using Gaussian elimination
        //    //    Vector<double> m_solution = matrix.Solve(rightside);

        //    //    Matrix<double> m_matrix = defaultMatrix.Transpose();





        //    //    Assert.AreEqual(99, 0);

        //    ////}
        //}

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
                Assert.AreEqual(2, reactantCount);
            }

            [TestMethod]
            public void TestInputServicesWithPath()
            {
                ICollection<Reactant> json = InputServices.GetJsonData();
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

    }
}

