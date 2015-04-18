using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;

namespace CourseWork
{
    internal static class Connection
    {
        public static bool IsOpened;
        public static bool FirstLoading = true;

        private static string _connectionString =
            @"Integrated Security=SSPI;Persist Security Info=False;User ID=Maria;Initial Catalog=CityLibrary;Data Source=MYPC";
        public static SqlConnection SqlConnection { get; private set; }

        public static SqlConnection CreateConnection()
        {
            if (FirstLoading)
                Mouse.OverrideCursor = Cursors.Wait;

            SqlConnection = new SqlConnection();

            try
            {
                SqlConnection.ConnectionString = _connectionString;
                SqlConnection.Open();
                IsOpened = true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Произошла следующая ошибка: " + ex.Message, "Ошибка",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }


            Mouse.OverrideCursor = Cursors.Arrow;
            FirstLoading = false;

            return SqlConnection;
        }
    }
}
