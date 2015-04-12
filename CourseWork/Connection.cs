using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Windows;

namespace CourseWork
{
    class Connection
    {
        private string connectionStr = @"Provider=SQLNCLI11.1;Integrated Security=SSPI;Persist Security Info=False;";

        void OpenConnection()
        {
            var connection = new SqlConnection();

            try
            {
                connection.Open();
            }
            catch
            {
                MessageBox.Show("Ошибка в соединении с базой данных");
            }
        }
    }
}
