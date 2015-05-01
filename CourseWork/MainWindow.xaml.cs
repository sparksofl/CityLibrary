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
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using CourseWork.Styles.CustomizedWindow;
using CheckBox = System.Windows.Controls.CheckBox;
using DataGrid = System.Windows.Controls.DataGrid;
using DataGridCell = System.Windows.Controls.DataGridCell;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.Forms.MessageBox;
using TabControl = System.Windows.Controls.TabControl;
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

        public  static  List<string> ComboBoxEntity = new List<string>(){"Книги", "Произведения"};
        public static List<string> ComboBoxParameter = new List<string>() { "Название", "Автор" };
        public static List<string> ClientsList = Command.ReadData("SELECT Name FROM Clients;", new List<string>() {"Name"});

        public static List<string> ComboBoxList = new List<string>();
        public static List<string> LastGivenBooks = Command.ReadDataToItemsList("SELECT TOP 5 BName, GiveDate, Name FROM Books B, Checkout C, Clients Cl WHERE B.Code=C.Code AND Cl.CardNumber=C.CardNumber AND BackDate IS NULL ORDER BY GiveDate DESC", new List<string>() { "BName", "GiveDate", "Name" });
        public List<string> LastBackBooks = Command.ReadDataToItemsList("SELECT TOP 5 BName, BackDate, Name FROM Books B, Checkout C, Clients CL WHERE B.Code=C.Code AND Cl.CardNumber=C.CardNumber AND BackDate IS NOT NULL ORDER BY BackDate DESC", new List<string>() { "BName", "BackDate", "Name" });
        public List<string> OverdueBooks = Command.ReadDataToItemsList("SELECT TOP 5 BName, Term, Name FROM Overdue O, Clients C WHERE o.CardNumber=C.CardNumber ORDER BY Term DESC", new List<string>() { "BName", "Term", "Name" });
        public static List<string> BName = new List<string>();
        public  static  List<string> ComboBoxFiltersList = new List<string>(); 
        public  static  List<string> AuthorsFiltersList = new List<string>(); 
        public  static  List<string> OpusesFiltersList = new List<string>();

        public static List<KeyValuePair<string, int>> Years;// =
           // Command.ReadDataToPair("SELECT Year, COUNT(Year) FROM Books GROUP BY Year");

        public static List<KeyValuePair<string, int>> Genres;// =
          //  Command.ReadDataToPair("SELECT Genre, COUNT(Genre) FROM Opuses GROUP BY Genre");

        public List<KeyValuePair<string, int>> Gives;// =
            //Command.ReadDataToPair("SELECT CONVERT(VARCHAR(10), GiveDate, 101), COUNT(*) FROM Checkout GROUP BY CONVERT(VARCHAR(10), GiveDate, 101)");
        public List<KeyValuePair<string, int>> Backs;// =
            //Command.ReadDataToPair("SELECT CONVERT(VARCHAR(10), BackDate, 101), COUNT(*) FROM Checkout WHERE BackDate IS NOT NULL GROUP BY CONVERT(VARCHAR(10), BackDate, 101)");
        public List<List<KeyValuePair<string, int>>> Checkout = new List<List<KeyValuePair<string, int>>>();

        public MainWindow()
        {
            InitializeComponent();
            SetSources();
        }

        private void SetSources()
        {
            // DO NOT DELETE — it's important to see how this view was created
            //Command.ExecuteCommand(
            //    "CREATE VIEW Overdue AS SELECT B.Code, BName, ChID, DATEADD(month,1,GiveDate) AS 'Term', C.CardNumber FROM Books B, Checkout C WHERE B.Code=C.Code AND BackDate IS NULL GROUP BY GiveDate, BName, B.Code, ChID HAVING (CAST(DATEADD(month,1,GiveDate) AS DATETIME)) < GETDATE();");
            //Command.ExecuteCommand("DROP VIEW Overdue;");ComboBoxList.AddRange(Tables.Keys);
            ComboBoxList.AddRange(Tables.Keys);
            ComboBoxTables.ItemsSource = ComboBoxList;
            ComboBoxTables.SelectedIndex = 1;
            ListBoxLastGiven.ItemsSource = LastGivenBooks;
            ListBoxLastBack.ItemsSource = LastBackBooks;
            ListBoxOverdue.ItemsSource = OverdueBooks;
            ComboBoxEnt.ItemsSource = ComboBoxEntity;
            ComboBoxParam.ItemsSource = ComboBoxParameter;
            ComboBoxEntBack.ItemsSource = ComboBoxEntity;
            ComboBoxParamBack.ItemsSource = ComboBoxParameter;
            ComboBoxClients.ItemsSource = ClientsList;

            ComboBoxFiltersList = new List<string>() { "1850-1900", "1900-1950", "1950-1970", "1970-1980", "1980-1990", "1990-2000", "2000-2010", "2010-2020" };
            ComboBoxFilters.ItemsSource = ComboBoxFiltersList;
            AuthorsFiltersList = new List<string>();
            AuthorsFiltersList = Command.ReadData("SELECT DISTINCT Nationality FROM Authors",
                      "Nationality");
            OpusesFiltersList = Command.ReadData("SELECT DISTINCT Genre FROM Opuses",
                        "Genre");
            //LastGivenBooks = Command.ReadData("SELECT TOP 7 BName, GiveDate FROM Books B, Checkout C WHERE B.Code=C.Code AND BackDate IS NULL ORDER BY GiveDate DESC", "BName", "GiveDate");
            FillStartGrids();
            //UpdateLists();

            ShowChart();
        }

        private void FillStartGrids()
        {
            FillDataGrid(
                        "SELECT B.Code, BName, OName, AName FROM Books B, Content C, Opuses O, Creative Cr, Authors A, Checkout Ch WHERE B.Code=C.Code AND O.OID=C.OpusID AND O.OID=Cr.OID AND Cr.AID=A.AID AND " +
                        "Ch.Code=B.Code AND (B.Code NOT IN (SELECT Code FROM Checkout WHERE BackDate IS NULL)) " +
                        "UNION SELECT B.Code, BName, OName, AName FROM Books B, Content C, Opuses O, Creative Cr, Authors A WHERE B.Code=C.Code AND O.OID=C.OpusID AND O.OID=Cr.OID AND Cr.AID=A.AID AND B.Code NOT IN (SELECT Code FROM Checkout)",
                        GiveDataGrid);
            FillDataGrid("SELECT Ch.ChID, B.Code, BName, OName, AName,  GiveDate, Ch.CardNumber, Cl.Name " +
                                     "FROM Books B, Content C, Opuses O, Creative Cr, Authors A, Checkout Ch, CLients Cl " +
                                     "WHERE B.Code=C.Code AND O.OID=C.OpusID AND O.OID=Cr.OID AND Cr.AID=A.AID AND Ch.CardNumber=Cl.CardNumber AND Ch.Code=B.Code AND B.Code IN (SELECT Code FROM Checkout) AND (BackDate IS NULL)",//" OR BackDate < (SELECT Term FROM Overdue));";
                    BackDataGrid);
            
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

                var stringForCheck = query;
                stringForCheck = stringForCheck.ToUpper();
                if (stringForCheck.Contains("DROP") || stringForCheck.Contains("INSERT") || stringForCheck.Contains("UPDATE") || stringForCheck.Contains("DELETE")) 
                {
                    var errorMessage = "YOU'VE GOT NO POWER HERE";
                    ErrorTextBlock.Text = errorMessage;
                    ErrorTextBlock.ToolTip = errorMessage;
                    return;
                }

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

        public static void FillDataGrid(string query, DataGrid dataGrid)
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
                        ShowChart();
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
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void ShowChart()
        {
            /*CREATE VIEW Backs AS SELECT 'backdate' = 
            CASE 
            WHEN CONVERT(VARCHAR(10), BackDate, 101) IS NOT NULL 
            THEN CONVERT(VARCHAR(10), BackDate, 101)
            ELSE CONVERT(VARCHAR(10), GiveDate, 101) 
            END,
            'c' = 
            CASE
            WHEN CONVERT(VARCHAR(10), BackDate, 101) IS NOT NULL 
            THEN COUNT(*) 
            ELSE 0
            END
            FROM Checkout
            GROUP BY CONVERT(VARCHAR(10), BackDate, 101), CONVERT(VARCHAR(10), GiveDate, 101)*/
            Gives = new List<KeyValuePair<string, int>>();
            Backs = new List<KeyValuePair<string, int>>();
            Years = new List<KeyValuePair<string, int>>();
            Genres = new List<KeyValuePair<string, int>>();
            Checkout = new List<List<KeyValuePair<string, int>>>(); 

            Years = Command.ReadDataToPair("SELECT Year, COUNT(Year) FROM Books GROUP BY Year");
            Genres = Command.ReadDataToPair("SELECT Genre, COUNT(Genre) FROM Opuses WHERE Genre IS NOT NULL GROUP BY Genre");
            Gives = Command.ReadDataToPair("SELECT CONVERT(VARCHAR(10), GiveDate, 101), COUNT(*) FROM Checkout GROUP BY CONVERT(VARCHAR(10), GiveDate, 101)");
            Backs = Command.ReadDataToPair("SELECT CONVERT(VARCHAR(10), BackDate, 101), COUNT(*) FROM Checkout WHERE BackDate IS NOT NULL GROUP BY CONVERT(VARCHAR(10), BackDate, 101)");
            //DATENAME(weekday, BackDate)
            List<string> gDates = Gives.Select(pair => pair.Key).ToList();
            List<string> bDates = Backs.Select(pair => pair.Key).ToList();
            List<string> Dates = new List<string>();
            Dates.AddRange(gDates);
            Dates.AddRange(bDates);
            Dates = Dates.Distinct().ToList();
            Dates.Sort();

            // отладить и посмотреть индексы элементов для вставки
            int h = 0;
            foreach (var item in Dates)
            {
                if (!gDates.Contains(item))
                {
                    Gives.Add(new KeyValuePair<string, int>(item, 0));
                    Gives.Insert(h, new KeyValuePair<string, int>(item, 0));
                }
                h++;
            }

            h = 0;
            foreach (var item in Dates)
            {
                if (!bDates.Contains(item))
                {
                    Backs.Add(new KeyValuePair<string, int>(item, 0));
                    Backs.Insert(h, new KeyValuePair<string, int>(item, 0));
                }
                h++;
                //Backs.Add(new KeyValuePair<string, int>(Dates[i], 0));
            }

            //Gives.Sort((x, y) => x.Value.CompareTo(y.Value));
            //Backs.Sort((x, y) => x.Value.CompareTo(y.Value));

            Checkout.Add(Gives);
            Checkout.Add(Backs);

            PieChartYears.DataContext = Years;
            PieChartGenres.DataContext = Genres;
            LineChart.DataContext = Checkout;
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
            ButtonAdd.Visibility= Visibility.Visible;
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

            if (tableName == "Выдачи" || tableName == "Содержания" || tableName == "Авторства")
                ButtonAdd.Visibility = Visibility.Collapsed;
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

                UpdateFilters(tableName);
                UpdateLists();
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
            string query = "";
            if (tableName == "Books")
            {
                int a = Convert.ToInt32(searchCriteria.Split('-')[0]);
                int b = Convert.ToInt32(searchCriteria.Split('-')[1]);
                query = "SELECT * FROM Books WHERE Year BETWEEN " + a +
                        " AND " + b;
            }

            else
            {
                query = "SELECT * FROM " + tableName + " WHERE " + properties[0] + " LIKE '%" + searchCriteria + "%'";
                for (var j = 1; j < properties.Count; j++)
                    query += " UNION " + "SELECT * FROM " + tableName + " WHERE " + properties[j] + " LIKE '%" +
                             searchCriteria + "%'";
                if (ComboBoxTables.SelectedValue.ToString() == "Книги")
                {
                    query += " UNION " +
                             "SELECT Code, BName, Year, Pages, Info, P.PublID, InnerID FROM Books b INNER JOIN Publishers p ON b.PublID=p.PublID WHERE PName LIKE '%" +
                             searchCriteria + "%'";
                }
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

            UpdateClients();
            UpdateFilters(tableName);
            UpdateLists();
            FillStartGrids();
            ShowChart();
        }

        private void UpdateClients()
        {
            ClientsList = Command.ReadData("SELECT Name FROM Clients;", "Name");
            ComboBoxClients.ItemsSource = ClientsList;
        }

        private void UpdateFilters(string tableName)
        {
            ComboBoxFiltersList = new List<string>(){"1850-1900", "1900-1950", "1950-1970", "1970-1980", "1980-1990", "1990-2000", "2000-2010", "2010-2020"};
            ComboBoxFilters.ItemsSource = ComboBoxFiltersList;
            AuthorsFiltersList = Command.ReadData("SELECT DISTINCT Nationality FROM Authors",
                "Nationality");
            OpusesFiltersList = Command.ReadData("SELECT DISTINCT Genre FROM Opuses",
                "Genre");
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
                        UpdateFilters(tableName);
                    }
                }
                UpdateFilters(tableName);
                UpdateLists();
            }

            catch (Exception exc)
            {
                //MessageBox.Show(exc.Message);
            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                FillDataGrid("SELECT BName, OName, AName FROM Books B, Content C, Opuses O, Creative Cr, Authors A WHERE B.Code=C.Code AND C.OpusID=O.OID AND O.OID=Cr.OID AND Cr.AID=A.AID;", GiveDataGrid);
            }
        }

        private void SearchGive(Key e)
        {
            var entity = "";
            var param = "";
            string property;// = "AName";

            TextBoxSearchGive.BorderBrush = Brushes.DarkSlateGray;

            if (e == Key.Enter)
            {
                if (ComboBoxEnt.SelectedValue == null || ComboBoxParam.SelectedValue == null)
                {
                    FillDataGrid(
                        "SELECT B.Code, BName, OName, AName FROM Books B, Content C, Opuses O, Creative Cr, Authors A, Checkout Ch WHERE B.Code=C.Code AND O.OID=C.OpusID AND O.OID=Cr.OID AND Cr.AID=A.AID AND " +
                        "Ch.Code=B.Code AND (B.Code NOT IN (SELECT Code FROM Checkout WHERE BackDate IS NULL)) " +
                        "UNION SELECT B.Code, BName, OName, AName FROM Books B, Content C, Opuses O, Creative Cr, Authors A WHERE B.Code=C.Code AND O.OID=C.OpusID AND O.OID=Cr.OID AND Cr.AID=A.AID AND B.Code NOT IN (SELECT Code FROM Checkout)",
                        GiveDataGrid);
                    //TextBoxSearchGive.BorderBrush = Brushes.Crimson;
                    return;
                }

                //if (ComboBoxClients.SelectedValue == null) return;

                if (ComboBoxEnt.SelectedValue != null && ComboBoxParam.SelectedValue != null)
                {
                    entity = ComboBoxEnt.SelectedValue.ToString();
                    param = ComboBoxParam.SelectedValue.ToString();

                    property = SwitchValue(entity, param);

                    ButtonGive.IsEnabled = true;
                    FillDataGrid(
                        "SELECT B.Code, BName, OName, AName FROM Books B, Content C, Opuses O, Creative Cr, Authors A, Checkout Ch WHERE B.Code=C.Code AND O.OID=C.OpusID AND O.OID=Cr.OID AND Cr.AID=A.AID AND " +
                        "Ch.Code=B.Code AND (B.Code NOT IN (SELECT Code FROM Checkout WHERE BackDate IS NULL)) AND " +
                        property + " LIKE '%" + TextBoxSearchGive.Text + "%' " +
                        "UNION SELECT B.Code, BName, OName, AName FROM Books B, Content C, Opuses O, Creative Cr, Authors A WHERE B.Code=C.Code AND O.OID=C.OpusID AND O.OID=Cr.OID AND Cr.AID=A.AID AND B.Code NOT IN (SELECT Code FROM Checkout) AND " +
                        property + " LIKE '%" + TextBoxSearchGive.Text + "%' ",
                        GiveDataGrid);
                }
            }
        }

        private void TextBoxSearchGive_OnKeyDown(object sender, KeyEventArgs e)
        {
            Key k = e.Key;
            SearchGive(k);
        }

        private void ComboBoxEnt_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string entity = ComboBoxEnt.SelectedValue.ToString();
            if (entity == "Книги")
            {
                ComboBoxParameter = new List<string>() { "Название", "Автор", "Шифр" };
                ComboBoxParam.ItemsSource = ComboBoxParameter;
            }
            else
            {
                ComboBoxParameter = new List<string>() { "Название", "Автор" };
                ComboBoxParam.ItemsSource = ComboBoxParameter;
            }
        }

        private void TextBoxSearchGive_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBoxSearchGive.Text = "";
        }

        private void ButtonGive_OnClick(object sender, RoutedEventArgs e)
        {
            DataGrid dgDataGrid = this.GiveDataGrid;
            string idValue = "";
            try
            {
                foreach (DataGridCellInfo di in dgDataGrid.SelectedCells)
                {
                    DataRowView dvr = (DataRowView) di.Item;
                    idValue = dvr[0].ToString();
                }
            }
            catch
            {
            }

            var clientName = ComboBoxClients.SelectedValue.ToString();
            var clientId =
                Command.ReadData("SELECT CardNumber FROM Clients WHERE Name = '" + clientName + "';", "CardNumber")[0];

            // cant't give more than 5 books
            var booksCounter =
                Command.GetIntValue("SELECT Count(CardNumber) FROM Checkout WHERE CardNumber=" + clientId +
                                    " AND BackDate IS NULL");
            if (booksCounter >= 5)
            {
                MessageBox.Show("Невозможно выдать ещё одну книгу. Максимальное число книг: 5.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // can't give a book to a client who hasn't gave his books in time
            var overdueClientId = Command.GetIntValue("SELECT CardNumber FROM Overdue WHERE CardNumber = " + clientId);
            if (Convert.ToInt32(clientId) == overdueClientId)
            {
                var overdueBooks = Command.ReadData("SELECT BName FROM Overdue WHERE CardNumber=" + clientId, "BName");
                var books = overdueBooks.Aggregate("", (current, book) => current + book + "\n");
                MessageBox.Show("Невозможно выдать книгу так как у данного читателя есть задолженности:\n" + books,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            var property = "";
            try
            {
                var entity = ComboBoxEnt.SelectedValue.ToString();
                var param = ComboBoxParam.SelectedValue.ToString();

                entity = ComboBoxEnt.SelectedValue.ToString();
                param = ComboBoxParam.SelectedValue.ToString();

                property = SwitchValue(entity, param);
            }
            catch
            {
                
            }

            if (ComboBoxClients.SelectedValue != null)
                Command.ExecuteCommand("INSERT INTO Checkout(Code, CardNumber, GiveDate) VALUES(" + idValue + ", " +
                                   clientId + ",GETDATE());");
            if (property != "")
            {
                FillDataGrid(
                        "SELECT B.Code, BName, OName, AName FROM Books B, Content C, Opuses O, Creative Cr, Authors A, Checkout Ch WHERE B.Code=C.Code AND O.OID=C.OpusID AND O.OID=Cr.OID AND Cr.AID=A.AID AND " +
                        "Ch.Code=B.Code AND (B.Code NOT IN (SELECT Code FROM Checkout WHERE BackDate IS NULL)) AND " +
                        property + " LIKE '%" + TextBoxSearchGive.Text + "%' " +
                        "UNION SELECT B.Code, BName, OName, AName FROM Books B, Content C, Opuses O, Creative Cr, Authors A WHERE B.Code=C.Code AND O.OID=C.OpusID AND O.OID=Cr.OID AND Cr.AID=A.AID AND B.Code NOT IN (SELECT Code FROM Checkout) AND " +
                        property + " LIKE '%" + TextBoxSearchGive.Text + "%' ",
                        GiveDataGrid);
            }
            else
            {
                FillDataGrid(
                         "SELECT B.Code, BName, OName, AName FROM Books B, Content C, Opuses O, Creative Cr, Authors A, Checkout Ch WHERE B.Code=C.Code AND O.OID=C.OpusID AND O.OID=Cr.OID AND Cr.AID=A.AID AND " +
                         "Ch.Code=B.Code AND (B.Code NOT IN (SELECT Code FROM Checkout WHERE BackDate IS NULL)) " +
                         "UNION SELECT B.Code, BName, OName, AName FROM Books B, Content C, Opuses O, Creative Cr, Authors A WHERE B.Code=C.Code AND O.OID=C.OpusID AND O.OID=Cr.OID AND Cr.AID=A.AID AND B.Code NOT IN (SELECT Code FROM Checkout)",
                         GiveDataGrid);
                
            }
            UpdateLists();
            FillStartGrids();
            ShowChart();

            var result = MessageBox.Show("Создать отчёт о выданной книге?", "Создание отчёта", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                var path = @"D:\CityLibrary\Book" + idValue + "_" + clientId + "_" + ".txt";
                var text = Command.GetBookInfo(Convert.ToInt32(idValue));
                try
                {
                    using (TextWriter writer = File.CreateText(path))
                    {
                        int i = 0;
                        while(text.Count > i)
                            writer.WriteLine(text[i++]);
                    }
                    MessageBox.Show("Отчёт создан. Путь: " + path, "Уведомление");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void UpdateLists()
        {
            LastGivenBooks = LastGivenBooks = Command.ReadDataToItemsList("SELECT TOP 5 BName, GiveDate, Name FROM Books B, Checkout C, Clients Cl WHERE B.Code=C.Code AND Cl.CardNumber=C.CardNumber AND BackDate IS NULL ORDER BY GiveDate DESC", new List<string>() { "BName", "GiveDate", "Name" });
            ListBoxLastGiven.ItemsSource = LastGivenBooks;

            LastBackBooks = LastGivenBooks = Command.ReadDataToItemsList("SELECT TOP 5 BName, BackDate, Name FROM Books B, Checkout C, Clients CL WHERE B.Code=C.Code AND Cl.CardNumber=C.CardNumber AND BackDate IS NOT NULL ORDER BY BackDate DESC", new List<string>() { "BName", "BackDate", "Name" });
            ListBoxLastBack.ItemsSource = LastBackBooks;

            OverdueBooks = LastGivenBooks = Command.ReadDataToItemsList("SELECT TOP 5 BName, Term, Name FROM Overdue O, Clients C WHERE o.CardNumber=C.CardNumber ORDER BY Term DESC", new List<string>() { "BName", "Term", "Name" });

            ListBoxOverdue.ItemsSource = OverdueBooks;

        }

        private string SwitchValue(string entity, string param)
        {
            var property = "";
            if (entity == "Книги")
            {
                if (param == "Название")
                    property = "BName";
                if (param == "Автор")
                    property = "AName";
                if (param == "Шифр")
                    property = "B.Code";
                return property;
            }

            if (entity == "Произведения")
            {
                if (param == "Название")
                    property = "OName";
                if (param == "Автор")
                    property = "AName";
                return property;
            }
            return property;
        }

        private void ComboBoxClients_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GiveDataGrid.SelectedValue == null)
                return;
            ButtonGive.IsEnabled = true;
        }

        private void ComboBoxEntBack_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string entity = ComboBoxEntBack.SelectedValue.ToString();
            if (entity == "Книги")
            {
                ComboBoxParameter = new List<string>() { "Название", "Автор", "Шифр" };
                ComboBoxParamBack.ItemsSource = ComboBoxParameter;
            }
            else
            {
                ComboBoxParameter = new List<string>() { "Название", "Автор" };
                ComboBoxParamBack.ItemsSource = ComboBoxParameter;
            }
        }

        private void TextBoxSearchBack_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBoxSearchBack.Text = "";
        }


        private static string back = "SELECT Ch.ChID, B.Code, BName, OName, AName,  GiveDate, Ch.CardNumber, Cl.Name " +
                                     "FROM Books B, Content C, Opuses O, Creative Cr, Authors A, Checkout Ch, CLients Cl " +
                                     "WHERE B.Code=C.Code AND O.OID=C.OpusID AND O.OID=Cr.OID AND Cr.AID=A.AID AND Ch.CardNumber=Cl.CardNumber AND Ch.Code=B.Code AND B.Code IN (SELECT Code FROM Checkout) AND (BackDate IS NULL)";//" OR BackDate < (SELECT Term FROM Overdue));";

        private void SearchBack(Key e)
        {
            var entity = "";
            var param = "";
            var property = "AName";

            TextBoxSearchBack.BorderBrush = Brushes.DarkSlateGray;
            if (ComboBoxEntBack.SelectedValue == null || ComboBoxParamBack.SelectedValue == null && e == Key.Enter)
            {

                FillDataGrid(back,
                    BackDataGrid);
                //TextBoxSearchBack.BorderBrush = Brushes.Crimson;
                return;
            }

            entity = ComboBoxEntBack.SelectedValue.ToString();
            param = ComboBoxParamBack.SelectedValue.ToString();

            property = SwitchValue(entity, param);

            if (e == Key.Enter)
            {
                string q = "SELECT ChID, B.Code, BName, OName, AName,  GiveDate, Ch.CardNumber, Cl.Name " +
                           "FROM Books B, Content C, Opuses O, Creative Cr, Authors A, Checkout Ch, CLients Cl " +
                           "WHERE B.Code=C.Code AND O.OID=C.OpusID AND O.OID=Cr.OID AND Cr.AID=A.AID AND Ch.CardNumber=Cl.CardNumber AND Ch.Code=B.Code AND " +
                           property + " LIKE '%" + TextBoxSearchBack.Text +
                           "%' AND B.Code IN (SELECT Code FROM Checkout) AND BackDate IS NULL;";

                FillDataGrid(
                    q,
                    BackDataGrid);
            }
        }

        private void TextBoxSearcBack_OnKeyDown(object sender, KeyEventArgs e)
        {
            SearchBack(e.Key);
        }

        private void BackDataGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonBack.IsEnabled = true;
        }

        private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
        {
            DataGrid dgDataGrid = this.BackDataGrid;
            string idValue = "";
            foreach (DataGridCellInfo di in dgDataGrid.SelectedCells)
            {
                DataRowView dvr = (DataRowView)di.Item;
                idValue = dvr[0].ToString();
            }

            Command.ExecuteCommand("UPDATE Checkout SET BackDate=GETDATE() WHERE ChID=" + idValue);
            FillDataGrid(back, BackDataGrid);

            UpdateLists();
            FillStartGrids();
        }

        private void GiveDataGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonGive.IsEnabled = false || ComboBoxClients.SelectedValue != null;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            new AddClient().ShowDialog();
            UpdateClients();
        }

        private void ButtonGiveSearch_OnClick(object sender, RoutedEventArgs e)
        {
            SearchGive(Key.Enter);
        }

        private void ButtonBackSearch_OnClick(object sender, RoutedEventArgs e)
        {
            SearchBack(Key.Enter);
        }

        private void ButtonOverdue_OnClick(object sender, RoutedEventArgs e)
        {
            if (Command.GetIntValue("SELECT COUNT(*) FROM Overdue") == 0)
                return;
            var path = @"D:\CityLibrary\Overdue.txt";
            var text = Command.GetOverdue();
            try
            {
                using (TextWriter writer = File.CreateText(path))
                {
                    int i = 0;
                    while (text.Count > i)
                        writer.WriteLine(text[i++]);
                    writer.WriteLine();
                }
                System.Windows.MessageBox.Show("Отчёт создан. Путь: " + path, "Уведомление");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
    }
}
