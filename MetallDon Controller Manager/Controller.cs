using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MOXA_CSharp_MXIO;

namespace MetallDon_Controller_Manager
{
    class Controller
    {
        int ret;
        Int32[] hConnection = new Int32[1];
        public string IPAddr;
        ushort port = 502;
        string Password = "";
        UInt32 Timeout = 5000;
        public DateTime lastUpdatetime;
        
        public Timer timer = new Timer();

        public String status = "";
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="ip">IP-адрес контроллера</param>
        /// <param name="pswrd">Пароль</param>
        /// <param name="timeout">Время таймаута, мс</param>
        /// <param name="ping">Интервал опроса, мс</param>
        /// <param name="updateTime">Время последнего обновления параметров из базы</param>
        public Controller(String ip, String pswrd, UInt32 timeout, Int32 ping, DateTime updateTime)
        {
            ret = MXIO_CS.MXEIO_Init();
            CheckErr(ret, "MXEIO_Init");
            timer.Interval = ping;
            IPAddr = ip;
            Password = pswrd;
            Timeout = timeout;
            lastUpdatetime = updateTime;

            timer.Elapsed += new ElapsedEventHandler(checkSensor);

        }

        // Коннект
        public Boolean Connect() {
            Console.WriteLine("Подключение к IP={0}, Порт={1} Таймаут={2}, Пароль={3}", IPAddr, port, Timeout, Password);
            ret = MXIO_CS.MXEIO_E1K_Connect(System.Text.Encoding.UTF8.GetBytes(IPAddr), port, Timeout, hConnection, System.Text.Encoding.UTF8.GetBytes(Password));
            return CheckErr(ret, "Connect");
        }

        // Дисконнект
        public Boolean Disconnect()
        {
            ret = MXIO_CS.MXEIO_Disconnect(hConnection[0]);
            MXIO_CS.MXEIO_Exit();
            return CheckErr(ret, "Disconnect");
        }

        // Проверка коннекта
        public void CheckConnect()
        {
            byte[] bytCheckStatus = new byte[1];
            ret = MXIO_CS.MXEIO_CheckConnection(hConnection[0], Timeout, bytCheckStatus);
            if (CheckErr(ret, "CheckConnection"))
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
        public void checkSensor(object source, ElapsedEventArgs e)
        {
            
            Int32 dwShiftValue;
            UInt32 i;
            UInt32[] dwGetDIValue = new UInt32[1];
            ret = MXIO_CS.E1K_DI_Reads(hConnection[0], 0, 16, dwGetDIValue);
            if (CheckErr(ret, "DI_Reads"))
            {
                for (i = 0, dwShiftValue = 0; i < 16; i++, dwShiftValue++)
                    Console.WriteLine("Выход: ch[{0}] = {1}", i + 0, ((dwGetDIValue[0] & (1 << dwShiftValue)) == 0) ? "OFF" : "ON");
            } else {
                //timer.Stop();
                // Не смогли прочитать порты
                Console.WriteLine("Невозможно прочитать статусы выходов");
            }
        }

        public static Boolean CheckErr(int iRet, string functionName)
        {
            Console.WriteLine("Функция \"{0}\". Сообщение : {1}\n", functionName, Enum.GetName(typeof(MXIO_CS.MXIO_ErrorCode), iRet));
            if (iRet == MXIO_CS.MXIO_OK)
                return true;
            else
                return false;
        }
    }
}
