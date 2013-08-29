using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MOXA_CSharp_MXIO;

namespace MetallDon_Controller_Manager
{
    class ControllerMOXA
    {
        const ushort Port = 502;
        const UInt32 Timeout = 2000;

        String IPAddr;
        String Password;
        Int32 ReturnController;
        Int32[] Connection = new Int32[1];
        Boolean isConnect = false;
        String DateAccident = "";        
        public delegate void FooDelegate(String ip);
        FooDelegate callback;

        public Timer Timer = new Timer();
        public Timer ReConnectTimer = new Timer(10000);

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="ip">IP-адрес контроллера</param>
        /// <param name="pswrd">Пароль</param>
        /// <param name="timeout">Время таймаута, мс</param>
        /// <param name="ping">Интервал опроса, мс</param>
        public ControllerMOXA(String ip, String pswrd, Int32 ping, FooDelegate cb)
        {
            ReturnController = MXIO_CS.MXEIO_Init();
            CheckErr(ReturnController, "MXEIO_Init"); // Проверяем инициализацию
            IPAddr = ip;
            Password = pswrd;
            Timer.Interval = ping; // устанавливаем интервал пинга
            Timer.Elapsed += new ElapsedEventHandler(CheckSensor); // и событие
            ReConnectTimer.Elapsed += new ElapsedEventHandler(ReConnect);
            callback = cb;
        }

        // Коннект
        public Boolean Connect() {
            Console.WriteLine("Подключение к IP={0}, Порт={1} Таймаут={2}, Пароль={3}", IPAddr, Port, Timeout, Password);
            for (int i = 3; i > 0; i--)
            {
                ReturnController = MXIO_CS.MXEIO_E1K_Connect(System.Text.Encoding.UTF8.GetBytes(IPAddr), Port, Timeout, Connection, System.Text.Encoding.UTF8.GetBytes(Password));
                if (CheckErr(ReturnController, "Connect " + IPAddr))
                {
                    isConnect = true;
                    return true;
                }
            }
            Console.WriteLine("Контроллер {0}, невозможно соединиться", IPAddr);
            isConnect = false;
            return false;
        }

        // Дисконнект
        public Boolean Disconnect()
        {
            ReturnController = MXIO_CS.MXEIO_Disconnect(Connection[0]);
            MXIO_CS.MXEIO_Exit();
            isConnect = false;
            return CheckErr(ReturnController, "Disconnect " + IPAddr);
        }

        // Проверка коннекта
        public void CheckConnect()
        {
            byte[] bytCheckStatus = new byte[1];
            ReturnController = MXIO_CS.MXEIO_CheckConnection(Connection[0], Timeout, bytCheckStatus);
            if (CheckErr(ReturnController, "CheckConnection " + IPAddr))
            {
                switch (bytCheckStatus[0])
                {
                    case MXIO_CS.CHECK_CONNECTION_OK:
                        Console.WriteLine("MXEIO_CheckConnection: Check connection ok => {0}", bytCheckStatus[0]);
                        break;
                    case MXIO_CS.CHECK_CONNECTION_FAIL:
                        Console.WriteLine("MXEIO_CheckConnection: Check connection fail => {0}", bytCheckStatus[0]);
                        break;
                    case MXIO_CS.CHECK_CONNECTION_TIME_OUT:
                        Console.WriteLine("MXEIO_CheckConnection: Check connection time out => {0}", bytCheckStatus[0]);
                        break;
                    default:
                        Console.WriteLine("MXEIO_CheckConnection: Check connection status unknown => {0}", bytCheckStatus[0]);
                        break;
                }
            }
        }

        // Проверка датчиков
        public void CheckSensor(object source, ElapsedEventArgs e)
        {
            if (isConnected())
            {
                Int32 dwShiftValue;
                UInt32 i;
                UInt32[] dwGetDIValue = new UInt32[1];
                ReturnController = MXIO_CS.E1K_DI_Reads(Connection[0], 0, 16, dwGetDIValue);
                if (CheckErr(ReturnController, "Чтение выходов " + IPAddr))
                {
                    for (i = 0, dwShiftValue = 0; i < 16; i++, dwShiftValue++)
                        Console.WriteLine("Выход: ch[{0}] = {1}", i + 0, ((dwGetDIValue[0] & (1 << dwShiftValue)) == 0) ? "OFF" : "ON");
                }
                else
                {
                    // Не смогли прочитать порты
                    callback(IPAddr);
                    Timer.Stop();
                    isConnect = false;
                    Console.WriteLine("Невозможно прочитать статусы выходов");
                    ReConnectTimer.Start();
                }
            }
        }

        // Вывод ошибок
        public static Boolean CheckErr(int iRet, string functionName)
        {
            Console.WriteLine("Функция \"{0}\". Сообщение : {1}", functionName, Enum.GetName(typeof(MXIO_CS.MXIO_ErrorCode), iRet));
            if (iRet == MXIO_CS.MXIO_OK)
                return true;
            else
                return false;
        }

        // Проверка коннекта
        public Boolean isConnected()
        {
            return isConnect;
        }

        public String GetIPAddress()
        {
            return IPAddr;
        }

        public void SetDateAccident(String date)
        {
            DateAccident = date;
        }

        public void ReConnect(object source, ElapsedEventArgs e)
        {
            if (!isConnected())
            {
                Connect();
            }
            else
            {
                ReConnectTimer.Stop();
                Timer.Start();
            }
            
        }
    }
}
