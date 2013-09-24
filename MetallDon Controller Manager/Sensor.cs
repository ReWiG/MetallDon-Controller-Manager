using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MetallDon_Controller_Manager
{
    class Sensor
    {
        String IdSensor;
        Int32 Port;
        Boolean NormalState;
        Boolean State;
        MOXAController Controller;
        Boolean FlagAccident = false;
        UInt64 IdAccident = 0;

        public event EventHandler<String[]> SetStateSensorEvent;
        public event EventHandler<String> SetAccidentSensorEvent;
        public event EventHandler<UInt64> RecoveryAccidentEvent;

        public Timer CheckStatusTimer = new Timer();

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="id">ID контроллера в БД</param>
        /// <param name="port">Номер выхода</param>
        /// <param name="normState">Нормальное состояние датчика</param>
        /// <param name="state">Текущее состояние датчика</param>
        public Sensor(String id, Int32 port, Boolean normState, Boolean state, MOXAController contr)
        {
            IdSensor = id;
            Port = port;
            NormalState = normState;
            State = state;
            Controller = contr;
            CheckStatusTimer.Interval = Controller.GetPing();
            CheckStatusTimer.Elapsed += new ElapsedEventHandler(CheckStatus);
        }

        public void CheckStatus(object source, ElapsedEventArgs e)
        {
            Boolean[] temp = Controller.GetStatusInputs();
            if (temp != null)
            {
                SetStatusToDB(temp[Port]);

                if (NormalState == temp[Port])
                {
                    Console.WriteLine("Порт {0}, Статус: ОК", Port);
                    LogManager.Write("Порт " + Port + ", Статус: ОК", true);

                    FlagAccident = false;

                    // Если была авария, то записываем время восстановления
                    if (IdAccident != 0)
                    {
                        // Время восстановления
                        var handler = RecoveryAccidentEvent;
                        if (handler != null)
                            handler(this, IdAccident);
                    }
                }
                else
                {
                    Console.WriteLine("Порт {0}, Статус: Авария!", Port);
                    LogManager.Write("Порт " + Port + ", Статус: Авария!", true);

                    var setAccident = SetAccidentSensorEvent;
                    if ((setAccident != null) && (FlagAccident != true))
                    {
                        setAccident(this, IdSensor);
                        FlagAccident = true;
                    }
                    
                }

                State = temp[Port];
            }
        }

        void SetStatusToDB(Boolean newStatus)
        {
            String[] st = new String[2];
            st[0] = IdSensor; // ID датчика

            // Запись статуса датчика в базу
            var setState = SetStateSensorEvent;
            if ((setState != null) && (State != newStatus))
            {
                st[1] = newStatus.ToString();
                setState(this, st);
            }
        }

        public String GetID()
        {
            return IdSensor;
        }

        public Int32 GetPort() 
        {
            return Port;
        }

        public Boolean GetState()
        {
            return State;
        }

        public Boolean GetNormalState()
        {
            return NormalState;
        }

        public void SetIdAccident(UInt64 Id)
        {
            IdAccident = Id;
        }

        public UInt64 GetIdAccident()
        {
            return IdAccident;
        }
    }
}
