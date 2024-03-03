using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEAconsole
{
    public class Combustion
    {
        public int X { get; set; } // Number of Carbon atoms in the hydrocarbon
        public int Y { get; set; } // Number of Hydrogen atoms in the hydrocarbon

        public Combustion(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public void BalanceEquation()
        {
            int O2 = (X + Y) / 4; // Number of Oxygen molecules required
            int CO2 = X; // Number of Carbon Dioxide molecules produced
            int H2O = Y / 2; // Number of Water molecules produced

            Console.WriteLine($"C{X}H{Y} + {O2}O2 -> {CO2}CO2 + {H2O}H2O");
        }
    }
}
