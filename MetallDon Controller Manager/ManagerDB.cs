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
        string connString;
        MySqlConnection mngConn;
        public ManagerController mngContrl = new ManagerController();

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="nameDB">Имя базы в MySQL</param>
        /// <param name="host">Имя или IP-адрес сервера</param>
        /// <param name="user">Имя пользователя MySQL</param>
        /// <param name="password">пароль пользователя БД MySQL</param>
        public ManagerDB(String nameDB, String host, String user, String password)
        {
            connString = "Database="+nameDB+";Data Source="+host+";User Id="+user+";Password="+password;
            mngConn = new MySqlConnection(connString);
        }


        public void SelectControllers() {
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "SELECT * FROM controllers";
            command.Connection = mngConn;
            MySqlDataReader reader;
            try
            {
                command.Connection.Open();
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    mngContrl.addController(new Controller(
                        reader["ipAddress"].ToString(),
                        reader["password"].ToString(),
                        5000,
                        Int32.Parse(reader["pingInterval"].ToString()), 
                        DateTime.Parse(reader["lastUpdateTime"].ToString())));
                }
                reader.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Ошибка работы с БД(SelectControllers): \r\n{0}", ex.ToString());
                Console.ReadKey();
                Environment.Exit(0);
            }
            finally
            {
                command.Connection.Close();
            }
        }


    }
}
