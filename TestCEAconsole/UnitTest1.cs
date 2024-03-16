using CEAconsole.Models;
using CEAconsole.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace TestCEAconsole
{
    [TestClass]
    public class UnitTest1
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
            double Kelvin_298_15 = 698.15;
            double Kelvin_1000 = 1000;
            List<JToken> coefficientsList = [-1.766850998e+05, 2.786181020e+03, -1.202577850e+01, 3.917619290e-02, -3.619054430e-05, 2.026853043e-08, -4.976705490e-12];
            List<JToken> integrationConstants = [-2.331314360e+04, 8.904322750e+01];
            double temp298_15 = ThermoDynamics.GetEnthalpy(coefficientsList, integrationConstants, Kelvin_298_15);
            double delta = 0.005;
            Assert.AreEqual(99, temp298_15, delta);
        }

    }

}

