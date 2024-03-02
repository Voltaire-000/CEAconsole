﻿

// See https://aka.ms/new-console-template for more information
using CEAconsole.Models;
using CEAconsole.Services;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

ThermoService thermoService = new();
ElementsService elementsService = new();
TransportService transportService = new();

//List<Reactant>? reactants = new(thermoService.GetReactants());
//string json = JsonConvert.SerializeObject(reactants, Formatting.Indented);

//List<Element>? elements = new(elementsService.GetElements());
//string json = JsonConvert.SerializeObject(elements, Formatting.Indented);

List<TransportProperty>? transportProperties = new(transportService.GetTransportProperties());
string json = JsonConvert.SerializeObject(transportProperties, Formatting.Indented);

Console.WriteLine(json);

//Console.WriteLine("Hello, World!");
