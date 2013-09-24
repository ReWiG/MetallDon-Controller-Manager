using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MetallDon_Controller_Manager
{
    class ManagerController
    {
        ManagerDB db;
        String DateAccident;
        Timer TimerUpdateDB = new Timer(Properties.Settings.Default.intervalUpdateDB);

        List<MOXAController> ControllerList = new List<MOXAController>();
        List<Sensor> SensorList = new List<Sensor>();

        public ManagerController()
        {
            db = new ManagerDB(Properties.Settings.Default.nameDB,
                Properties.Settings.Default.ipDB,
                Properties.Settings.Default.userDB,
                Properties.Settings.Default.passDB);
            TimerUpdateDB.Elapsed += new ElapsedEventHandler(CheckUpdateSensorList);
            TimerUpdateDB.Start();
        }

        public void FillSensorList()
        {
            List<object[]> reader = db.SelectSensor();
            if (reader != null)
            {
                foreach(object[] list in reader)
                {
                    AddSensor(
                        list[0].ToString(),
                        Int32.Parse(list[1].ToString()),
                        Boolean.Parse(list[2].ToString()),
                        Boolean.Parse(list[3].ToString()),
                        list[4].ToString(),
                        list[5].ToString(),
                        Int32.Parse(list[6].ToString()));
                }
            }
            else
            {
                Console.ReadKey();
                Environment.Exit(1);
            }
        }

        // Добавить датчик и контроллер
        void AddSensor(String id, Int32 port, Boolean state, Boolean normalState,
            String ip, String pass, Int32 ping)
        {
            // Если данного IP нет в списке, то добавляем контроллер
            int temp = ControllerList.FindIndex(ctr => ctr.GetIPAddress() == ip);
            if (temp != -1)
            {
                SensorList.Add(new Sensor(id, port, normalState, state, ControllerList[temp]));
            }
            else 
            {
                MOXAController contr = new MOXAController(ip, pass, ping);
                SensorList.Add(new Sensor(id, port, normalState, state, contr));
                ControllerList.Add(contr);
            }
        }

        public void RunningMonitoring()
        {
            foreach (MOXAController con in ControllerList)
            {
                // Ивент для записи аварии контроллера
                con.FailCheckInputsEvent += (sender, ip) =>
                {
                    DateAccident = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    // Записываем в базу и запоминаем ID аварии
                    con.SetIdAccident(db.InsertAccident("INSERT INTO `MoxaControllerAlarm` SELECT NULL, id, '" +
                            DateAccident + "', NULL FROM MoxaController WHERE ipAddress = '" +
                            ip + "';"));
                };

                // Ивент для записи времени восстановления контроллера
                con.RecoveryAccidentEvent += (Sender, IdAccident) =>
                {
                    DateAccident = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    db.Update("UPDATE `MoxaControllerAlarm`" +
                        " SET `recoveryDateTime`='" + DateAccident + "' WHERE id = " + 
                        con.GetIdAccident());
                    con.SetIdAccident(0); // обнуляем ID аварии
                };

                if (con.Connect())
                {
                    con.CheckInputsTimer.Start();
                }
                else
                {
                    String DateAccident = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    // Записываем в базу и запоминаем ID аварии
                    con.SetIdAccident(db.InsertAccident("INSERT INTO `MoxaControllerAlarm` SELECT NULL, id, '" +
                            DateAccident + "', NULL FROM MoxaController WHERE ipAddress = '" +
                            con.GetIPAddress() + "';"));
                    con.ReconnectTimer.Start(); // Запускаем таймер для бесконечной попытки приконнектится
                }
            }

            foreach (Sensor sens in SensorList)
            {
                // Ивент для записи состояния датчика в базу
                sens.SetStateSensorEvent += (sender, args) =>
                {
                    db.Update("UPDATE `MoxaSensor` SET `state`=" + args[1] +
                        " WHERE idsensor = " + args[0]);
                };

                // Ивент для записи аварий датчика
                sens.SetAccidentSensorEvent += (sender, idsensor) =>
                {
                    DateAccident = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    // Записываем в базу и запоминаем ID аварии
                    sens.SetIdAccident(db.InsertAccident("INSERT INTO `MoxaSensorAlarm` SELECT NULL, fkSensor, '" +
                            DateAccident + "', NULL FROM MoxaSensor WHERE idsensor = '" +
                            idsensor + "';"));
                };

                // Ивент для записи времени восстановления датчика
                sens.RecoveryAccidentEvent += (Sender, IdAccident) =>
                {
                    DateAccident = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    db.Update("UPDATE `MoxaSensorAlarm`" +
                        " SET `recoveryDateTime`='" + DateAccident + "' WHERE id = " +
                        sens.GetIdAccident());
                    sens.SetIdAccident(0); // обнуляем ID аварии
                };

                sens.CheckStatusTimer.Start();
            }
        }
        // Проверка и загрузка обновлённого списка датчиков и контроллеров из БД
        void CheckUpdateSensorList(object s, ElapsedEventArgs e)
        {
            if (db.IsUpdated())
            {
                foreach (MOXAController ctr in ControllerList)
                {
                    ctr.CheckInputsTimer.Stop();
                    ctr.ReconnectTimer.Stop();
                    ctr.Disconnect();
                }
                foreach (Sensor sen in SensorList)
                {
                    sen.CheckStatusTimer.Stop();
                }
                ControllerList.Clear();
                SensorList.Clear();

                Console.WriteLine("======================\nСписок датчиков и контроллеров успешно очищен\n======================");
                LogManager.Write("======================\nСписок датчиков и контроллеров успешно очищен\n======================", false);
                FillSensorList();
                Console.WriteLine("...и загружен заного");
                LogManager.Write("...и загружен заного", false);
                RunningMonitoring();
                Console.WriteLine("======================\nМониторинг запущен\n======================");
                LogManager.Write("...и загружен заного", false);
            }
        }
    }
}
