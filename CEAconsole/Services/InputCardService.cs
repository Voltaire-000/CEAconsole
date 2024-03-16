using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace CEAconsole.Services
{
    public static class InputCardService
    {
        private static readonly string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/H2AirInp.json");
        private static readonly string json = File.ReadAllText(path);

        public static string GetInputCard()
        {
            return json;
        }
    }
}
