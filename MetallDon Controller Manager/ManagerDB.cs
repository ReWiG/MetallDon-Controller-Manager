using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace MetallDon_Controller_Manager
{
    class ManagerDB
    {
        String ConnectionString;

        public ManagerDB(String nameDB, String host, String user, String password)
        {
            ConnectionString = "Persist Security Info=False;Database=" + nameDB + ";Data Source=" + host + ";User Id=" + user + ";Password=" + password;
        }

        // Получение датчиков из БД
        public List<object[]> SelectSensor()
        {
            using (MySqlConnection ManagerCon = new MySqlConnection(ConnectionString))
            {
                MySqlCommand command = new MySqlCommand();
                command.CommandText = "SELECT idsensor, port, state, normalState, ipAddress, password, pingInterval FROM controllers INNER JOIN sensors ON controllers.id = sensors.controller WHERE controllers.working = 1";
                command.Connection = ManagerCon;
                MySqlDataReader reader;
                try
                {
                    command.Connection.Open();
                    reader =  command.ExecuteReader();
                    List<object[]> result = new List<object[]>();
                    while (reader.Read())
                    {
                        object[] values = new Object[reader.FieldCount];
                        int fieldCount = reader.GetValues(values);
                        result.Add(values);
                    }
                    reader.Close();
                    return result;
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("Ошибка работы с БД(SelectControllers): \r\n{0}", ex.ToString());
                    return null;
                }
                finally
                {
                    command.Connection.Close();
                    ManagerCon.Close();
                }
            }
        }

        public Boolean IsUpdated()
        {
            using (MySqlConnection ManagerConn = new MySqlConnection(ConnectionString))
            {
                MySqlCommand command = new MySqlCommand();
                command.CommandText = "SELECT `isUpdated` FROM `update` WHERE id =1";
                command.Connection = ManagerConn;
                try
                {
                    command.Connection.Open();

                    if ((Boolean)command.ExecuteScalar())
                    {
                        Console.WriteLine("UPDATE `update` SET `isUpdated`= 0 WHERE `id` = 1");
                        command.CommandText = "UPDATE `update` SET `isUpdated`= 0 WHERE `id` = 1";
                        command.ExecuteNonQuery();

                        return true;
                    }
                    return false;
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("Ошибка работы с БД(UpdateControllerList): \r\n{0}", ex);
                    return false;
                }
                finally
                {
                    command.Connection.Close();
                    ManagerConn.Close();
                }
            }
        }

        public void InsertToDB(String CommandText)
        {
            using (MySqlConnection ManagerConn = new MySqlConnection(ConnectionString))
            {
                MySqlCommand command = new MySqlCommand();

                command.CommandText = CommandText;
                Console.WriteLine(command.CommandText);
                command.Connection = ManagerConn;
                try
                {
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("Ошибка работы с БД(InsertToDB): \r\n{0}", ex.ToString());
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                finally
                {
                    command.Connection.Close();
                    ManagerConn.Close();
                }
            }
        }
    }
}
