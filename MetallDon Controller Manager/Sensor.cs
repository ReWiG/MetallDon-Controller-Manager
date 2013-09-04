using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetallDon_Controller_Manager
{
    class Sensor
    {
        Int32 Port;
        Boolean NormalState;
        Boolean State;

        public Sensor(Int32 port, Boolean normState, Boolean state)
        {
            Port = port;
            NormalState = normState;
            State = state;
        }

        public Int32 GetPort() 
        {
            return Port;
        }

        public Boolean GetState()
        {
            return State;
        }

        public Boolean GetNormalState()
        {
            return NormalState;
        }
    }
}
