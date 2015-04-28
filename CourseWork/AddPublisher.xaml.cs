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
    /// Interaction logic for AddPublisher.xaml
    /// </summary>
    public partial class AddPublisher : Window
    {
        public AddPublisher()
        {
            InitializeComponent();
            Add.IsEnabled = false;
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
            string add = "INSERT INTO Publishers(PName, PAddress, PNumber) VALUES ('" +
                         PublName.Text + "', '" + PublAddress.Text + "', '" + PublPhone.Text + "');";
            Command.ExecuteCommand(add);
            this.Close();
        }
        private void CheckField(TextBox tb, Image i, Regex r)
        {
            Add.IsEnabled = true;
            tb.BorderBrush = Brushes.DodgerBlue;
            i.Visibility = Visibility.Hidden;
            if (PublName.Text == "")
                Add.IsEnabled = false;
            if (IsCorrect(tb.Text, r) || tb.Text == "" && tb.Name != PublName.Name) return;
            tb.BorderBrush = Brushes.Crimson;
            i.Visibility = Visibility.Visible;
        }

        private void PublName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckField(PublName, ImageName, new Regex(@"(^[\p{L}\s'.-]{5,30}$)"));
        }

        private void PublAddress_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckField(PublAddress, ImageAddress, new Regex(@"[^0-9]{5,50}"));
        }

        private void PublPhone_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckField(PublPhone, ImagePhone, new Regex(@"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$"));
        }
    }
}
