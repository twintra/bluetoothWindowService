using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothService.Libs
{
    internal class MessageCommand
    {


        public static string DETECT_NEARBY_DEVICES = "detectNearbyDevices";
        public static string DISCONNECT = "disconnect";


        public static List<string> commandsList()
        {
            List<string> commands = new List<string>();
            commands.Add(DETECT_NEARBY_DEVICES);
            commands.Add(DISCONNECT);
            return commands;


        }

    }
}
