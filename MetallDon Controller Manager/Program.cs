using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetallDon_Controller_Manager
{
    class Program
    {
        static void Main(string[] args)
        {
            LogManager.Write("Приложение запущено.", false);
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CurrentDomain_ProcessExit);

            ManagerController mc = new ManagerController();
            mc.FillSensorList();
            mc.RunningMonitoring();
            
            Console.ReadKey();
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            LogManager.Write("Приложение корректно закрыто.", false);
        }
    }
}
