using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for AddBook.xaml
    /// </summary>
    public partial class AddBook : Window
    {
        private List<string> PublList = Command.ReadData("SELECT DISTINCT PName FROM Publishers;", "PName"); 
        private List<string> Opuses = Command.ReadData("SELECT DISTINCT OName FROM Opuses;", "OName"); 
        public AddBook()
        {
            InitializeComponent();
            Add.IsEnabled = false;
            ComboBoxPublName.ItemsSource = PublList;
            ComboBoxOpuses.ItemsSource = Opuses;
            ComboBoxOpuses2.ItemsSource = Opuses;
            ComboBoxOpuses3.ItemsSource = Opuses;
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

        private void CheckField(TextBox tb, Image i, Regex r)
        {
            Add.IsEnabled = true;
            tb.BorderBrush = Brushes.DodgerBlue;
            i.Visibility = Visibility.Hidden;
            if (BookName.Text == "" || ComboBoxPublName.SelectedValue == null || BookPages.Text == "" || ComboBoxOpuses.SelectedValue == null)
                Add.IsEnabled = false;
            if (IsCorrect(tb.Text, r) || tb.Text == "") return;
            tb.BorderBrush = Brushes.Crimson;
            i.Visibility = Visibility.Visible;
        }

        private void PublNameComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void AddPublButton_OnClick(object sender, RoutedEventArgs e)
        {
            new AddPublisher().ShowDialog();
            PublList = Command.ReadData("SELECT DISTINCT PName FROM Publishers;", "PName");
            ComboBoxPublName.ItemsSource = PublList;
        }

        private void BookYear_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckField(BookYear, ImageYear, new Regex(@"[0-9]{4}"));
            if (BookYear.Text.Length == 4) return;
            BookYear.BorderBrush = Brushes.Crimson;
            ImageYear.Visibility = Visibility.Visible;
        }

        private void BookCode_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckField(BookCode, ImageCode, new Regex(@"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$"));
        }

        private void BookName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckField(BookName, ImageName, new Regex(@"(^[\p{L}\s'.-]{3,30}$)"));
        }

        private void ComboBoxOpuses_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BookName.Text != "" || ComboBoxOpuses.SelectedValue != null)
                Add.IsEnabled = true;
            Label2.Foreground = Brushes.Black;
            ComboBoxOpuses2.IsEnabled = true;
            RemoveOpusButton2.IsEnabled = true;
        }

        private void AddOpusButton_OnClick(object sender, RoutedEventArgs e)
        {
            new AddOpus().ShowDialog();
            Opuses = Command.ReadData("SELECT DISTINCT OName FROM Opuses;", "OName");
            ComboBoxOpuses.ItemsSource = Opuses;
            ComboBoxOpuses2.ItemsSource = Opuses;
            ComboBoxOpuses3.ItemsSource = Opuses;
        }

        private void ComboBoxOpuses2_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Label3.Foreground = Brushes.Black;
            ComboBoxOpuses3.IsEnabled = true;
            RemoveOpusButton3.IsEnabled = true;
            RemoveOpusButton2.IsEnabled = true;
        }

        private void RemoveOpuseButton_OnClick(object sender, RoutedEventArgs e)
        {
            ComboBoxOpuses2.SelectedValue = null;
            ComboBoxOpuses3.IsEnabled = false;
            Label3.Foreground = Brushes.DarkGray;
            RemoveOpusButton2.IsEnabled = false;
            RemoveOpusButton3.IsEnabled = false;
        }

        private void RemoveOpus3Button_OnClick(object sender, RoutedEventArgs e)
        {
            ComboBoxOpuses3.SelectedValue = null;
            RemoveOpusButton3.IsEnabled = false;
        }

        private void Add_OnClick(object sender, RoutedEventArgs e)
        {
            ImageCode.Visibility = Visibility.Hidden;
            List<string> idsList = Command.ReadData("SELECT Code FROM Books;", "Code");
            if (idsList.Contains(BookCode.Text))
            {
                ImageCode.Visibility = Visibility.Visible;
                return;
            }
            int publId =
                Command.GetIntValue("SELECT PublID FROM Publishers WHERE PName = '" + ComboBoxPublName.SelectedValue + "'");
            string add;
            if (BookYear.Text == "" && BookAnn.Text != "")
                add = "INSERT INTO Books(Code, BName, Pages, Info, PublID) VALUES ('" +
                      BookCode.Text + "', '" + BookName.Text + "', '" + BookPages.Text + "', '" + BookAnn.Text + "' , " +
                      publId + ");";
            else if (BookYear.Text != "" && BookAnn.Text == "")
                add = "INSERT INTO Books(Code, BName, Pages, Year, PublID) VALUES ('" +
                      BookCode.Text + "', '" + BookName.Text + "', '" + BookPages.Text + "', '" + BookYear.Text + "' , " +
                      publId + ");";
            else if (BookYear.Text == "" && BookAnn.Text == "")
                add = "INSERT INTO Books(Code, BName, Pages, PublID) VALUES ('" +
                      BookCode.Text + "', '" + BookName.Text + "', '" + BookPages.Text + "', " + publId + ");";

            else
                add = "INSERT INTO Books(Code, BName, Year, Pages, Info, PublID) VALUES ('" +
                      BookCode.Text + "', '" + BookName.Text + "', '" + BookYear.Text + "', '" + BookPages.Text + "', '" +
                      BookAnn.Text + "', '" + publId + "');";
            Command.ExecuteCommand(add);

            UpdateContent(1);
            if (ComboBoxOpuses2.SelectedValue != null)
                UpdateContent(2);
            if (ComboBoxOpuses3.SelectedValue != null)
                UpdateContent(3);

            this.Close();
        }

        private void UpdateContent(int i)
        {
            ComboBox cb = new ComboBox();
            switch (i)
            {
                case(1):
                    cb = ComboBoxOpuses;
                    break;
                case(2):
                    cb = ComboBoxOpuses2;
                    break;
                case(3):
                    cb = ComboBoxOpuses3;
                    break;

            }

            string oID = Command.ReadData("SELECT OID FROM Opuses WHERE OName='" + cb.SelectedItem + "'",
                       "OID")[0];
            string Code = Command.ReadData("SELECT Code FROM Books WHERE InnerID=(SELECT MAX(InnerID) FROM Books)",
                       "Code")[0];
            string updateContent = "INSERT INTO Content(Code, OpusID) VALUES (" + Code + ", " + oID + ")";
            Command.ExecuteCommand(updateContent);
        }

        private void BookPages_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckField(BookPages, ImagePages, new Regex(@"[0-9]{1,5}"));
        }

        private void BookAnn_OnTextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void ComboBoxOpuses3_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoveOpusButton3.IsEnabled = true;
        }
    }
}
