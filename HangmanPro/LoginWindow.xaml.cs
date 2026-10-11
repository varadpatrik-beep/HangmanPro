using System.Windows;
using System.Windows.Controls;

namespace HangmanPro
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            PlayerNameBox.Focus();
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            string playerName = PlayerNameBox.Text.Trim();
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "Vendég";
            }

            int difficultyIndex = DifficultyComboBox.SelectedIndex;
            int[] durations = { 180, 300, 600, 0 };

            // Kategória kiolvasása a legördülőből
            string selectedCategory = ((ComboBoxItem)CategoryComboBox.SelectedItem).Content.ToString();

            // Átadjuk az adatokat a főablaknak
            MainWindow main = new MainWindow(playerName, difficultyIndex, durations[DurationComboBox.SelectedIndex], selectedCategory);
            main.Show();
            this.Close();
        }
    }
}