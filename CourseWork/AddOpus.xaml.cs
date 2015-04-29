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
    /// Interaction logic for AddOpus.xaml
    /// </summary>
    public partial class AddOpus : Window
    {
        public static List<string> Authors = Command.ReadData("SELECT DISTINCT AName FROM Authors;", "AName"); 
        public AddOpus()
        {
            InitializeComponent();
            Add.IsEnabled = false;
            ComboBoxAuthors.ItemsSource = Authors;
            ComboBoxAuthors2.ItemsSource = Authors;
            ComboBoxAuthors3.ItemsSource = Authors;
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
            if (OpusYear.Text == "")
                add = "INSERT INTO Opuses(OName, Genre, Year) VALUES ('" +
                         OpusName.Text + "', '" + OpusGenre.Text + "', NULL);";
            else
                add = "INSERT INTO Opuses(OName, Genre, Year) VALUES ('" +
                         OpusName.Text + "', '" + OpusGenre.Text + "', '" + OpusYear.Text + "');";
            Command.ExecuteCommand(add);

            UpdateCreatives(1);
            if(ComboBoxAuthors2.SelectedValue!=null)
                UpdateCreatives(2);
            if(ComboBoxAuthors3.SelectedValue!=null)
                UpdateCreatives(3);

            this.Close();
        }

        private void UpdateCreatives(int i)
        {
            ComboBox cb = new ComboBox();
            switch (i)
            {
                case (1):
                    cb = ComboBoxAuthors;
                    break;
                case (2):
                    cb = ComboBoxAuthors2;
                    break;
                case (3):
                    cb = ComboBoxAuthors3;
                    break;

            }
            string aID =
                   Command.ReadData("SELECT AID FROM Authors WHERE AName='" + cb.SelectedItem + "'",
                       "AID")[0];
            int oID = Command.GetIntValue("SELECT MAX(OID) FROM Opuses");
                /*= Command.ReadData(
                    "SELECT OID FROM Opuses WHERE OName='" + OpusName.Text + "' AND Genre='" + OpusGenre.Text +
                    "' AND Year='" + OpusYear.Text + "';", "OID")[0];*/
            string updateCreatives = "INSERT INTO Creative(AID, ANumber, OID) VALUES (" + aID + ", " + i + ", " + oID + ")";
            Command.ExecuteCommand(updateCreatives);
        }

        private void CheckField(TextBox tb, Image i, Regex r)
        {
            Add.IsEnabled = true;
            tb.BorderBrush = Brushes.DodgerBlue;
            i.Visibility = Visibility.Hidden;
            if (OpusName.Text == "" || ComboBoxAuthors.SelectedValue == null)
                Add.IsEnabled = false;
            if (IsCorrect(tb.Text, r) || tb.Text == "" && tb.Name != OpusName.Name) return;
            tb.BorderBrush = Brushes.Crimson;
            i.Visibility = Visibility.Visible;
        }
        private void OpusName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckField(OpusName, ImageName, new Regex(@"(^[,\d\p{L}\s'.-]{3,30}$)"));
        }

        private void OpusYear_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckField(OpusYear, ImageYear, new Regex(@"[0-9]{4}"));
            if (OpusYear.Text.Length == 4) return;
            OpusYear.BorderBrush = Brushes.Crimson;
            ImageYear.Visibility = Visibility.Visible;
        }

        private void OpusGenre_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckField(OpusGenre, ImageGenre, new Regex(@"[^0-9]{5,50}"));
        }

        private void AddAuthorButton_OnClick(object sender, RoutedEventArgs e)
        {
            new AddAuthor().ShowDialog();
            Authors = Command.ReadData("SELECT DISTINCT AName FROM Authors;", "AName");
            ComboBoxAuthors.ItemsSource = Authors;
            ComboBoxAuthors2.ItemsSource = Authors;
            ComboBoxAuthors3.ItemsSource = Authors;
        }

        private void ComboBoxAuthors_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OpusName.Text != "" || ComboBoxAuthors.SelectedValue != null)
                Add.IsEnabled = true;
            Label2.Foreground = Brushes.Black;
            ComboBoxAuthors2.IsEnabled = true;
        }

        private void ComboBoxAuthors2_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Label3.Foreground = Brushes.Black;
            ComboBoxAuthors3.IsEnabled = true;
        }

        private void RemoveAuthorButton_OnClick(object sender, RoutedEventArgs e)
        {
            ComboBoxAuthors2.SelectedValue = null;
            ComboBoxAuthors3.IsEnabled = false;
            Label3.Foreground = Brushes.DarkGray;
        }

        private void RemoveAuthor3Button_OnClick(object sender, RoutedEventArgs e)
        {
            ComboBoxAuthors3.SelectedValue = null;
        }
    }
}
