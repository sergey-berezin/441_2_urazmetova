using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class Route
    {
        public List<int> Cities { get; private set; }
        private double[,] distanceMatrix;

        public Route(double[,] distanceMatrix)
        {
            Cities = Enumerable.Range(0, distanceMatrix.GetLength(0)).ToList();
            this.distanceMatrix = distanceMatrix;
            Shuffle();
        }

        public Route(List<int> cities, double[,] distanceMatrix)
        {
            Cities = cities;
            this.distanceMatrix = distanceMatrix;
        }

        public double Length
        {
            get
            {
                double length = 0;
                for (int i = 0; i < Cities.Count; i++)
                {
                    int nextCity = (i + 1) % Cities.Count;
                    length += distanceMatrix[Cities[i], Cities[nextCity]];
                }
                return length;
            }
        }

        private void Shuffle()
        {
            Random rand = new Random();
            for (int i = Cities.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (Cities[i], Cities[j]) = (Cities[j], Cities[i]);
            }
        }

        public void Mutate()
        {
            Random rand = new Random();
            int i = rand.Next(Cities.Count);
            int j = rand.Next(Cities.Count);
            (Cities[i], Cities[j]) = (Cities[j], Cities[i]);
        }
    }
}
