using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetallDon_Controller_Manager
{
    class ManagerController
    {
        static List<Controller> contrlList = new List<Controller>();

        // Добавить контроллер
        public void addController(Controller ctr){
            contrlList.Add(ctr);
        }

        // Удалить контроллер
        public void removeController(String ipAddress)
        {
            
            Controller ctrlr =  (Controller)contrlList.Find(s => s.IPAddr == ipAddress);
            if (ctrlr != null){
                ctrlr.timer.Stop(); // Останавливаем таймер
                if(ctrlr.Disconnect()){ // Дисконнектимся
                    Console.WriteLine("Контроллер отключён");
                }
                contrlList.Remove(ctrlr);
                Console.WriteLine("Контроллер удалён");
            } else {
                 Console.WriteLine("Контроллер с IP {0} не найден!", ipAddress);
            }
        }

        // Запуск таймера для проверки
        public void Launch()
        {
            foreach (Controller con in contrlList)
            {
                if (con.Connect())
                {
                    con.timer.Start();
                }                
            }
        }
    }
}
