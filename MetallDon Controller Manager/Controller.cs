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
        string IPAddr;
        ushort port = 502;
        string Password = "";
        UInt32 Timeout = 5000;
        DateTime lastUpdatetime;
        Timer timer = new Timer();

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="ip">IP-адрес контроллера</param>
        /// <param name="pswrd">Пароль</param>
        /// <param name="timeout">Время таймаута, мс</param>
        /// <param name="ping">Интервал опроса, секунды</param>
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

            Console.WriteLine( Connect());
            Console.ReadKey();
        }

        public Boolean Connect() {
            Console.WriteLine("Подключение к IP={0}, Порт={1} Таймаут={2}, Пароль={3}", IPAddr, port, Timeout, Password);
            ret = MXIO_CS.MXEIO_E1K_Connect(System.Text.Encoding.UTF8.GetBytes(IPAddr), port, Timeout, hConnection, System.Text.Encoding.UTF8.GetBytes(Password));
            return CheckErr(ret, "Connect");
        }

        public static Boolean CheckErr(int iRet, string functionName)
        {
            Console.WriteLine("Функция \"{0}\". Сообщение : {1}\n", functionName, Enum.GetName(typeof(MXIO_CS.MXIO_ErrorCode), iRet));
            if (iRet == MXIO_CS.EIO_TIME_OUT || iRet == MXIO_CS.HANDLE_ERROR)
            {
                //Завершаем работу
                //MXIO_CS.MXEIO_Exit();
                //Console.WriteLine("Нажмите любую клавишу для выхода из программы\r\n");
                //Console.ReadLine();
                //Environment.Exit(1);
                return false;
            }
            return true;
        }
    }
}
