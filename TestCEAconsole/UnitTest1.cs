using CEAconsole.Models;
using CEAconsole.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace TestCEAconsole
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Test_InputCardService()
        {
            string json = InputCardService.GetInputCard();
            int x_length = json.Length;

            Assert.AreEqual(589, x_length);
        }

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
}