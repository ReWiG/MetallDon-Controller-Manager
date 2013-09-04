﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MOXA_CSharp_MXIO;

namespace MetallDon_Controller_Manager
{
    class MOXAController
    {
        const ushort Port = 502;
        const UInt32 Timeout = 2000;
        const double ReconnectTimerInterval = 10000;
        const int NumConnectAttemps = 3;

        String IpAddr;
        String Password;
        List<Sensor> SensorList = new List<Sensor>();
        Int32[] Connection = new Int32[1];
        Boolean IsConnect = false;

        public event EventHandler<String> FailCheckInputsEvent;
        public Timer CheckInputsTimer = new Timer();
        public Timer ReconnectTimer = new Timer(ReconnectTimerInterval);

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="ip">IP-адрес контроллера</param>
        /// <param name="pswrd">Пароль</param>
        /// <param name="timeout">Время таймаута, мс</param>
        /// <param name="ping">Интервал опроса, мс</param>
        public MOXAController(String ip, String pswrd, Int32 ping)
        {
            Int32 Result = MXIO_CS.MXEIO_Init();
            CheckErr(Result, "MXEIO_Init"); // Проверяем инициализацию
            IpAddr = ip;
            Password = pswrd;
            CheckInputsTimer.Interval = ping; // устанавливаем интервал пинга
            CheckInputsTimer.Elapsed += new ElapsedEventHandler(CheckSensors); // и событие
            ReconnectTimer.Elapsed += new ElapsedEventHandler(Reсonnect);          
        }

        // Коннект
        public Boolean Connect() {
            if (!IsConnect)
            {
                Console.WriteLine("Подключение к IP={0}, Порт={1} Таймаут={2}, Пароль={3}", IpAddr, Port, Timeout, Password);
                for (int i = NumConnectAttemps; i > 0; i--)
                {
                    Int32 Result = MXIO_CS.MXEIO_E1K_Connect(System.Text.Encoding.UTF8.GetBytes(IpAddr), Port, Timeout, Connection, System.Text.Encoding.UTF8.GetBytes(Password));
                    if (CheckErr(Result, "Connect " + IpAddr))
                    {
                        IsConnect = true;
                        return true;
                    }
                }
                Console.WriteLine("Контроллер {0}, невозможно соединиться", IpAddr);
                IsConnect = false;
                return false;
            }
            else
            {
                return true;
            }
        }

        // Дисконнект
        public Boolean Disconnect()
        {
            if (IsConnect)
            {
                Int32 Result = MXIO_CS.MXEIO_Disconnect(Connection[0]);
                MXIO_CS.MXEIO_Exit();
                IsConnect = false;
                return CheckErr(Result, "Disconnect " + IpAddr);
            }
            else
            {
                return true;
            }
        }

        // Проверка коннекта
        //void CheckConnect()
        //{
        //    byte[] bytCheckStatus = new byte[1];
        //    Int32 Result = MXIO_CS.MXEIO_CheckConnection(Connection[0], Timeout, bytCheckStatus);
        //    if (CheckErr(Result, "CheckConnection " + IpAddr))
        //    {
        //        switch (bytCheckStatus[0])
        //        {
        //            case MXIO_CS.CHECK_CONNECTION_OK:
        //                Console.WriteLine("MXEIO_CheckConnection: Check connection ok => {0}", bytCheckStatus[0]);
        //                break;
        //            case MXIO_CS.CHECK_CONNECTION_FAIL:
        //                Console.WriteLine("MXEIO_CheckConnection: Check connection fail => {0}", bytCheckStatus[0]);
        //                break;
        //            case MXIO_CS.CHECK_CONNECTION_TIME_OUT:
        //                Console.WriteLine("MXEIO_CheckConnection: Check connection time out => {0}", bytCheckStatus[0]);
        //                break;
        //            default:
        //                Console.WriteLine("MXEIO_CheckConnection: Check connection status unknown => {0}", bytCheckStatus[0]);
        //                break;
        //        }
        //    }
        //}

        void CheckSensors(object source, ElapsedEventArgs e)
        {
            if (SensorList.Count != 0)
            {
                Boolean[] arr = CheckInputs();
                if (arr != null)
                {                    
                    foreach (Sensor b in SensorList)
                    {
                        if (b.GetNormalState() == arr[b.GetPort()])
                        {
                            Console.WriteLine("Порт {0}, Статус: ОК", b.GetPort());
                        }
                        else
                        {
                            Console.WriteLine("Порт {0}, Статус: Авария!", b.GetPort());
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Ошибка проверки датчиков!");
                }
            }
            else
            {
                Console.WriteLine("К контроллеру не подключены датчики!");
            }
            
        }

        // Проверка датчиков
        Boolean[] CheckInputs()
        {
            Boolean[] arr = null;
            if (IsConnect)
            {
                Int32 dwShiftValue;
                UInt32 i;
                UInt32[] dwGetDIValue = new UInt32[1];
                Int32 Result = MXIO_CS.E1K_DI_Reads(Connection[0], 0, 16, dwGetDIValue);
                if (CheckErr(Result, "Чтение выходов " + IpAddr))
                {
                    arr = new Boolean[16];
                    for (i = 0, dwShiftValue = 0; i < 16; i++, dwShiftValue++)
                        arr[i] = ((dwGetDIValue[0] & (1 << dwShiftValue)) == 0) ? false : true;
                    return arr;
                }
                else
                {
                    // Не смогли прочитать порты
                    var handler = FailCheckInputsEvent;
                    if (handler != null)
                        handler(this, IpAddr);

                    CheckInputsTimer.Stop();
                    IsConnect = false;
                    Console.WriteLine("Невозможно прочитать статусы выходов");
                    ReconnectTimer.Start();

                    return arr;
                }
            }
            return arr;
        }

        // Вывод ошибок
        static Boolean CheckErr(int iRet, string functionName)
        {
            Console.WriteLine("Функция \"{0}\". Сообщение : {1}", functionName, Enum.GetName(typeof(MXIO_CS.MXIO_ErrorCode), iRet));
            if (iRet == MXIO_CS.MXIO_OK)
                return true;
            else
                return false;
        }

        public String GetIPAddress()
        {
            return IpAddr;
        }

        void Reсonnect(object source, ElapsedEventArgs e)
        {
            if (Connect())
            {
                ReconnectTimer.Stop();
                CheckInputsTimer.Start();
            }
            
        }

        public void AddSensor(Sensor s)
        {
            SensorList.Add(s);
        }
    }
}
