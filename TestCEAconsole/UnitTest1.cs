using CEAconsole.Models;
using CEAconsole.Services;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            string json = ThermoService.GetReactants();
            int reactantCount = json.Length;

            Assert.AreNotEqual(0, reactantCount);
        }

    }

    [TestClass]
    public class TestThermodynamicMethods
    {
        [TestMethod]
        public void TestHeatCapacity()
        {
            //"temperatureRange": [ 200.000, 1000.000 ],
            //"coefficients": [ -1.766850998e+05, 2.786181020e+03, -1.202577850e+01, 3.917619290e-02, -3.619054430e-05, 2.026853043e-08, -4.976705490e-12 ]
            //"integrationConstants": [ -2.331314360e+04, 8.904322750e+01 ]
            //List<JToken> Kelvin = [200.000, 1000.000];
            double Kelvin_298_15 = 298.15;
            double Kelvin_1000 = 1000;
            List<JToken> coefficientsList = [-1.766850998e+05, 2.786181020e+03, -1.202577850e+01, 3.917619290e-02, -3.619054430e-05, 2.026853043e-08, -4.976705490e-12];
            List<JToken> integrationConstants = [-2.331314360e+04, 8.904322750e+01];
            double temp298_15 = ThermoDynamics.GetHeatCapacity(coefficientsList, integrationConstants, Kelvin_298_15);
            double temp_1000 = ThermoDynamics.GetHeatCapacity(coefficientsList, integrationConstants, Kelvin_1000);
            double delta = 0.005;
            Assert.AreEqual(35.6911, temp298_15, delta);
            Assert.AreEqual(73.676, temp_1000, delta);
        }

        [TestMethod]
        public void TestEnthalpy()
        {
            //"temperatureRange": [ 200.000, 1000.000 ],
            //"coefficients": [ -1.766850998e+05, 2.786181020e+03, -1.202577850e+01, 3.917619290e-02, -3.619054430e-05, 2.026853043e-08, -4.976705490e-12 ]
            //"integrationConstants": [ -2.331314360e+04, 8.904322750e+01 ]
            double Kelvin_298_15 = 298.15;
            double Kelvin_1000 = 1000;
            List<JToken> coefficientsList = [-1.766850998e+05, 2.786181020e+03, -1.202577850e+01, 3.917619290e-02, -3.619054430e-05, 2.026853043e-08, -4.976705490e-12];
            List<JToken> integrationConstants = [-2.331314360e+04, 8.904322750e+01];
            double temp298_15 = ThermoDynamics.GetEnthalpy(coefficientsList, integrationConstants, Kelvin_298_15);
            double delta = 0.005;
            Assert.AreEqual(99, temp298_15, delta);
        }

    }

}

