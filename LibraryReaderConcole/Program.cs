// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

// Load the schema
var schemaJson = File.ReadAllText("coeffSchema.json");
var schema = JSchema.Parse(schemaJson);

// Load the data from the text file
var data = File.ReadAllText("ceaCoeff.txt");
var lines = data.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

// Parse each line into a dictionary
var records = new List<Dictionary<string, object>>();
foreach (var line in lines)
{
    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length == 8)
    {
        var record = new Dictionary<string, object>
                {
                    { "Species_Name", parts[0] },
                    { "Molecular_Weight", double.Parse(parts[1]) },
                    { "Enthalpy", double.Parse(parts[2]) },
                    { "Delta_Enthalpy", double.Parse(parts[3]) },
                    { "Delta_Enthalpy_Ref", double.Parse(parts[4]) },
                    { "CP_Ref", double.Parse(parts[5]) },
                    { "Enthalpy_Ref", double.Parse(parts[6]) },
                    { "Entropy_Ref", double.Parse(parts[7]) }
                };
        records.Add(record);
    }
}

// Convert the list of dictionaries to JSON
var json = JsonConvert.SerializeObject(records, Formatting.Indented);

// Write the JSON to a file
System.IO.File.WriteAllText("output.json", json);


// Validate the data against the schema
//bool isValid = jsonData.IsValid(schema);
//if (!isValid)
//{
//    Console.WriteLine("The data is not valid according to the schema.");
//    return;
//}

// If the data is valid, write it to a JSON file
//File.WriteAllText("output.json", jsonData.ToString());
Console.WriteLine("The data has been written to output.json.");


//Console.WriteLine("Hello, World!");
