using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CS_ControlArmamento_CapturaRFID;

namespace MifareDump
{
    class Program
    {

        static void Main(string[] args)
        {
            LectorHID hid = new LectorHID();
            hid.Iniciar();
            
            for (; ;)
            {
                System.Threading.Thread.Sleep(200);
            }
        }
    }
}
