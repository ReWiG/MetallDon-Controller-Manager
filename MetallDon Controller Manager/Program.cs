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
        public static void CheckErr(int iRet, string szFunctionName)
        {
            Console.WriteLine("Function \"{0}\". Return Message : {1}\n", szFunctionName, Enum.GetName(typeof(MXIO_CS.MXIO_ErrorCode), iRet));
            if (iRet == MXIO_CS.EIO_TIME_OUT || iRet == MXIO_CS.HANDLE_ERROR)
            {
                //To terminates use of the socket
                MXIO_CS.MXEIO_Exit();
                Console.WriteLine("Press any key to close application\r\n");
                Console.ReadLine();
                Environment.Exit(1);
            }
        }

        static void Main(string[] args)
        {
            Controller contr = new Controller("192.168.127.254","",3000,20,new DateTime());
            ////Console.WriteLine(MetallDon_Controller_Manager.Properties.Settings.Default.ipDB);
            //int iii = MXIO_CS.MXEIO_Init();
            //CheckErr(iii, "init");

            //int ret;
            //Int32[] hConnection = new Int32[1];
            //string IPAddr = "192.168.127.254";
            //string Password = "";
            //UInt32 Timeout = 5000;

            ////Connect to ioLogik device
            //Console.WriteLine("MXEIO_E1K_Connect IP={0}, Timeout={1}, Password={2}", IPAddr, Timeout, Password);
            //ret = MXIO_CS.MXEIO_E1K_Connect(System.Text.Encoding.UTF8.GetBytes(IPAddr), 502, Timeout, hConnection, System.Text.Encoding.UTF8.GetBytes(Password));
            //CheckErr(ret, "MXEIO_E1K_Connect");
            //if (ret == MXIO_CS.MXIO_OK)
            //    Console.WriteLine("MXEIO_E1K_Connect Success.");
            ////------------------------------------------------------------------------
            //byte bytCount = 16;
            //byte bytStartChannel = 0;
            //Int32 dwShiftValue;
            //UInt32 i;
            //UInt32[] dwGetDIValue = new UInt32[1];
            //ret = MXIO_CS.E1K_DI_Reads(hConnection[0], bytStartChannel, bytCount, dwGetDIValue);
            //CheckErr(ret, "E1K_DI_Reads");
            //if (ret == MXIO_CS.MXIO_OK)
            //{
            //    Console.WriteLine("E1K_DI_Reads Get Ch0~ch3 DI Direction DI Mode DI Value success.");
            //    for (i = 0, dwShiftValue = 0; i < bytCount; i++, dwShiftValue++)
            //        Console.WriteLine("DI vlaue: ch[{0}] = {1}", i + bytStartChannel, ((dwGetDIValue[0] & (1 << dwShiftValue)) == 0) ? "OFF" : "ON");
            //}
            ////--------------------------------------------------------------------------
            ////End Application
            //ret = MXIO_CS.MXEIO_Disconnect(hConnection[0]);
            //CheckErr(ret, "MXEIO_E1K_Connect");
            //if (ret == MXIO_CS.MXIO_OK)
            //    Console.WriteLine("MXEIO_Disconnect return {0}", ret);
            ////--------------------------------------------------------------------------
            //MXIO_CS.MXEIO_Exit();
            //Console.WriteLine("MXEIO_Exit, Press Enter To Exit.");
            //Console.ReadLine();

            //Console.ReadKey();
        }
    }
}
