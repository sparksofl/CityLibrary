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
    /// Interaction logic for AddClient.xaml
    /// </summary>
    public partial class AddClient : Window
    {
        public AddClient()
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
            string add = "INSERT INTO Clients(Name, ClientAddress, PhoneNumber) VALUES ('" +
                         ClientName.Text + "', '" + ClientAddress.Text + "', '" + ClientPhone.Text + "');";
            Command.ExecuteCommand(add);
            this.Close();
        }

        private void ClientName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckField(ClientName, ImageName, new Regex(@"(^[\p{L}\s'.-]{5,30}$)"));
        }

        private void CheckField(TextBox tb, Image i, Regex r)
        {
            Add.IsEnabled = true;
            tb.BorderBrush = Brushes.DodgerBlue;
            i.Visibility = Visibility.Hidden;
            if (ClientName.Text=="")
                Add.IsEnabled = false;
            if (IsCorrect(tb.Text, r) || tb.Text == "" && tb.Name != ClientName.Name) return;
            tb.BorderBrush = Brushes.Crimson;
            i.Visibility = Visibility.Visible;
        }

        private void ClientAddress_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckField(ClientAddress, ImageAddress, new Regex(@"[^0-9]{5,50}"));
        }

        private void ClientPhone_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckField(ClientPhone, ImagePhone, new Regex(@"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$"));
        }
    }
}
