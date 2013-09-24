using System;
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
        Int32[] Connection = new Int32[1];
        Boolean IsConnect = false;
        Boolean[] StatusInputs;
        UInt64 IdAccident = 0;

        public event EventHandler<String> FailCheckInputsEvent;
        public event EventHandler<UInt64> RecoveryAccidentEvent;

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
            CheckInputsTimer.Elapsed += new ElapsedEventHandler(SetStatusInputs); // и событие
            ReconnectTimer.Elapsed += new ElapsedEventHandler(Reсonnect);          
        }

        // Коннект
        public Boolean Connect() {
            if (!IsConnect)
            {
                Console.WriteLine("Подключение к IP={0}, Порт={1} Таймаут={2}, Пароль={3}", IpAddr, Port, Timeout, Password);
                LogManager.Write(String.Format("Подключение к IP={0}, Порт={1} Таймаут={2}, Пароль={3}", IpAddr, Port, Timeout, Password), false);
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
                LogManager.Write("Контроллер " + IpAddr + ", невозможно соединиться", false);

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

        // Проверка датчиков
        void SetStatusInputs(object source, ElapsedEventArgs e)
        {
            StatusInputs = null;
            if (IsConnect)
            {
                Int32 dwShiftValue;
                UInt32 i;
                UInt32[] dwGetDIValue = new UInt32[1];
                Int32 Result = MXIO_CS.E1K_DI_Reads(Connection[0], 0, 16, dwGetDIValue);
                if (CheckErr(Result, "Чтение выходов " + IpAddr))
                {
                    StatusInputs = new Boolean[16];
                    for (i = 0, dwShiftValue = 0; i < 16; i++, dwShiftValue++)
                        StatusInputs[i] = ((dwGetDIValue[0] & (1 << dwShiftValue)) == 0) ? false : true;

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
                    // Не смогли прочитать порты
                    var handler = FailCheckInputsEvent;
                    if (handler != null)
                        handler(this, IpAddr);
                    
                    CheckInputsTimer.Stop();
                    IsConnect = false;
                    Console.WriteLine("Невозможно прочитать статусы выходов");
                    LogManager.Write("Невозможно прочитать статусы выходов", false);
                    Console.WriteLine("Ошибка проверки датчиков!");
                    LogManager.Write("Ошибка проверки датчиков!", false);
                    ReconnectTimer.Start();
                }
            }
        }

        // Вывод ошибок
        static Boolean CheckErr(int iRet, string functionName)
        {
            Console.WriteLine("Функция \"{0}\". Сообщение : {1}", functionName, Enum.GetName(typeof(MXIO_CS.MXIO_ErrorCode), iRet));
            LogManager.Write(String.Format("Функция \"{0}\". Сообщение : {1}", functionName, Enum.GetName(typeof(MXIO_CS.MXIO_ErrorCode), iRet)), true);

            if (iRet == MXIO_CS.MXIO_OK)
                return true;
            else
                return false;
        }

        public String GetIPAddress()
        {
            return IpAddr;
        }

        public Boolean[] GetStatusInputs()
        {
            return StatusInputs;
        }

        public Double GetPing()
        {
            return CheckInputsTimer.Interval;
        }

        void Reсonnect(object source, ElapsedEventArgs e)
        {
            if (Connect())
            {
                if (IdAccident != 0)
                {
                    // Время восстановления
                    var handler = RecoveryAccidentEvent;
                    if (handler != null)
                        handler(this, IdAccident);
                }

                ReconnectTimer.Stop();
                CheckInputsTimer.Start();
            }
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
