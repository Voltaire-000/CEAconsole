

using ConsoleAddNode;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.ComponentModel.DataAnnotations;

//var jsonString = Service.GetNASA("NASApolynomials.json");
string json = File.ReadAllText("NASApolynomials.json");



List<Specie>? species = JsonSerializer.Deserialize<List<Specie>>(json);

DTO_Specie dTO_Specie = new();
List<DTO_Specie> dTO_Species = new();

List<Specie> newList = new List<Specie>();
var newMolecule = new Molecule();
foreach (var specie in species)
{
    dTO_Specie.Name = specie.Name;
    dTO_Specie.Description = specie.Description;
    dTO_Specie.TempIntervals = specie.TempIntervals;
    dTO_Specie.IdCode = specie.IdCode;

    newMolecule.Count = 1.0;
    var chemDict = new Dictionary<string, double>();
    int count = specie.ChemicalFormula.Count;
    for (int i = 0; i < count; i++)
    {
        newMolecule.ChemicalFormula = new Dictionary<string, double>
        {
            {specie.ChemicalFormula.ElementAt(i).Symbol,
                specie.ChemicalFormula.ElementAt(i).NumberOfAtoms }
        };
        //chemDict.Add(newMolecule.ChemicalFormula.ElementAt(0).Key, newMolecule.ChemicalFormula.ElementAt(0).Value);
        newMolecule.ChemicalFormula.Add(newMolecule.ChemicalFormula.ElementAt(0).Key, newMolecule.ChemicalFormula.ElementAt(0).Value);

    }

    // fix here
    //for (int i = 0; i < chemDict.Count; i++)
    //{
    //    var m_symbol = chemDict.ElementAt(i).Key;
    //    var m_value = chemDict.ElementAt(i).Value;

    //    newMolecule.ChemicalFormula.Add(m_symbol, m_value);

    //}

    //dTO_Specie.Molecule.ChemicalFormula = null;
        dTO_Specie.PhaseValue = specie.PhaseValue;
        dTO_Specie.MolecularWeight = specie.MolecularWeight;
        dTO_Specie.HeatOfFormation = specie.HeatOfFormation;
        dTO_Specie.DataRecords = specie.DataRecords;

    dTO_Species.Add(dTO_Specie);
}


int sum = 99;


Console.WriteLine("Hello, World!");
