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
            int iii = MXIO_CS.MXEIO_Init();
            Console.WriteLine("привет");
            Console.WriteLine(iii);

            Console.ReadKey();
            MXIO_CS.MXEIO_Exit();
        }
    }
}
