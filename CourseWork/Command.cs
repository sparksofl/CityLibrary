using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using CourseWork;

namespace CourseWork
{
    internal static class Command
    {
        public static SqlCommand CreateCommand(string query)
        {
            Connection.CreateConnection();

            var command = Connection.SqlConnection.CreateCommand();
            command.CommandText = query;

            //Connection.SqlConnection.Close();

            return command;
        }

        /*public static List<string> ReadData(string query, string s)
        {
            Connection.CreateConnection();

            var items = new List<string>();
            var c = CreateCommand(query);
            var reader = c.ExecuteReader();
            do
            {

                while (reader.Read())
                {

                    items.Add(reader[s].ToString());
                }
                /*items.Add(reader.GetValue(2).ToString());
                items.Add(reader.GetValue(3).ToString());
                items.Add(reader.GetValue(4).ToString());
                items.Add(reader.GetValue(5).ToString());
            } while (reader.NextResult());
            //if (!reader.Read())
            //reader = c.ExecuteReader();

            Connection.SqlConnection.Close();

            return items;
        }*/

        public static List<string> ReadData(string query, List<string> list)
        {
            Connection.CreateConnection();

            var items = new List<string>();
            var c = CreateCommand(query);
            var reader = c.ExecuteReader();
            do
            {
                while (reader.Read())
                {
                    int i = 0;
                    while(i < list.Count)
                        items.Add(reader[i++].ToString());
                }
            } while (reader.NextResult());

            Connection.SqlConnection.Close();

            return items;
        }

        public static List<string> ReadDataToItemsList(string query, List<string> list)
        {
            Connection.CreateConnection();
            var array = new string[list.Count * list.Count];
            try
            {
                var c = CreateCommand(query);
                var reader = c.ExecuteReader();
                var j = 0;
                do
                {
                    while (reader.Read())
                    {
                        int i = 0;
                        while (i < list.Count)
                            array[j] += (reader[list[i++]].ToString()) + "\r";
                        j++;
                    }
                } while (reader.NextResult());

                Connection.SqlConnection.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return array.ToList();
        }

        public static List<string> ReadData(string query, string p)
        {
            return ReadData(query, new List<string>() {p});
        }

        public static List<KeyValuePair<string, int>> ReadDataToPair(string query)
        {
            Connection.CreateConnection();
                List<KeyValuePair<string, int>> pairs = new List<KeyValuePair<string, int>>();
                var c = CreateCommand(query);
            try
            {
                var reader = c.ExecuteReader();
                do
                {
                    while (reader.Read())
                    {
                            pairs.Add(new KeyValuePair<string, int>(reader[0].ToString(),
                                Convert.ToInt32(reader[1])));
                    }
                } while (reader.NextResult());

                Connection.SqlConnection.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return pairs;
        }

        public static int GetIntValue(string query)
        {
            try
            {
                var c = Connection.CreateConnection();
                SqlCommand cmd = new SqlCommand(query, c);

                return (int) cmd.ExecuteScalar();
            }
            catch
            {
                return 0;
            }

        }

        public static bool ExecuteCommand(string query)
        {
            if (query == null) return false;

            try
            {
                Connection.CreateConnection();

                var c = CreateCommand(query);
                c.ExecuteNonQuery();

                Connection.SqlConnection.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("Неверные данные.\n\nПодробности: \n" + e.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
    }
}
