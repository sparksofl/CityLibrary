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

        public static List<string> ReadData(string query, List<string> list)
        {
            return ReadData(query, list.Count);
        }

        public static List<string> ReadData(string query, int n)
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
                    while (i < n)
                        items.Add(reader[i++].ToString());
                }
            } while (reader.NextResult());

            Connection.SqlConnection.Close();

            return items;
        }

        public static List<string> ReadDataForReport(string query, int n)
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
                    while (i < n)
                        items.Add(reader[i++].ToString() + "\r\n");
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
                            array[j] += (reader[list[i++]]) + "\r";
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

        public static List<string> GetClienInfo(int id)
        {
            var query =
                "SELECT CONCAT('Читательский битет №', C.CardNumber), CONCAT('Имя:    ', Name), " +
                "CONCAT('Адресс:    ', ClientAddress), CONCAT('Номер телефона:    ', PhoneNumber) FROM Clients C" +
                " WHERE C.CardNumber=" + id;

            return ReadDataForReport(query, 4);
        }

        public static List<string> GetBookInfo(int code)
        {
            var query =
                "SELECT DISTINCT CONCAT('Шифр:    ', B.Code), CONCAT('Название книги:    ', BName), CONCAT('Автор:    ', AName), CONCAT('Дата выдачи:    ',GiveDate) FROM Books B, Checkout Ch, Content Ct, Opuses O, Creative Cr, Authors A, Clients Cl " +
                "WHERE B.Code=Ch.Code AND Ct.Code=B.Code AND O.OID=Ct.OpusID AND Cr.OID=O.OID AND A.AID=Cr.AID AND Cl.CardNumber=Ch.CardNumber AND BackDate IS NULL AND B.Code=" +
                code;
            return ReadDataForReport(query, 4);
        }

        public static List<string> GetOverdue()
        {
            var query =
                "SELECT CONCAT('Шифр:    ', Code), CONCAT('Название:    ', BName), CONCAT('Последний срок сдачи:    ', Term), CONCAT('Читатель:    ', Name) FROM Overdue O, Clients C WHERE O.CardNumber=C.CardNumber";
            return ReadDataForReport(query, 4);
        } 
    }
}
