using ClassLibrary1;
using System.Data;
using System.Runtime.CompilerServices;
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
        double[,] matrix = new double[0, 0];
        private GeneticAlgorithm ga;
        private Point[] fixedCoordinates;
        private List<Line> allLines = new List<Line>();
        private int generationCount = 0;

        private Manager experimentManager;
        private CancellationTokenSource cancellationTokenSource;
        public MainWindow()
        {
            InitializeComponent();
            experimentManager = new Manager();
            LoadExperimentsList();
            ga = new GeneticAlgorithm(new double[,] { { 0, 2, 9 },
                                                      { 2, 0, 6 },
                                                      { 9, 6, 0 } }, 10, 10);
        }

        private async void StartGeneticAlgorithm_click(object sender, RoutedEventArgs e)
        {
            if (ga != null)
            {
                cancellationTokenSource = new CancellationTokenSource();
                CancellationToken token = cancellationTokenSource.Token;
                //ga.Run();
                await Task.Factory.StartNew(() => RunGeneticAlgorithm(token), token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                //DrawInitialGraph(GraphCanvas, matrix);
                DisplayGeneration();
            }
            else
            {
                MessageBox.Show("Сначала сгенерируйте матрицу.");
            }
        }
        private void RunGeneticAlgorithm(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                ga.Run();
                Dispatcher.Invoke(() => DisplayGeneration());

                Thread.Sleep(300);
            }
        }
        private void DrawInitialGraph(Canvas canvas, double[,] distanceMatrix)
        {
            ClearCanvas();
            int size = distanceMatrix.GetLength(0);
            Random rand = new Random();
            Point[] points = new Point[size];

            double centerX = canvas.ActualWidth / 2;
            double centerY = canvas.ActualHeight / 2;

            points[0] = new Point(centerX, centerY);//первая точка идет в центр, остальные размещаются относительно ее
            double initialRadius = 100;

            for (int i = 1; i < size; i++)
            {
                double angle = rand.NextDouble() * 2 * Math.PI;
                double radius = initialRadius + rand.Next(20, 50);
                points[i] = new Point(centerX + radius * Math.Cos(angle),
                                      centerY + radius * Math.Sin(angle));

                // Перемещаем точки дальше, если расстояние не соответствует
                for (int j = 0; j < i; j++)
                {
                    double currentDistance = Distance(points[i], points[j]);
                    while (currentDistance < distanceMatrix[i, j])
                    {
                        radius += 5;
                        points[i] = new Point(centerX + radius * Math.Cos(angle),
                                              centerY + radius * Math.Sin(angle));
                        currentDistance = Distance(points[i], points[j]);
                    }
                }
            }

            fixedCoordinates = points;

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (i != j)
                    {
                        Line line = new Line
                        {
                            X1 = points[i].X,
                            Y1 = points[i].Y,
                            X2 = points[j].X,
                            Y2 = points[j].Y,
                            Stroke = Brushes.Gray,
                            StrokeThickness = 1
                        };
                        canvas.Children.Add(line);
                        allLines.Add(line);
                    }
                }
            }
            
            for (int i = 0; i < size; i++)
            {
                Ellipse ellipse = new Ellipse
                {
                    Width = 20,
                    Height = 20,
                    Fill = Brushes.Blue
                };
                Canvas.SetLeft(ellipse, points[i].X - 10);
                Canvas.SetTop(ellipse, points[i].Y - 10);
                canvas.Children.Add(ellipse);

                TextBlock textBlock = new TextBlock
                {
                    Text = i.ToString(),
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Canvas.SetLeft(textBlock, points[i].X - 5);
                Canvas.SetTop(textBlock, points[i].Y - 10);
                canvas.Children.Add(textBlock);
            }
        }

        private double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        
        private void DisplayGeneration()
        {
            generationCount++;
            LegendTextBlock.Text = $"Generation: {generationCount}\nBest Route Length: {ga.BestRouteLength()}\n{ga.BestRouteToString()}";

            DrawBestRoute(ga.GetBestRoute());
        }

        private void ClearCanvas()
        {
            GraphCanvas.Children.Clear();
        }
        private void ClearHighlight()
        {

            foreach (var line in allLines)
            {
                line.Stroke = Brushes.Gray;
                line.StrokeThickness = 1;
            }
        }
        private void DrawBestRoute(int[] bestRoute)
        {
            ClearHighlight();

            if (bestRoute.Length > 0)
            {
                for (int i = 0; i < bestRoute.Length - 1; i++)
                {
                    int startIndex = bestRoute[i];
                    int endIndex = bestRoute[i + 1];
                    //попытка найти существующую линию, чтобы ее "подсветить" и не создавать новую, иначе изначальный график стирается
                    var lineToHighlight = allLines.FirstOrDefault(line =>
                            line.X1 == fixedCoordinates[startIndex].X && line.Y1 == fixedCoordinates[startIndex].Y &&
                            line.X2 == fixedCoordinates[endIndex].X && line.Y2 == fixedCoordinates[endIndex].Y);

                    if (lineToHighlight != null)
                    {
                        lineToHighlight.Stroke = Brushes.Green;
                        lineToHighlight.StrokeThickness = 2;
                    }
                }
            }
            else
            {
                MessageBox.Show("Best route indices are empty after retrieval.");
            }
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.X)
            {
                cancellationTokenSource?.Cancel();
                MessageBox.Show($"Result:\nAt generation {generationCount}\nBest Route Length: {ga.BestRouteLength()}\n{ga.BestRouteToString()}");
            }
            //else
            //if (e.Key == Key.Space)
            //{
            //    ga.Run();
            //    DisplayGeneration();
            //}
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
                DrawInitialGraph(GraphCanvas, matrix);
                generationCount = 0;
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
                for (int j = i + 1; j < size; j++)
                {
                    if (i == j)
                    {
                        matrix[i, j] = 0;
                    }
                    else
                    {
                        double value = rand.Next(1, 100);
                        matrix[i, j] = value;
                        matrix[j, i] = value;
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

        private void LoadExperimentsList()
        {
            List<Experiment> experiments = experimentManager.LoadExperiments();

            ExperimentsListBox.ItemsSource = experiments.Select(e => e.Name).ToList();
        }

        private void ExperimentsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedExperimentName = (string)ExperimentsListBox.SelectedItem;
            if (!string.IsNullOrEmpty(selectedExperimentName))
            {
                try
                {
                    generationCount = 0;
                    experimentManager.LoadExperimentPopulation(selectedExperimentName, ga);

                    //if (ga.population.Count > 0)
                    //{
                    //    Route firstRoute = ga.population.First();
                    //    MessageBox.Show($"Первый маршрут после загрузки: {string.Join(" -> ", firstRoute.Cities)}");
                    //    MessageBox.Show($"Длина первого маршрута: {firstRoute.Length}");
                    //}
                    DrawInitialGraph(GraphCanvas, ga.distanceMatrix);
                    DisplayGeneration();

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке эксперимента: {ex.Message}");
                }
            }
        }


        public static string ShowInputDialog(string title, string prompt)
        {
            var inputBox = new Window
            {
                Title = title,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var label = new Label { Content = prompt, Margin = new Thickness(10) };
            Grid.SetRow(label, 0);

            var textBox = new TextBox { Margin = new Thickness(10) };
            Grid.SetRow(textBox, 1);

            var button = new Button { Content = "OK", Margin = new Thickness(10) };
            Grid.SetRow(button, 2);
            button.Click += (sender, e) =>
            {
                inputBox.DialogResult = true;
                inputBox.Close();
            };

            grid.Children.Add(label);
            grid.Children.Add(textBox);
            grid.Children.Add(button);

            inputBox.Content = grid;
            inputBox.ShowDialog();

            return textBox.Text;
        }

        private void SaveExperiment_Click(object sender, RoutedEventArgs e)
        {
            string experimentName = ShowInputDialog("Введите название эксперимента", "Название эксперимента:");
            if (!string.IsNullOrEmpty(experimentName))
            {
                experimentManager.SaveExperiment(experimentName, ga.population, ga.distanceMatrix);
                MessageBox.Show("Эксперимент успешно сохранен.");
                LoadExperimentsList();
            }
            else
            {
                MessageBox.Show("Название эксперимента не может быть пустым.");
            }
        }
    }
}