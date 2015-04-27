using System;
using System.Collections;
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
using System.Net.Mime;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using CourseWork.Styles.CustomizedWindow;
using CheckBox = System.Windows.Controls.CheckBox;
using DataGrid = System.Windows.Controls.DataGrid;
using DataGridCell = System.Windows.Controls.DataGridCell;
using MessageBox = System.Windows.Forms.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

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
        public static List<string> LastGivenBooks = new List<string>();
        public static List<string> BName = new List<string>();
        public  static  List<string> ComboBoxFiltersList = new List<string>(); 
        public  static  List<string> AuthorsFiltersList = new List<string>(); 
        public  static  List<string> OpusesFiltersList = new List<string>(); 

        public MainWindow()
        {
            InitializeComponent();
            ComboBoxList.AddRange(Tables.Keys);
            ComboBoxTables.ItemsSource = ComboBoxList;
            ComboBoxTables.SelectedIndex = 1;
            ListBoxLastGiven.ItemsSource = LastGivenBooks; 
            
            ComboBoxFiltersList =
                 Command.ReadData("SELECT DISTINCT Year, PName FROM Books B, Publishers P WHERE B.PublID=P.PublID",
                     "Year");
            ComboBoxFiltersList.AddRange(Command.ReadData("SELECT DISTINCT Year, PName FROM Books B, Publishers P WHERE B.PublID=P.PublID",
                     "PName"));
            ComboBoxFilters.ItemsSource = ComboBoxFiltersList;
            AuthorsFiltersList = Command.ReadData("SELECT DISTINCT Nationality FROM Authors",
                     "Nationality");
            OpusesFiltersList = Command.ReadData("SELECT DISTINCT Genre FROM Opuses",
                        "Genre");
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
            string tableName = ComboBoxTables.SelectedValue.ToString();
            LabelFilter.Foreground = Brushes.DarkSlateGray;
            ComboBoxFilters.IsEnabled = true;
            if (TextBoxSearch.Text != "Введите поисковый запрос...")
                TextBoxSearch.Text = "";
            if (Tables.Keys.Contains(ComboBoxTables.SelectedValue.ToString()))
                FillDataGrid("SELECT * FROM " + Tables[ComboBoxTables.SelectedValue.ToString()], EditDataGrid);
            if (tableName == "Авторы")
            {
                ComboBoxFilters.ItemsSource = AuthorsFiltersList;
            }

            if (tableName == "Произведения")
            {
                ComboBoxFilters.ItemsSource = OpusesFiltersList;
            } 
            
            if (tableName == "Книги")
            {
                ComboBoxFilters.ItemsSource = ComboBoxFiltersList;
            }

            else if (tableName != "Авторы" && tableName != "Произведения" && tableName != "Книги")
            {
                LabelFilter.Foreground = Brushes.LightGray;
                ComboBoxFilters.IsEnabled = false;
                ComboBoxFilters.ItemsSource = new List<string>();
            }
        }

        private void EditDataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            TextBlockResult.Text = "";
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

                List<string> idValues = Command.ReadData("SELECT " + Ids[tableName] + " FROM " + tableName,
                    Ids[tableName]);
                if (value.Equals("") || !idValues.Contains(idValue)) return;
                {
                    var query = "";
                    if (value == "")
                        query = "UPDATE " + tableName + " SET " + propertyName + "=NULL WHERE " +
                            Ids[tableName] + "='" + idValue + "'"; 
                    query = "UPDATE " + tableName + " SET " + propertyName + "='" + value + "' WHERE " +
                             Ids[tableName] + "='" + idValue + "'";
                    /*if (!Command.ExecuteCommand(query) && !dgDataGrid.SelectedCells.ToString().Contains(value))
                    {
                        MessageBox.Show("Поля с таким значением не существует. Данные не будут сохранены.", "Ошибка",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }*/
                    
                    if (!Command.ExecuteCommand(query))
                    {
                        TextBlockResult.Foreground = Brushes.Crimson;
                        TextBlockResult.Text = "Неверные данные: изменения не будут сохранены.";
                    }
                    else
                    {
                        Command.ExecuteCommand(query);
                        TextBlockResult.Foreground = Brushes.LimeGreen;
                        TextBlockResult.Text = "Изменения сохранены.";
                    }
                }
            }

            catch (Exception exc)
            {
                //MessageBox.Show(exc.Message);
            }
        }

        private void EditDataGrid_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)
              //  EditDataGrid_CurrentCellChanged(sender, e);
        }

        public DataGridCell GetDataGridCell(DataGridCellInfo cellInfo)
        {
            var cellContent = cellInfo.Column.GetCellContent(cellInfo.Item);
            if (cellContent != null)
                return (DataGridCell)cellContent.Parent;

            return null;
        }

        private void ButtonSearch_OnClick(object sender, RoutedEventArgs e)
        {
            if (ComboBoxFilters.SelectedValue != null)
                ComboBoxFilters.SelectedValue = "";
            if (ComboBoxTables.SelectedIndex < 0)
            {
                ComboBoxTables.Background = Brushes.Crimson;
                return;
            }

            string searchCriteria = TextBoxSearch.Text;
            string tableName = Tables[ComboBoxTables.SelectedValue.ToString()];

            if (searchCriteria == "" || searchCriteria == "Введите поисковый запрос...")
            {
                FillDataGrid("SELECT * FROM " + tableName, EditDataGrid);
                TextBoxSearch.BorderBrush = Brushes.Crimson;
                TextBoxSearch.Foreground = Brushes.Crimson;
                TextBoxSearch.Text = "";
                return;
            }

            TextBoxSearch.BorderBrush = Brushes.DimGray;
            TextBoxSearch.Foreground = Brushes.DimGray;
            ComboBoxTables.BorderBrush = Brushes.LightGray;

            string getNames = "SELECT COLUMN_NAME from CityLibrary.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "'";
            List<string> properties = Command.ReadData(getNames, "COLUMN_NAME");

            try
            {
                var query = "SELECT * FROM " + tableName + " WHERE " + properties[0] + " LIKE '%" + searchCriteria + "%'";
                    for (var j = 1; j < properties.Count; j++)
                        query += " UNION " + "SELECT * FROM " + tableName + " WHERE " + properties[j] + " LIKE '%" + searchCriteria + "%'";
                FillDataGrid(query, EditDataGrid);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (TextBoxSearch.Text != "Введите поисковый запрос...")
                TextBoxSearch.Text = "";
            if (TextBoxSearch.Text == "")
                TextBoxSearch.Text = "Введите поисковый запрос...";
        }

        private void TextBoxSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBoxSearch.Text = "";
        }

        private void TextBoxSearch_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ButtonSearch_OnClick(sender, new RoutedEventArgs());
        }

        private void EditDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid dg = this.EditDataGrid;
            var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
            while (cell != null && !(cell is System.Windows.Controls.DataGridCell)) cell = VisualTreeHelper.GetParent(cell);
            System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult result = MessageBox.Show("Вы уверены, что хотите удалить данную запись?", "Подтверждение удaления",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                TextBlockResult.Text = "";
                DataGrid dgDataGrid = this.EditDataGrid;
                string idValue = "";
                foreach (DataGridCellInfo di in dgDataGrid.SelectedCells)
                {
                    DataRowView dvr = (DataRowView) di.Item;
                    idValue = dvr[0].ToString();
                }

                var tableName = Tables[ComboBoxTables.SelectedValue.ToString()];

                // books refrs publ
                //var q = "ALTER TABLE Books ADD CONSTRAINT fk_employee FOREIGN KEY (PName) REFERENCES Publishers (PName) ON DELETE CASCADE;";
                //Command.ExecuteCommand(q);

                var query = "DELETE FROM " + tableName + " WHERE " + Ids[tableName] + "=" + idValue;
                Command.ExecuteCommand(query);
                FillDataGrid("SELECT * FROM " + tableName, EditDataGrid);
                TextBlockResult.Text = "Строка удалена.";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void ComboBoxFilters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxFilters.SelectedValue == null) return;
            string searchCriteria = ComboBoxFilters.SelectedValue.ToString();
            string tableName = Tables[ComboBoxTables.SelectedValue.ToString()];
            string getNames = "SELECT COLUMN_NAME from CityLibrary.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "'";
            List<string> properties = Command.ReadData(getNames, "COLUMN_NAME");

            var query = "SELECT * FROM " + Tables[ComboBoxTables.SelectedValue.ToString()] + " WHERE " + properties[0] + " LIKE '%" + searchCriteria + "%'";
            for (var j = 1; j < properties.Count; j++)
                query += " UNION " + "SELECT * FROM " + tableName + " WHERE " + properties[j] + " LIKE '%" + searchCriteria + "%'";
            if (ComboBoxTables.SelectedValue.ToString() == "Книги")
            {
                query += " UNION " + "SELECT Code, BName, Year, Pages, Info, P.PublID, InnerID FROM Books b INNER JOIN Publishers p ON b.PublID=p.PublID WHERE PName LIKE '%" + searchCriteria + "%'";
            }
            FillDataGrid(query, EditDataGrid);
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            var tableName = ComboBoxTables.SelectedValue.ToString();
            var selected = ComboBoxTables.SelectedValue.ToString();
            if (selected == "Читатели")
                new AddClient().ShowDialog();
            if (selected == "Авторы")
                new AddAuthor().ShowDialog();
            if (selected == "Издательства")
                new AddPublisher().ShowDialog();
            if (selected == "Произведения")
                new AddOpus().ShowDialog();
            if (selected == "Книги")
                new AddBook().ShowDialog();
            FillDataGrid("SELECT * FROM " + Tables[ComboBoxTables.SelectedValue.ToString()], EditDataGrid);
            ComboBoxFiltersList =
                 Command.ReadData("SELECT DISTINCT Year, PName FROM Books B, Publishers P WHERE B.PublID=P.PublID",
                     "Year");
            ComboBoxFiltersList.AddRange(Command.ReadData("SELECT DISTINCT Year, PName FROM Books B, Publishers P WHERE B.PublID=P.PublID",
                     "PName"));
            ComboBoxFilters.ItemsSource = ComboBoxFiltersList;
            AuthorsFiltersList = Command.ReadData("SELECT DISTINCT Nationality FROM Authors",
                     "Nationality");
            OpusesFiltersList = Command.ReadData("SELECT DISTINCT Genre FROM Opuses",
                        "Genre"); if (tableName == "Авторы")
            {
                ComboBoxFilters.ItemsSource = AuthorsFiltersList;
            }

            if (tableName == "Произведения")
            {
                ComboBoxFilters.ItemsSource = OpusesFiltersList;
            }

            if (tableName == "Книги")
            {
                ComboBoxFilters.ItemsSource = ComboBoxFiltersList;
            }

            else if (tableName != "Авторы" && tableName != "Произведения" && tableName != "Книги")
            {
                LabelFilter.Foreground = Brushes.LightGray;
                ComboBoxFilters.IsEnabled = false;
                ComboBoxFilters.ItemsSource = new List<string>();
            }
        }

        private void EditDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            TextBlockResult.Text = "";
            try
            {

                 var element = e.EditingElement as TextBox;
                var value = element.Text;
                var dgDataGrid = EditDataGrid;//sender as DataGrid;

                int indexOfSelectedColumn = dgDataGrid.CurrentColumn.DisplayIndex;
                string propertyName = e.Column.Header.ToString();

                var tableName = Tables[ComboBoxTables.SelectedValue.ToString()];
                string oldValue = "";

                var idValue = "";


                foreach (DataGridCellInfo di in dgDataGrid.SelectedCells)
                {
                    DataRowView dvr = (DataRowView)di.Item;
                    idValue = dvr[0].ToString();
                    oldValue = dvr[indexOfSelectedColumn].ToString();
                }

                List<string> idValues = Command.ReadData("SELECT " + Ids[tableName] + " FROM " + tableName,
                    Ids[tableName]);
                if (!idValues.Contains(idValue) && oldValue.ToString().Equals(value.ToString())) return;
                {
                    var query = "";
                    if (value == "")
                        query = "UPDATE " + tableName + " SET " + propertyName + "=NULL WHERE " +
                            Ids[tableName] + "='" + idValue + "'";
                    else{
                        query = "UPDATE " + tableName + " SET " + propertyName + "='" + value + "' WHERE " +
                             Ids[tableName] + "='" + idValue + "'";}

                    if (!Command.ExecuteCommand(query))
                    {
                        TextBlockResult.Foreground = Brushes.Crimson;
                        TextBlockResult.Text = "Неверные данные: изменения не будут сохранены.";
                    }
                    else
                    {
                        Command.ExecuteCommand(query);
                        TextBlockResult.Foreground = Brushes.LimeGreen;
                        TextBlockResult.Text = "Изменения сохранены.";
                    }
                }
            }

            catch (Exception exc)
            {
                //MessageBox.Show(exc.Message);
            }
        }
    }
}
