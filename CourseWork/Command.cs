using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWork
{
    internal class Command
    {
        public SqlCommand command;
        public Command(string query)
        {
            if (Connection.FirstLoading)
                Connection.CreateConnection();
            command = Connection.SqlConnection.CreateCommand();
            command.CommandText = query;
        }
    }
}
