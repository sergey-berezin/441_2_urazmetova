using ClassLibrary1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public void SaveExperiment(string experimentName, List<Route> population)
        {
            string populationFile = Path.Combine(PopulationDirectory, $"{experimentName}_population.json");

            string populationJson = JsonConvert.SerializeObject(population, Formatting.Indented);
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
            List<Experiment> experiments = LoadExperiments();
            var experiment = experiments.FirstOrDefault(e => e.Name == experimentName);

            if (experiment == null)
            {
                throw new Exception("Эксперимент не найден.");
            }

            string populationFile = experiment.PopulationFileName;

            if (File.Exists(populationFile))
            {
                string populationJson = File.ReadAllText(populationFile);
                var population = JsonConvert.DeserializeObject<List<Route>>(populationJson);
                ga.SetPopulation(population);
            }
            else
            {
                throw new Exception("Популяция для эксперимента не найдена.");
            }
        }
    }

    public class Experiment
    {
        public string Name { get; set; }
        public string PopulationFileName { get; set; }
    }
}
