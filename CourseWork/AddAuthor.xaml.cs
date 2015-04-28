using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CourseWork
{
    /// <summary>
    /// Interaction logic for AddAuthor.xaml
    /// </summary>
    public partial class AddAuthor : Window
    {
        public static List<string> Nationalities = new List<string>(); 
        public AddAuthor()
        {
            InitializeComponent();
            Add.IsEnabled = false;
            Nationalities = Command.ReadData("SELECT DISTINCT Nationality FROM Authors WHERE Nationality IS NOT NULL;", "Nationality");
            AuthorNationality.ItemsSource = Nationalities;
        }

        private static bool IsCorrect(string input, Regex regex)
        {
            var match = regex.Match(input);
            return match.Success;
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Add_OnClick(object sender, RoutedEventArgs e)
        {
            string add;
            if (AuthorDeath.Text == "")
                add = "INSERT INTO Authors(AName, BirthDate, DeathDate, Nationality) VALUES ('" +
                         AuthorName.Text + "', '" + AuthorBirth.Text + "', NULL, '" + AuthorNationality.Text + "');";
            else
                add = "INSERT INTO Authors(AName, BirthDate, DeathDate, Nationality) VALUES ('" +
                         AuthorName.Text + "', '" + AuthorBirth.Text + "', '" + AuthorDeath.Text + "', '" + AuthorNationality.Text + "');";
            Command.ExecuteCommand(add);
            this.Close();
        }

        private void CheckField(TextBox tb, Image i, Regex r)
        {
            Add.IsEnabled = true;
            tb.BorderBrush = Brushes.DodgerBlue;
            i.Visibility = Visibility.Hidden;
            if (AuthorName.Text == "" || AuthorBirth.Text == "")
                Add.IsEnabled = false;
            if (IsCorrect(tb.Text, r) || tb.Text == "" && tb.Name != AuthorName.Name) return;
            tb.BorderBrush = Brushes.Crimson;
            i.Visibility = Visibility.Visible;
        }

        private void AuthorName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckField(AuthorName, ImageName, new Regex(@"(^[\p{L}\s'.-]{5,30}$)"));
        }

        /*private void AuthorNationality_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckField(AuthorNationality, ImageNationality, new Regex(@"(^[\p{L}\s'.-]{5,20}$)"));
        }*/

        private void AuthorNationality_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ImageNationality.Visibility = Visibility.Hidden;
            AuthorNationality.BorderBrush = Brushes.DodgerBlue;
            try
            {
                if (AuthorNationality.SelectedValue.ToString().Length > 20)
                {
                    ImageNationality.Visibility = Visibility.Visible;
                    AuthorNationality.BorderBrush = Brushes.Crimson;
                    Add.IsEnabled = false;
                }
            }
            catch
            {
            }
        }

        private void AuthorBirth_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            Add.IsEnabled = true;
            AuthorBirth.BorderBrush = Brushes.DodgerBlue;
            ImageBirth.Visibility = Visibility.Hidden;
            if (AuthorName.Text != "" && AuthorBirth.Text != "") return;
            Add.IsEnabled = false;
            AuthorBirth.BorderBrush = Brushes.Crimson;
            ImageBirth.Visibility = Visibility.Visible;
        }
    }
}
