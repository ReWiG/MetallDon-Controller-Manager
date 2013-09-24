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
                command.CommandText = "SELECT idsensor, port, state, normalState, ipAddress, password, pingInterval FROM MoxaController INNER JOIN MoxaSensor ON MoxaController.id = MoxaSensor.fkController WHERE MoxaController.active = 1";
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
                    LogManager.Write("Ошибка работы с БД(SelectControllers): \r\n" + ex.Message, true);
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
                command.CommandText = "SELECT `request` FROM `MoxaUpdateRequest` WHERE id =1";
                command.Connection = ManagerConn;
                try
                {
                    command.Connection.Open();

                    if ((Boolean)command.ExecuteScalar())
                    {
                        Console.WriteLine("UPDATE `MoxaUpdateRequest` SET `request`= 0 WHERE `id` = 1");
                        LogManager.Write("UPDATE `MoxaUpdateRequest` SET `request`= 0 WHERE `id` = 1", false);

                        command.CommandText = "UPDATE `MoxaUpdateRequest` SET `request`= 0 WHERE `id` = 1";
                        command.ExecuteNonQuery();

                        return true;
                    }
                    return false;
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("Ошибка работы с БД(isUpdated): \r\n{0}", ex);
                    LogManager.Write("Ошибка работы с БД(isUpdated): \r\n" + ex.Message, true);
                    return false;
                }
                finally
                {
                    command.Connection.Close();
                    ManagerConn.Close();
                }
            }
        }

        public UInt64 InsertAccident(String CommandText)
        {
            UInt64 temp = 0;
            using (MySqlConnection ManagerConn = new MySqlConnection(ConnectionString))
            {
                MySqlCommand command = new MySqlCommand();

                command.CommandText = CommandText +  "SELECT @@IDENTITY;";

                Console.WriteLine(command.CommandText);
                LogManager.Write(command.CommandText, true);

                command.Connection = ManagerConn;
                try
                {
                    command.Connection.Open();
                    temp = (UInt64)command.ExecuteScalar();
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("Ошибка работы с БД(InsertAccident): \r\n{0}", ex.ToString());
                    LogManager.Write("Ошибка работы с БД(InsertAccident): \r\n" + ex.Message, true);
                }
                finally
                {
                    command.Connection.Close();
                    ManagerConn.Close();
                }
                return temp;
            }
        }

        public void Update(string CommandText)
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
                    Console.WriteLine("Ошибка работы с БД(Update): \r\n{0}", ex.ToString());
                    LogManager.Write("Ошибка работы с БД(Update): \r\n" + ex.Message, true);
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
