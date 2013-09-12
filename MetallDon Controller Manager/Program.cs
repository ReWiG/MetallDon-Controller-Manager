using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MOXA_CSharp_MXIO;

namespace MetallDon_Controller_Manager
{
    class Program
    {
        static void Main(string[] args)
        {
            ManagerController mc = new ManagerController();
            mc.FillSensorList();
            mc.RunningMonitoring();
            
            Console.ReadKey();
            
        }
    }
}
