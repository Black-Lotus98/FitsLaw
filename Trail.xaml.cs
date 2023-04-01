using FitsLaw.Class;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace FitsLaw
{
    /// <summary>
    /// Interaction logic for Trail.xaml
    /// </summary>
    public partial class Trail : Page
    {
        DBConnection dbConnection = new DBConnection();

        int counter = 0;
        MySqlDataReader reader;
        string query;
        private DateTime startTime;
        private DateTime endTime;
        private Point startMousePosition;
        private Point endMousePosition;
        private List<int> missClickCount = new List<int>();
        private int missClicks;
        private string participantName;
        double distance;
        TimeSpan elapsedTime;
        private int participantId;
        decimal elapsedTimeInSeconds;

        private List<int> trials;
        bool isStarted = false;
        public int MissClicks { get { return missClicks; } set { missClicks = value; } }

        public Trail(string fullname)
        {
            this.participantName = fullname;
            InitializeComponent();
            circleBtn.Visibility = Visibility.Hidden;
            //generateRandomTrail();
            ShuffleTrials(32);
            GetParticipantId(participantName);
            ParticipantName.Text = $"Hello {participantName}, thanks for participating";

        }

        private void CircleBtn(object sender, RoutedEventArgs e)
        {
            circleBtn.Visibility = Visibility.Hidden;
            squareBtn.Visibility = Visibility.Visible;
            endMousePosition = Mouse.GetPosition(this);
            distance = CalculateDistance(startMousePosition, endMousePosition);
            endTime = DateTime.Now;
            elapsedTime = endTime - startTime;
            elapsedTimeInSeconds = Convert.ToDecimal(elapsedTime.TotalSeconds);
            isStarted = false;
            missClickCount.Add(MissClicks);
            int taskId = trials[counter];
            counter++;

            SaveResultToDatabase(taskId, participantId, distance, MissClicks, elapsedTime);
        }

        private void SquarBtn(object sender, RoutedEventArgs e)
        {
            startMousePosition = Mouse.GetPosition(this);
            MissClicks = 0;
            isStarted = true;
            getTrial();
        }

        private void getTrial()
        {
            squareBtn.Visibility = Visibility.Hidden;
            circleBtn.Visibility = Visibility.Visible;
            startTime = DateTime.Now;

            dbConnection.Open();
            if(counter < 32)
            {
                query = "SELECT * FROM tasks Where id =" + "'"+trials[counter]+ "'";
                Console.WriteLine();
                reader = dbConnection.ExecuteQuery(query);
                if (reader.HasRows)
                {
                    if (reader.Read())
                    {
                        int taskId = reader.GetInt32("id");
                        int diameter = reader.GetInt32("diameter");
                        int distance = reader.GetInt32("distance");
                        string direction = reader.GetString("direction");
                        circleBtn.ApplyTemplate();
                        Ellipse ellipse = FindChild<Ellipse>(circleBtn, "ellipse"); // Search for the Ellipse element by name
                        if (ellipse != null)
                        {
                            ellipse.Width = diameter;
                            ellipse.Height = diameter;
                        }
                        GenerateBtn(direction, distance);

                        resultsBox.Text = $"Task {counter}/32";
                    }
                }
            }
            else
            {
                circleBtn.Visibility = Visibility.Hidden;
                squareBtn.Visibility = Visibility.Hidden;
                resultsBox.Text = "You are done, Thanks";
            }

            dbConnection.Close();
        }

        private void GenerateBtn(string direction, int distance)
        {
            if(direction.ToUpper() == "LEFT")
            {
                circleBtn.HorizontalAlignment = HorizontalAlignment.Left;
                circleBtn.Margin = new Thickness(distance, circleBtn.Margin.Top, circleBtn.Margin.Right, circleBtn.Margin.Bottom);

            }
            else
            {
                circleBtn.HorizontalAlignment = HorizontalAlignment.Right;
                circleBtn.Margin = new Thickness(circleBtn.Margin.Left, circleBtn.Margin.Top, distance, circleBtn.Margin.Bottom);
            }

        }



        private T FindChild<T>(DependencyObject parent, string name) where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t && (string)child.GetValue(FrameworkElement.NameProperty) == name)
                {
                    return t;
                }
                T found = FindChild<T>(child, name);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        private void ShuffleTrials(int count)
        {
            // Initialize the list with trial IDs from 1 to count
            trials = new List<int>(count);
            for (int i = 1; i <= count; i++)
            {
                trials.Add(i);
            }

            // Shuffle the trial IDs using Fisher-Yates shuffle algorithm
            Random random = new Random();
            for (int i = count - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                int temp = trials[i];
                trials[i] = trials[swapIndex];
                trials[swapIndex] = temp;
            }
        }

        private double CalculateDistance(Point startPoint, Point endPoint)
        {
            double xDistance = endPoint.X - startPoint.X;
            double yDistance = endPoint.Y - startPoint.Y;

            return Math.Sqrt(Math.Pow(xDistance, 2) + Math.Pow(yDistance, 2));
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseOverElement(squareBtn) && !IsMouseOverElement(circleBtn) && isStarted )
            {
                MissClicks++;
            }
        }
        private bool IsMouseOverElement(UIElement element)
        {
            Point mousePosition = Mouse.GetPosition(element);
            return (mousePosition.X >= 0) && (mousePosition.X < element.RenderSize.Width) &&
                   (mousePosition.Y >= 0) && (mousePosition.Y < element.RenderSize.Height);
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        private void SaveResultToDatabase(int taskId, int participantId, double distance, int missClicks, TimeSpan elapsedTime)
        {
            dbConnection.Open();
            string query = "INSERT INTO Trail (task_id, participant_id, distance, miss_clicks, elapsed_time) VALUES (@taskId, @participantId, @distance, @missClicks, @elapsedTime)";

            using (MySqlCommand cmd = new MySqlCommand(query, dbConnection.Connection))
            {
                cmd.Parameters.AddWithValue("@taskId", taskId);
                cmd.Parameters.AddWithValue("@participantId", participantId);
                cmd.Parameters.AddWithValue("@distance", Math.Round(distance, 2));
                cmd.Parameters.AddWithValue("@missClicks", missClicks);
                cmd.Parameters.AddWithValue("@elapsedTime", Math.Round(elapsedTime.TotalSeconds, 2));

                cmd.ExecuteNonQuery();
            }

            dbConnection.Close();
        }

        private void GetParticipantId(string participantName)
        {
            dbConnection.Open();
            string participantIdQuery = "SELECT id FROM participants WHERE fullname = " + "'" + participantName+"'";
            reader = dbConnection.ExecuteQuery(participantIdQuery);
            if (reader.HasRows)
            {
                if (reader.Read())
                {
                    this.participantId = reader.GetInt32("id");
                }
            }
            dbConnection.Close();
        }
    }
}
