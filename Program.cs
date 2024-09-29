using ClassLibrary1;
using System;

class Program
{
    static void Main(string[] args)
    {
        double[,] distances = {
            { 0, 2, 4, 7, 13, 11 },
            { 2, 0, 9, 15, 10, 3 },
            { 4, 9, 0, 10, 5, 1 },
            { 7, 15, 10, 0, 8, 5 },
            { 13, 10, 5, 8, 0, 14 },
            { 11, 3, 1, 5, 14, 0 }
        };

        var ga = new GeneticAlgorithm(distances, populationSize: 10, generations: 10);
        Route bestRoute = ga.Run();

        Console.WriteLine("Best Route Length: " + bestRoute.Length);
        Console.WriteLine("Best Route: " + string.Join(" -> ", bestRoute.Cities));
    }
}
