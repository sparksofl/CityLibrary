using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using MenuItem = System.Windows.Controls.MenuItem;
using System.Collections.ObjectModel;
using MessageBox = System.Windows.Forms.MessageBox;

namespace CourseWork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static Dictionary<string, string> Tables = new Dictionary<string, string>()
        {
            { "Авторы", "Authors"},
            { "Книги", "Books"},
            { "Выдачи", "Checkout"},
            { "Читатели", "Clients"},
            { "Содержания", "Content"},
            { "Авторства", "Creative"},
            { "Произведения", "Opuses"},
            { "Издательства", "Publishers"}
        };

        static Dictionary<string, string> Ids = new Dictionary<string, string>()
        {
            { "Authors", "AID"},
            { "Books", "Code"},
            { "Checkout", "ChID"},
            { "Clients", "CardNumber"},
            { "Content", "ContID"},
            { "Creative", "CrID"},
            { "Opuses", "OID"},
            { "Publishers", "PName"}
        };

        public static List<string> ComboBoxList = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            ComboBoxList.AddRange(Tables.Keys);
            ComboBoxTables.ItemsSource = ComboBoxList;
        }

        private void DoQuery(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorTextBlock.Text = "";
                ErrorTextBlock.ToolTip = "";

                // creating connection
                SqlConnection connection = Connection.CreateConnection();

                // query string from richTextBox
                string query = new TextRange(RichTextBox.Document.ContentStart, RichTextBox.Document.ContentEnd).Text;

                // creating command to perform
                SqlCommand command = new SqlCommand(query, connection);
                //command.ExecuteNonQuery();

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable("CityLibrary");

                // dunno for what it
                adapter.Fill(dataTable);
                QueryDataGrid.ItemsSource = dataTable.DefaultView;
                adapter.Update(dataTable);

                // it's necessary to always do it after proccessing DB.
                connection.Close();
                Connection.IsOpened = false;
            }

            catch (Exception ex)
            {
                ErrorTextBlock.Text = ex.Message;
                ErrorTextBlock.ToolTip = ex.Message;
            }
        }

        private void FillDataGrid(string query, DataGrid dataGrid)
        {
            try
            {
                //creating connection
                SqlConnection connection = Connection.CreateConnection();

                // creating command to perform
                SqlCommand command = new SqlCommand(query, connection);
                //command.ExecuteNonQuery();

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable("CityLibrary");

                // dunno for what it
                adapter.Fill(dataTable);
                dataGrid.ItemsSource = dataTable.DefaultView;
                adapter.Update(dataTable);

                // it's necessary to always do it after proccessing DB.
                connection.Close();
                Connection.IsOpened = false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void MenuClick(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            try
            {
                var currentItem = new TabItem();

                switch (menuItem.Name)
                {
                    case ("MLastActions"):
                        currentItem = TLastActions;
                        break;
                    case ("MGiveBook"):
                        currentItem = TGiveBook;
                        break;
                    case ("MTakeBackBook"):
                        currentItem = TTakeBackBook;
                        break;
                    case ("MEditing"):
                        currentItem = TEditing;
                        break;
                    case ("MQuery"):
                        currentItem = TQuery;
                        break;
                    case ("MStatistic"):
                        currentItem = TStatistic;
                        break;
                    case ("MReports"):
                        currentItem = TReports;
                        break;
                    case ("Exit"):
                        (new ExitWindow()).ShowDialog();
                        break;
                }
                currentItem.IsSelected = true;
                CurrentMenu.Text = currentItem.Header.ToString();
            }
            catch
            {
            }
        }

        private void Window_Closing(object sender, CancelEventArgs cancelEventArgs)
        {
            if (Connection.SqlConnection != null && Connection.SqlConnection.State == ConnectionState.Open)
                Connection.SqlConnection.Close();

            cancelEventArgs.Cancel = true;
            (new ExitWindow()).ShowDialog();
        }

        private void EditDataGrid_OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var displayName = GetPropertyDisplayName(e.PropertyDescriptor);

            if (!string.IsNullOrEmpty(displayName))
            {
                e.Column.Header = displayName;
            }
        }

        public static string GetPropertyDisplayName(object descriptor)
        {
            var pd = descriptor as PropertyDescriptor;

            if (pd != null)
            {
                // Check for DisplayName attribute and set the column header accordingly
                var displayName = pd.Attributes[typeof (DisplayNameAttribute)] as DisplayNameAttribute;

                if (displayName != null && !displayName.Equals(DisplayNameAttribute.Default))
                {
                    return displayName.DisplayName;
                }

            }
            else
            {
                var pi = descriptor as PropertyInfo;

                if (pi == null) return null;
                // Check for DisplayName attribute and set the column header accordingly
                var attributes = pi.GetCustomAttributes(typeof (DisplayNameAttribute), true);
                return (from t in attributes
                    select t as DisplayNameAttribute
                    into displayName
                    where displayName != null && displayName.Equals(DisplayNameAttribute.Default)
                    select displayName.DisplayName).FirstOrDefault();
            }

            return null;
        }

        private void ComboBoxTables_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Tables.Keys.Contains(ComboBoxTables.SelectedValue.ToString()))
                FillDataGrid("SELECT * FROM " + Tables[ComboBoxTables.SelectedValue.ToString()], EditDataGrid);
        }

        private static string query;


        private void EditDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
        }

        private void EditDataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            try
            {
                var dgDataGrid = sender as DataGrid;

                int indexOfSelectedColumn = dgDataGrid.CurrentColumn.DisplayIndex;

                string propertyName = this.EditDataGrid.CurrentColumn.Header.ToString();

                var tableName = Tables[ComboBoxTables.SelectedValue.ToString()];
                string value = "";

                var idValue = "";


                foreach (DataGridCellInfo di in dgDataGrid.SelectedCells)
                {
                    DataRowView dvr = (DataRowView) di.Item;
                    idValue = dvr[0].ToString();
                    value = dvr[indexOfSelectedColumn].ToString();
                }
                if (value.Equals("")) return;
                {
                    query = "UPDATE " + tableName + " SET " + propertyName + "='" + value + "' WHERE " +
                            Ids[tableName] + "='" + idValue + "'";
                    Command.ExecuteCommand(query);
                }
            }

            catch (Exception exc)
            {
                //MessageBox.Show(exc.Message);
            }
        }

    }
}
