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
            ManagerDBandControllers db = new ManagerDBandControllers(Properties.Settings.Default.nameDB,
                Properties.Settings.Default.ipDB,
                Properties.Settings.Default.userDB,
                Properties.Settings.Default.passDB);
            db.SelectControllers();
            db.RunningMonitoring();
            
            Console.ReadKey();
            
        }
    }
}
