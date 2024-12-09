using ClassLibrary1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace WpfApp1_M
{
    public class Manager
    {
        private const string ExperimentsFile = "runs.json";
        private const string PopulationDirectory = "PopulationData";

        public Manager()
        {
            if (!Directory.Exists(PopulationDirectory))
            {
                Directory.CreateDirectory(PopulationDirectory);
            }
        }

        public List<Experiment> LoadExperiments()
        {
            if (File.Exists(ExperimentsFile))
            {
                string json = File.ReadAllText(ExperimentsFile);
                return JsonConvert.DeserializeObject<List<Experiment>>(json) ?? new List<Experiment>();
            }
            return new List<Experiment>();
        }

        public void SaveExperiment(string experimentName, List<Route> population, double[,] distanceMatrix)
        {
            string populationFile = Path.Combine(PopulationDirectory, $"{experimentName}_population.json");

            var populationData = new PopulationData
            {
                DistanceMatrix = distanceMatrix,
                Population = population.Select(route => new RouteData
                {
                    Cities = route.Cities,
                    Length = route.Length
                }).ToList()
            };

            string populationJson = JsonConvert.SerializeObject(populationData, Formatting.Indented);
            File.WriteAllText(populationFile, populationJson);

            List<Experiment> experiments = LoadExperiments();

            var existingExperiment = experiments.FirstOrDefault(e => e.Name == experimentName);
            if (existingExperiment == null)
            {
                experiments.Add(new Experiment
                {
                    Name = experimentName,
                    PopulationFileName = populationFile
                });

                string experimentsJson = JsonConvert.SerializeObject(experiments, Formatting.Indented);
                File.WriteAllText(ExperimentsFile, experimentsJson);
            }
        }
        
        public void LoadExperimentPopulation(string experimentName, GeneticAlgorithm ga)
        {
            string runsFileName = "runs.json";
            string runsJson = File.ReadAllText(runsFileName);
            var experiments = JsonConvert.DeserializeObject<List<Experiment>>(runsJson);

            var experiment = experiments.FirstOrDefault(e => e.Name == experimentName);
            if (experiment != null)
            {
                string populationFileName = experiment.PopulationFileName;
                string populationJson = File.ReadAllText(populationFileName);
                var populationData = JsonConvert.DeserializeObject<PopulationData>(populationJson);

                ga.distanceMatrix = populationData.DistanceMatrix;
                ga.SetPopulation(populationData.Population.Select(r => new Route(r.Cities, populationData.DistanceMatrix)).ToList());

                Console.WriteLine($"Эксперимент {experimentName} загружен.");
            }
            else
            {
                Console.WriteLine($"Эксперимент с именем {experimentName} не найден.");
            }
        }
    }

    public class Experiment
    {
        public string Name { get; set; }
        public string PopulationFileName { get; set; }
    }
    public class PopulationData
    {
        public double[,] DistanceMatrix { get; set; }
        public List<RouteData> Population { get; set; }
    }

    public class RouteData
    {
        public List<int> Cities { get; set; }
        public double Length { get; set; }
    }
}
