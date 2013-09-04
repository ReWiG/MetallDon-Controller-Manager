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
        List<MOXAController> ControllerlList = new List<MOXAController>();
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
        void AddController(String ip, String pass, Int32 ping, Int32 port, Boolean state, Boolean normalState)
        {
            // Если данный IP уже есть в списке, то добавляем датчик
            int temp = ControllerlList.FindIndex(ctr => ctr.GetIPAddress() == ip);
            if (temp != -1)
            {
                ControllerlList[temp].AddSensor(new Sensor(port, normalState, state));
            }
            else
            {
                MOXAController tempContr = new MOXAController(ip, pass, ping);
                tempContr.AddSensor(new Sensor(port, normalState, state));
                ControllerlList.Add(tempContr);
            }
        }

        // Запуск таймера для проверки
        public void RunningMonitoring()
        {
            foreach (MOXAController con in ControllerlList)
            {
                // Ивент для обработки ошибок чтения портов
                con.FailCheckInputsEvent += (sender, ip) =>
                {
                    String DateAccident = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    InsertToDB("INSERT INTO `accidentcontroller` select NULL, id, '" +
                            DateAccident + "' FROM controllers WHERE ipAddress = '" +
                            ip + "'");
                };

                // Коннектимся
                if (con.Connect())
                {
                    con.CheckInputsTimer.Start(); // Запускаем проверку датчиков
                }
                else
                {
                    String DateAccident = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    InsertToDB("INSERT INTO `accidentcontroller` select NULL, id, '" +
                            DateAccident + "' FROM controllers WHERE ipAddress = '" +
                            con.GetIPAddress() + "'");
                    con.ReconnectTimer.Start(); // Запускаем таймер для бесконечной попытки приконнектится
                }
            }
        }

        // Получение контроллеров из БД
        public void SelectControllers() {
            using (MySqlConnection ManagerConn = new MySqlConnection(ConnectionString))
            {
                MySqlCommand command = new MySqlCommand();
                command.CommandText = "SELECT  ipAddress, password, pingInterval, port, state, normalState FROM controllers INNER JOIN sensors ON controllers.id = sensors.controller WHERE controllers.working = 1 ";
                command.Connection = ManagerConn;
                MySqlDataReader reader;
                try
                {
                    command.Connection.Open();
                    reader = command.ExecuteReader();

                    while(reader.Read())
                    {                        
                        AddController(
                            reader["ipAddress"].ToString(),
                            reader["password"].ToString(),
                            Int32.Parse(reader["pingInterval"].ToString()),
                            Int32.Parse(reader["port"].ToString()),
                            Boolean.Parse(reader["state"].ToString()),
                            Boolean.Parse(reader["normalState"].ToString()));
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
            foreach(MOXAController ctr in ControllerlList) {
                ctr.CheckInputsTimer.Stop();
                ctr.ReconnectTimer.Stop();
                ctr.Disconnect();
            }
            ControllerlList.Clear();
            Console.WriteLine("======================\nСписок контроллеров успешно очищен\n======================");
            SelectControllers();
            Console.WriteLine("...и загружен заного");
            RunningMonitoring();
            Console.WriteLine("======================\nМониторинг запущен\n======================");
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

        void InsertToDB(String CommandText)
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
