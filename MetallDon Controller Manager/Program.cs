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
            ManagerDB db = new ManagerDB("metalldon", "localhost", "metalldon", "123456");
            db.SelectControllers();
            db.mngContrl.Launch();
            //Controller contr = new Controller("192.168.127.254", "", 3000, 10000, new DateTime());

            //ManagerController m = new ManagerController();
            //m.addController(contr);
            //m.Launch();
            //m.removeController("192.168.127.254");
            
            Console.ReadKey();
            
        }
    }
}
