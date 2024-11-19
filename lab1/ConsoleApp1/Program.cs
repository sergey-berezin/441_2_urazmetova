using ClassLibrary1;
using System;

class Program
{
    static void Main(string[] args)
    {
        double[,] distances = {
            { 0, 9, 5, 7, 13, 11 },
            { 9, 0, 9, 15, 10, 3 },
            { 5, 9, 0, 10, 11, 7 },
            { 7, 15, 10, 0, 8, 16 },
            { 13, 10, 11, 8, 0, 14 },
            { 11, 3, 7, 16, 14, 0 }
        };

        GeneticAlgorithm ga = new GeneticAlgorithm(distances, populationSize: 10, generations: 10);

        ConsoleKeyInfo cki;
        Console.Clear();
        int count = 0;

        while (true)
        {
            Console.WriteLine("Generation " + ++count);
            Console.WriteLine("Best Route Length: " + ga.BestRouteLength());
            Console.WriteLine(ga.BestRouteToString());

            Console.WriteLine();

            cki = Console.ReadKey(true);

            // Exit if the user pressed the 'X' key.
            if (cki.Key == ConsoleKey.X) break;

            ga.Run();
        }

        Console.WriteLine("\nResult:\n");
        Console.WriteLine("At generation " + count);
        Console.WriteLine("Best Route Length: " + ga.BestRouteLength());
        Console.WriteLine(ga.BestRouteToString());


        //Route bestRoute = ga.Run();

        //Console.WriteLine("Best Route Length: " + bestRoute.Length);
        //Console.WriteLine("Best Route: " + string.Join(" -> ", bestRoute.Cities));
    }
}
