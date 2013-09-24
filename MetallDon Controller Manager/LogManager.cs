using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MetallDon_Controller_Manager
{
    class LogManager
    {
        private static object Sync = new object();

        public static void Write(String Message, Boolean IsDebug)
        {
            switch(Properties.Settings.Default.loggingMode)
            {
                case "debug":
                    WriteFile(Message);
                    break;
                case "release":
                    if (!IsDebug)
                        WriteFile(Message);
                    break;
                default:
                    break;
            }
        }

        static void WriteFile(String Message)
        {
            try
            {
                // Путь .\\Log
                string PathToLog = Environment.CurrentDirectory;

                string FileName = Path.Combine(PathToLog, "Log.txt");
                string FullText = string.Format("[{0:dd.MM.yyy HH:mm:ss}] {1}\r\n",
                DateTime.Now, Message);
                lock (Sync)
                {
                    File.AppendAllText(FileName, FullText, Encoding.GetEncoding("UTF-8"));
                }
            }
            catch (Exception e)
            {
                // Перехватываем все и ничего не делаем
                Console.WriteLine("Ошибка логирования: " + e);
            }
        }
    }
}
