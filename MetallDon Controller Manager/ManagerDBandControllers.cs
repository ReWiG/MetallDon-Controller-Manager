using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using MySql.Data.MySqlClient;

namespace MetallDon_Controller_Manager
{
    class ManagerDBandControllers
    {
        List<ControllerMOXA> ControllerlList = new List<ControllerMOXA>();
        String ConnectionString;
        Timer TimerUpdateDB = new Timer(Properties.Settings.Default.intervalUpdateDB);
        
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="nameDB">Имя базы в MySQL</param>
        /// <param name="host">Имя или IP-адрес сервера</param>
        /// <param name="user">Имя пользователя MySQL</param>
        /// <param name="password">Пароль пользователя БД MySQL</param>
        public ManagerDBandControllers(String nameDB, String host, String user, String password)
        {
            ConnectionString = "Database="+nameDB+";Data Source="+host+";User Id="+user+";Password="+password;
            TimerUpdateDB.Elapsed += new ElapsedEventHandler(UpdateControllerList);
            TimerUpdateDB.Start();
        }

        // Добавить контроллер
        void AddController(String ip, String pass, Int32 ping)
        {
            ControllerlList.Add(new ControllerMOXA(ip, pass, ping, SetAccidentToDB));
        }

        // Запуск таймера для проверки
        public void RunningMonitoring()
        {
            foreach (ControllerMOXA con in ControllerlList)
            {                
                if (con.Connect() && con.isConnected())
                {
                    con.Timer.Start();
                }
                else
                {
                    using (MySqlConnection ManagerConn = new MySqlConnection(ConnectionString))
                    {
                        MySqlCommand command = new MySqlCommand();
                        String DateAccident = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        con.SetDateAccident(DateAccident);
                        command.CommandText = "INSERT INTO `accidentcontroller` select NULL, id, '" +
                            DateAccident +"' FROM controllers WHERE ipAddress = '" +
                            con.GetIPAddress() + "'";
                        Console.WriteLine(command.CommandText);
                        command.Connection = ManagerConn;
                        try
                        {
                            command.Connection.Open();
                            command.ExecuteNonQuery();
                        }
                        catch (MySqlException ex)
                        {
                            Console.WriteLine("Ошибка работы с БД(UpdateControllerList): \r\n{0}", ex.ToString());
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                        finally
                        {
                            command.Connection.Close();
                        }
                    }
                    con.ReConnectTimer.Start(); // Запускаем таймер для бесконечной попытки приконнектится
                }
            }
        }

        // Получение контроллеров из БД
        public void SelectControllers() {
            using (MySqlConnection ManagerConn = new MySqlConnection(ConnectionString))
            {
                MySqlCommand command = new MySqlCommand();
                command.CommandText = "SELECT * FROM controllers WHERE working = 1";
                command.Connection = ManagerConn;
                MySqlDataReader reader;
                try
                {
                    command.Connection.Open();
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {                        
                        AddController(
                            reader["ipAddress"].ToString(),
                            reader["password"].ToString(),
                            Int32.Parse(reader["pingInterval"].ToString()));
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

        // Обновление списка контроллеров из базы
        void ReLoadControllers()
        {
            foreach(ControllerMOXA ctr in ControllerlList) {
                ctr.Timer.Stop();
                ctr.Disconnect();
            }
            ControllerlList.Clear();
            Console.WriteLine("Список контроллеров успешно очищен...");
            SelectControllers();
            Console.WriteLine("...и загружен заного");
            RunningMonitoring();
            Console.WriteLine("Мониторинг запущен");
        }

        // Проверка и загрузка обновлённого списка контроллеров из БД
        void UpdateControllerList(object s, ElapsedEventArgs e)
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
                        command.CommandText = "UPDATE `update` SET `isUpdated`= 0 WHERE `id` = 1";
                        command.ExecuteNonQuery();
                        ReLoadControllers();
                    }

                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("Ошибка работы с БД(UpdateControllerList): \r\n{0}", ex.ToString());
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                finally
                {
                    command.Connection.Close();
                }
            }
        }

        void SetAccidentToDB(String ip)
        {
            using (MySqlConnection ManagerConn = new MySqlConnection(ConnectionString))
            {
                MySqlCommand command = new MySqlCommand();
                String DateAccident = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                command.CommandText = "INSERT INTO `accidentcontroller` select NULL, id, '" +
                    DateAccident + "' FROM controllers WHERE ipAddress = '" +
                    ip + "'";
                Console.WriteLine(command.CommandText);
                command.Connection = ManagerConn;
                try
                {
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("Ошибка работы с БД(UpdateControllerList): \r\n{0}", ex.ToString());
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
}
