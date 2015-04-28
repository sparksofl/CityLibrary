﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;

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

        public static List<string> ReadData(string query, string s)
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
                items.Add(reader.GetValue(5).ToString());*/
            } while (reader.NextResult());
            //if (!reader.Read())
            //reader = c.ExecuteReader();

            Connection.SqlConnection.Close();

            return items;
        }

        public static List<string> ReadData(string query, string s, string s2)
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
                    items.Add(reader[s2].ToString());
                }
                /*items.Add(reader.GetValue(2).ToString());
                items.Add(reader.GetValue(3).ToString());
                items.Add(reader.GetValue(4).ToString());
                items.Add(reader.GetValue(5).ToString());*/
            } while (reader.NextResult());
            //if (!reader.Read())
            //reader = c.ExecuteReader();

            Connection.SqlConnection.Close();

            return items;
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
