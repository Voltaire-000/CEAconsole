using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace CEAconsole.Models
{
    public static class BalanceEquation
    {
        public static string HydrocarbonAndOxygen(Reactant Fuel, Reactant Oxidizer)
        {
            // Set elements counts in the matrix
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new[,]
            {
                {1.0, 0.0, -1.0, 0.0 },
                {4.0, 0.0, 0.0, -2.0},
                {0.0, 2.0, -2.0, -1.0},
                {1.0, 0.0, 0.0, 0.0 }
            });
            return "a";
            //return JsonSerializer.Serialize(equation, options);
        }

    }
}
