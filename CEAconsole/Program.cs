

// See https://aka.ms/new-console-template for more information
using CEAconsole.Models;
using CEAconsole.Services;
using CEAconsole.ThermoChemistry;
using MathNet.Numerics;
using ScottPlot.Colormaps;

// Constants
double REFERENCE_TEMPERATURE = 298.15;

// Dummy Data
double[] DummyData = { -999.123, -999.123, -999.123, -999.123, -999.123, -999.123, -999.123, -999.123, -999.123, -999.123 };
//
// Services Section
string NASAsearchString = "CH4";
ICollection<DTO_Specie> nasaPolynomials = InputServices.GetNASA("Data/NASApolynomials.json");
IEnumerable<DTO_Specie> NASA_specie = from NASAspecie in nasaPolynomials
                                                          where NASAspecie.Name == NASAsearchString
                                                          select NASAspecie;

ICollection<CPHSRef> cPHSRefs = InputServices.GetDefaultCPHS("Data/Ref_Defaults.json");
ICollection<Reactant> ReactantsList = InputServices.GetSpecies("Data/newShortThermo.json");

string fuelName = "CH4";
IEnumerable<CPHSRef> CPHSdefaults = from item in cPHSRefs.Where(r => r.Species_Name == fuelName) select item;

List<double> temperatureRange = new();
List<double> coefficients = new();
List<double> temperatureExponents = new();
List<double> integrationConstants = new();
double H_Enthalpy = 0;

List<double> temperatureList = [];
List<double> heatCapacityList = [];
List<double> enthalpyChangeFromRefList = [];
List<double> entropyList = [];
List<double> gibbsList = [];
List<double> enthalpyList = [];
List<double> deltaHfList = [];
List<double> logKlist = [];

Console.WriteLine( "\nThermoDynamic Functions Calculated from Coefficients for CH4");
Console.WriteLine("\n{0, -16} {1, -15} {2, -20} {3, -20} {4, -20} {5, -20} {6, -20} {7, -20}",
    "\tTemp Kelvin", "Cp J/mol-k", "H-H298.15 kJ/mol", "S J/mol-K", "G-H298.15/T J/mol-K", "H kJ/mol", "delta Hf kJ/mol", "log K");

double defaultEntropyRef = (double)CPHSdefaults.ElementAt(0).Entropy_Ref;
List<double> temperatureSchedule = [0.0, 298.15, 398.15, 498.15, 598.15, 698.15, 798.15, 898.15, 998.15, 1000.00];
ICollection<ReferenceElement> refElements = InputServices.GetReferenceElements("Data/refElements.json");
IEnumerable<ReferenceElement> H2_elementData = from element in refElements
                                               where element.Name == "H2"
                                               select element;
double H2_heatOfFormation = H2_elementData.First().HeatOfFormation;
List<double> H2_coefficients = H2_elementData.First().DataRecords.ElementAt(0).Coefficients;
List<double> H2_integrationConstants = H2_elementData.First().DataRecords.ElementAt(0).IntegrationConstants;
List<double> H2_tExpnts = H2_elementData.First().DataRecords.ElementAt(0).TExponents;
IEnumerable<ReferenceElement> Cg_elementData = from element in refElements
                                               where element.Name == "C(gr)"
                                               select element;
List<double> Cg_coeff = Cg_elementData.First().DataRecords.ElementAt(0).Coefficients;
List<double> Cg_expnts = Cg_elementData.First().DataRecords.ElementAt(0).TExponents;
List<double> Cg_intConstants = Cg_elementData.First().DataRecords.ElementAt(0).IntegrationConstants;
List<double> reactants = new();

foreach (double temperature in temperatureSchedule)
{
    temperatureList.Add(temperature);

    double cp_value = ThermoDynamics.HeatCapacity(temperature, 
                                                  NASA_specie.First().DataRecords.ElementAt(0).TExponents,
                                                  NASA_specie.First().DataRecords.ElementAt(0).Coefficients);
    heatCapacityList.Add(cp_value);

    double enthalpy_change_from_ref_value = ThermoDynamics.EnthalpyRefH298(REFERENCE_TEMPERATURE,
                                                                           temperature,
                                                                           NASA_specie.First().DataRecords.ElementAt(0).TExponents,
                                                                           NASA_specie.First().DataRecords.ElementAt(0).Coefficients
                                                                           );
    enthalpyChangeFromRefList.Add(enthalpy_change_from_ref_value);

    double entropy_value = ThermoDynamics.Entropy(temperature,
                                                  NASA_specie.First().DataRecords.ElementAt(0).TExponents,
                                                  NASA_specie.First().DataRecords.ElementAt(0).Coefficients,
                                                  NASA_specie.First().DataRecords.ElementAt(0).IntegrationConstants);
    entropyList.Add(entropy_value);

    double gibbs_value = ThermoDynamics.GibbsRef(REFERENCE_TEMPERATURE,
                                                 defaultEntropyRef, 
                                                 temperature, 
                                                 NASA_specie.First().DataRecords.ElementAt(0).Coefficients, 
                                                 NASA_specie.First().DataRecords.ElementAt(0).TExponents);
    gibbsList.Add(gibbs_value);

    double enthalpy_value = ThermoDynamics.Enthalpy(temperature, 
                                                    NASA_specie.First().DataRecords.ElementAt(0).TExponents, 
                                                    NASA_specie.First().DataRecords.ElementAt(0).Coefficients, 
                                                    NASA_specie.First().DataRecords.ElementAt(0).IntegrationConstants);
    enthalpyList.Add(enthalpy_value);

    double H2_enthalpy = ThermoDynamics.Enthalpy(temperature, 
                                                 H2_tExpnts, 
                                                 H2_coefficients, 
                                                 H2_integrationConstants);
    reactants.Add(H2_enthalpy + H2_enthalpy);
    double Cg_enthalpy = ThermoDynamics.Enthalpy(temperature, 
                                                 Cg_expnts, 
                                                 Cg_coeff, 
                                                 Cg_intConstants);
    reactants.Add(Cg_enthalpy);

    double deltaH_value = ThermoDynamics.DeltaHf(enthalpy_value, reactants);
    reactants.Clear();
    deltaHfList.Add(deltaH_value);
    double log_k_value = ThermoDynamics.Log_K(gibbs_value, temperature);
    logKlist.Add(log_k_value);
}

int round = 3;
for (int i = 0; i < 10; i++)
{
    Console.WriteLine("{0, -10} {1, -15} {2, -20} {3, -20} {4, -20} {5, -20} {6, -20} {7, -20}", "\t"
            + temperatureList.ElementAt(i) + " :", "\t"
            + heatCapacityList.ElementAt(i).Round(round), 
            enthalpyChangeFromRefList.ElementAt(i).Round(round), 
            entropyList.ElementAt(i).Round(round),
            gibbsList.ElementAt(i).Round(round),
            enthalpyList.ElementAt(i).Round(round),
            deltaHfList.ElementAt(i).Round(round),
            logKlist.ElementAt(i).Round(round));
}

Console.WriteLine("\nThermoDynamic Functions Calculated from Coefficients for Oxygen");
Console.WriteLine("\n{0, -16} {1, -15} {2, -20} {3, -20} {4, -20} {5, -20} {6, -20} {7, -20}",
    "\tTemp Kelvin", "Cp J/mol-k", "H-H298.15 kJ/mol", "S J/mol-K", "G-H298.15/T J/mol-K", "H kJ/mol", "delta Hf kJ/mol", "log K");
// clear all the lists
for (int i = 0; i < 6; i++)
{
    temperatureList.Clear();
    heatCapacityList.Clear();
    enthalpyChangeFromRefList.Clear();
    entropyList.Clear();
    gibbsList.Clear();
    enthalpyList.Clear();
    deltaHfList.Clear();
    logKlist.Clear();
}

string oxidizerName = "O2";
CPHSdefaults = from item in cPHSRefs.Where(r => r.Species_Name == oxidizerName) select item;
defaultEntropyRef = (double)CPHSdefaults.ElementAt(0).Entropy_Ref;
NASAsearchString = "O2";
NASA_specie = from NASAspecie in nasaPolynomials
              where NASAspecie.Name == NASAsearchString
              select NASAspecie;

foreach (double temperature in temperatureSchedule)
{
    temperatureList.Add(temperature);
    double cp_value = ThermoDynamics.HeatCapacity(temperature,
                                              NASA_specie.First().DataRecords.ElementAt(0).TExponents,
                                              NASA_specie.First().DataRecords.ElementAt(0).Coefficients);
    heatCapacityList.Add(cp_value);

    double enthalpy_change_from_ref_value = ThermoDynamics.EnthalpyRefH298(REFERENCE_TEMPERATURE,
                                                                       temperature,
                                                                       NASA_specie.First().DataRecords.ElementAt(0).TExponents,
                                                                       NASA_specie.First().DataRecords.ElementAt(0).Coefficients
                                                                       );
    enthalpyChangeFromRefList.Add(enthalpy_change_from_ref_value);

    double entropy_value = ThermoDynamics.Entropy(temperature,
                                                  NASA_specie.First().DataRecords.ElementAt(0).TExponents,
                                                  NASA_specie.First().DataRecords.ElementAt(0).Coefficients,
                                                  NASA_specie.First().DataRecords.ElementAt(0).IntegrationConstants);
    entropyList.Add(entropy_value);

    double gibbs_value = ThermoDynamics.GibbsRef(REFERENCE_TEMPERATURE,
                                                 defaultEntropyRef,
                                                 temperature,
                                                 NASA_specie.First().DataRecords.ElementAt(0).Coefficients,
                                                 NASA_specie.First().DataRecords.ElementAt(0).TExponents);
    gibbsList.Add(gibbs_value);

    double enthalpy_value = ThermoDynamics.Enthalpy(temperature,
                                                    NASA_specie.First().DataRecords.ElementAt(0).TExponents,
                                                    NASA_specie.First().DataRecords.ElementAt(0).Coefficients,
                                                    NASA_specie.First().DataRecords.ElementAt(0).IntegrationConstants);
    enthalpyList.Add(enthalpy_value);

    double deltaHf_value = 249.175 + (enthalpy_value * 2);
    deltaHfList.Add(deltaHf_value);
    // TODO need to calculate the reaction numbers first
    double log_k_value = ThermoDynamics.Log_K(gibbs_value, temperature);
    logKlist.Add(log_k_value);

}

for (int i = 0; i < 10; i++)
{
    Console.WriteLine("{0, -10} {1, -15} {2, -20} {3, -20} {4, -20} {5, -20} {6, -20} {7, -20}", "\t"
            + temperatureList.ElementAt(i) + " :", "\t"
            + heatCapacityList.ElementAt(i).Round(round),
            enthalpyChangeFromRefList.ElementAt(i).Round(round),
            entropyList.ElementAt(i).Round(round),
            gibbsList.ElementAt(i).Round(round),
            enthalpyList.ElementAt(i).Round(round),
            deltaHfList.ElementAt(i).Round(round),
            logKlist.ElementAt(i).Round(round)); ;
}


