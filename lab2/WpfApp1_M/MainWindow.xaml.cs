using ClassLibrary1;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1_M
{
    
    public partial class MainWindow : Window
    {
        double[,] matrix = new double[0,0];
        private GeneticAlgorithm ga;
        private Point[] fixedCoordinates;
        private int generationCount = 0;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartGeneticAlgorithm_click(object sender, RoutedEventArgs e)
        {
            if (ga != null)
            {
                ga.Run();
                DrawInitialGraph();
                DisplayGeneration();
            }
            else
            {
                MessageBox.Show("Сначала сгенерируйте матрицу.");
            }
        }
        private void DrawInitialGraph()
        {
            ClearCanvas();

            int cityCount = matrix.GetLength(0);
            GetPointCoordinates(cityCount);

            for (int i = 0; i < cityCount; i++)
            {
                Point point = fixedCoordinates[i];
                Ellipse cityPoint = new Ellipse
                {
                    Fill = Brushes.Blue,
                    Width = 5,
                    Height = 5
                };
                Canvas.SetLeft(cityPoint, point.X - 2.5);
                Canvas.SetTop(cityPoint, point.Y - 2.5);
                GraphCanvas.Children.Add(cityPoint);

                for (int j = 0; j < cityCount; j++)
                {
                    if (i != j)
                    {
                        Line line = new Line
                        {
                            X1 = point.X,
                            Y1 = point.Y,
                            X2 = fixedCoordinates[j].X,
                            Y2 = fixedCoordinates[j].Y,
                            Stroke = Brushes.Gray,
                            StrokeThickness = 0.5
                        };
                        GraphCanvas.Children.Add(line);
                    }
                }
            }
        }
        private void DisplayGeneration()
        {
            ClearCanvas();
            generationCount++;

            LegendTextBlock.Text = $"Generation: {generationCount}\nBest Route Length: {ga.BestRouteLength()}\n{ga.BestRouteToString()}";

            DrawBestRoute();
        }

        private void ClearCanvas()
        {
            GraphCanvas.Children.Clear();
        }
        private void ClearHighlight()
        {
            
            for (int i = 0; i < GraphCanvas.Children.Count; i++)
            {
                if (GraphCanvas.Children[i] is Line line && line.Stroke == Brushes.Green)
                {
                    GraphCanvas.Children.RemoveAt(i);
                    i--;
                }
            }
        }
        private void DrawBestRoute()
        {
            ClearHighlight();
            
            var bestRoute = ga.GetBestRoute();
            if (bestRoute == null || bestRoute.Length == 0) return;

            Point[] bestRouteCoordinates = GetPointCoordinates(bestRoute.Length);

            if (bestRoute.Length > 0)
            {
                Point previousPoint = bestRouteCoordinates[bestRoute[0]];

                for (int i = 1; i < bestRoute.Length; i++)
                {
                    Point currentPoint = bestRouteCoordinates[bestRoute[i]];

                    // Рисуем линию для лучшего пути
                    Line line = new Line
                    {
                        X1 = previousPoint.X,
                        Y1 = previousPoint.Y,
                        X2 = currentPoint.X,
                        Y2 = currentPoint.Y,
                        Stroke = Brushes.Green, // Подсвечиваем линию
                        StrokeThickness = 2
                    };
                    GraphCanvas.Children.Add(line);

                    previousPoint = currentPoint;
                }
            }
            else
            {
                MessageBox.Show("Best route indices are empty after retrieval.");
            }
        }

        private Point[] GetPointCoordinates(int cityCount)
        {

            fixedCoordinates = new Point[cityCount];
            double canvasWidth = GraphCanvas.ActualWidth;
            double canvasHeight = GraphCanvas.ActualHeight;

            for (int i = 0; i < cityCount; i++)
            {
                double x = (canvasWidth / (cityCount + 1)) * (i + 1); // Разделяем пространство по X
                double y = canvasHeight / 2; // Центрируем по Y
                fixedCoordinates[i] = new Point(x, y);
            }
            return fixedCoordinates;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.X)
            {
                // Финальный график
                MessageBox.Show($"Result:\nAt generation {generationCount}\nBest Route Length: {ga.BestRouteLength()}\n{ga.BestRouteToString()}");
            }
            else
            if (e.Key == Key.N)
            {
                // Запуск следующего поколения
                ga.Run();
                DisplayGeneration();
            }
        }
        private void GenerateMatrix_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(InputTextBox.Text, out int size) && size > 0)
            {
                matrix = GenerateDistanceMatrix(size);
                DataTable dataTable = ConvertToDataTable(matrix);
                MatrixDataGrid.ItemsSource = dataTable.DefaultView;
                for (int i = 0; i < size; i++)
                {
                    MatrixDataGrid.Columns.Add(new DataGridTextColumn
                    {
                        Header = $"Колонка {i + 1}",
                        Binding = new System.Windows.Data.Binding($"Column{i}")
                    });
                }
                ga = new GeneticAlgorithm(matrix, populationSize: 10, generations: 10);
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите корректный положительный размер матрицы.");
            }
        }

        static double[,] GenerateDistanceMatrix(int size)
        {
            Random rand = new Random();
            double[,] matrix = new double[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (i == j)
                    {
                        matrix[i, j] = 0;
                    }
                    else
                    {
                        matrix[i, j] = rand.Next(1, 100);
                    }
                }
            }

            // Алгоритм Флойда-Уоршелла
            for (int k = 0; k < size; k++)
            {
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        if (matrix[i, j] > matrix[i, k] + matrix[k, j])
                        {
                            matrix[i, j] = matrix[i, k] + matrix[k, j];
                        }
                    }
                }
            }

            return matrix;
        }

        private DataTable ConvertToDataTable(double[,] matrix)
        {
            DataTable dataTable = new DataTable();

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                dataTable.Columns.Add($"Column{i}");
            }

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                var row = dataTable.NewRow();
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    row[j] = matrix[i, j];
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }
}