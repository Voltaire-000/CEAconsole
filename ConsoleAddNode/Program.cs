

using ConsoleAddNode;
using System.Text.Json;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

string json = File.ReadAllText("NASApolynomials.json");

List<Specie>? NASAspecies = System.Text.Json.JsonSerializer.Deserialize<List<Specie>>(json);

//DTO_Specie dTO_Specie = new();
List<DTO_Specie> NASA_DTO = new();

List<Specie> newList = new List<Specie>();

foreach (var specie in NASAspecies)
{
    DTO_Specie dTO_Specie = new DTO_Specie();

    dTO_Specie.Name = specie.Name;
    dTO_Specie.Description = specie.Description;
    dTO_Specie.TempIntervals = specie.TempIntervals;
    dTO_Specie.IdCode = specie.IdCode;

    Molecule newMolecule = new Molecule();
    newMolecule.ChemicalFormula = specie.ChemicalFormula;
    

    //dTO_Specie.Molecule.ChemicalFormula.Add(new ChemicalFormula
    //{
    //    Symbol = specie.ChemicalFormula.First().Symbol

    //})

    
    //newMolecule.ChemicalFormula = new Dictionary<string, double>();
    newMolecule.Count = 1.0;
    var chemDict = new Dictionary<string, double>();
    //int count = specie.ChemicalFormula.Count;
    int count = specie.ChemicalFormula.Count;
    //for (int i = 0; i < count; i++)
    //{
        

    //    //var mkey = specie.ChemicalFormula.ElementAt(i).Symbol;
    //    //var mvalue = specie.ChemicalFormula.ElementAt(i).NumberOfAtoms;
    //    //newMolecule.ChemicalFormula.Add(mkey, mvalue);

    //    //chemDict.Add(newMolecule.ChemicalFormula.ElementAt(0).Key, newMolecule.ChemicalFormula.ElementAt(0).Value);
    //}

    dTO_Specie.Molecule = newMolecule;
        dTO_Specie.PhaseValue = specie.PhaseValue;
        dTO_Specie.MolecularWeight = specie.MolecularWeight;
        dTO_Specie.HeatOfFormation = specie.HeatOfFormation;
        dTO_Specie.BoilingPoint = specie.BoilingPoint;


    //var datacount = specie.DataRecords.Count;

    dTO_Specie.DataRecords = specie.DataRecords;

    NASA_DTO.Add(dTO_Specie);

}


//var options = new JsonSerializerOptions
//{
//    WriteIndented = false // Set this to false to remove new lines and indentation
//};
//string updatedNASApolynomials = JsonSerializer.Serialize(dTO_Species, options);

var m_json = JsonConvert.SerializeObject(NASA_DTO, Formatting.Indented);

File.WriteAllText("ModNASAspecies.json", m_json);
Console.WriteLine("Hello, World!");
