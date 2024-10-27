using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class GeneticAlgorithm
    {
        public List<Route> population;
        private double[,] distanceMatrix;
        private int populationSize;
        private int generations;

        public GeneticAlgorithm(double[,] distanceMatrix, int populationSize, int generations)
        {
            this.distanceMatrix = distanceMatrix;
            this.populationSize = populationSize;
            this.generations = generations;
            population = new List<Route>();

            for (int i = 0; i < populationSize; i++)
            {
                population.Add(new Route(distanceMatrix));
            }
        }

        public void Run()
        {
            //for (int generation = 0; generation < generations; generation++)
            //{
            Random rand = new Random();
            population = population.OrderBy(r => r.Length).ToList();
            List<Route> newPopulation = new List<Route>();

            for (int i = 0; i < populationSize / 2; i++)
            {
                newPopulation.Add(population[i]);
            }

            var tasks = new Task[populationSize - newPopulation.Count];

            var sem = new SemaphoreSlim(1, 1);

            int Count = newPopulation.Count;

            for (int i = newPopulation.Count; i < populationSize; i++)
            {


                tasks[i - Count] = Task.Factory.StartNew(pi =>
                {
                    int populationSize = (int)pi;
                    Route parent1 = population[rand.Next(populationSize / 2)];
                    Route parent2 = population[rand.Next(populationSize / 2, populationSize)];
                    Route child = Cross(parent1, parent2);
                    child.Mutate();
                    sem.Wait();
                    newPopulation.Add(child);
                    sem.Release();
                }, populationSize);

            }
            Task.WaitAll(tasks);
            /*while (newPopulation.Count < populationSize)
            {
                Route parent1 = population[rand.Next(populationSize / 2)];
                Route parent2 = population[rand.Next(populationSize / 2, populationSize)];

                Route child = Cross(parent1, parent2);
                child.Mutate();
                newPopulation.Add(child);

            }*/

            /*foreach (Route route in newPopulation)
            {
                Console.WriteLine(string.Join(" -> ", route.Cities));

            }*/

            population = newPopulation;
            //Route bestRoute = population.OrderBy(r => r.Length).First();
            //Console.WriteLine($"Generation: {generation}, Best Length: {bestRoute.Length}");
            //Console.WriteLine($"Best Route: {string.Join(", ", bestRoute.Cities)}");
            //}

            //return population.OrderBy(r => r.Length).First();
        }

        private Route Cross(Route parent1, Route parent2)
        {
            int length = parent1.Cities.Count;
            int[] childCities = new int[length];

            Random rand = new Random();

            int crossoverPoint = rand.Next(1, length - 1);

            for (int i = 0; i < crossoverPoint; i++)
            {
                childCities[i] = parent1.Cities[i];
            }

            int currentIndex = crossoverPoint;

            for (int i = 0; i < length; i++)
            {
                int city = parent2.Cities[i];
                if (!childCities.Take(crossoverPoint).Contains(city))
                {
                    childCities[currentIndex] = city;
                    currentIndex++;
                    if (currentIndex >= length) break;
                }
            }

            return new Route(childCities.ToList(), distanceMatrix);
        }

        public double BestRouteLength()
        {
            return population.Min(elem => elem.Length);
        }

        public int[] GetBestRoute()
        {
            var bestRoute = population.OrderBy(r => r.Length).First();
            return bestRoute.Cities.ToArray();
        }
        public string BestRouteToString()
        {
            double bestLength = BestRouteLength();
            List<Route> populationbest = new List<Route>();
            foreach (Route route in population)
                if (route.Length == bestLength)
                {
                    populationbest.Add(route);
                }
            string res = "Best Route: ";
            foreach (Route route in populationbest)
            {
                res += string.Join(" -> ", route.Cities);
                res += "\n";
            }

            return res;
        }
    }

}
