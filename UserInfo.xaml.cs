using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FitsLaw.Class;


namespace FitsLaw
{
    public partial class UserInfo : Page
    {
        DBConnection dbConnection = new DBConnection();
        public UserInfo()
        {
            InitializeComponent();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void NextBtn(object sender, RoutedEventArgs e)
        {
            // Get the values from the form controls
          

            if (string.IsNullOrEmpty(ParticipantName.Text))
            {
                MessageBox.Show("Please enter a participant name.");
                return;
            }

            if (string.IsNullOrEmpty(ParticipantAge.Text))
            {
                MessageBox.Show("Please enter a participant age.");
                return;
            }
            if (LeftHandedRadioButton.IsChecked == null)
            {
                // Show an error message or take other appropriate action
                MessageBox.Show("Please select left or right handedness.");
                return;
            }

            if (YesFPSRadioButton.IsChecked == null)
            {
                // Show an error message or take other appropriate action
                MessageBox.Show("Please select if you play FPS games.");
                return;
            }

            if (YesDPIRadioButton.IsChecked == null)
            {
                // Show an error message or take other appropriate action
                MessageBox.Show("Please select if you have a DPI setting preference.");
                return;
            }

            string fullname = ParticipantName.Text;
            int age = int.Parse(ParticipantAge.Text);
            string handedness = LeftHandedRadioButton.IsChecked == true ? "Left Handed" : "Right Handed";
            bool playsFPS = YesFPSRadioButton.IsChecked == true ? true : false;
            string dpiFeeling = YesDPIRadioButton.IsChecked == true ? "Yes" : "No";

            // Insert the data into the database
            try
            {
                dbConnection.Open();

                string query = "INSERT INTO participants (fullname, age, handedness, plays_fps_pc_games, dpi_feeling) " +
                    "VALUES ('" + fullname + "', '" + age + "', '" + handedness + "', '" + playsFPS + "', '" + dpiFeeling + "')";
                dbConnection.ExecuteNonQuery(query);

                dbConnection.Close();

                Window parentWindow = Window.GetWindow(this);
                parentWindow.Content = new Instructions(fullname);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while submitting the data. Error message: {ex.Message}");
            }
        }


        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

     
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
           
        }

        private void SkipBtn(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.Content = new Instructions("NO NAME ENTERD");
        }
    }
}
