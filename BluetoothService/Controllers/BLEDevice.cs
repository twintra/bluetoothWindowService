using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BluetoothService.Controllers
{
    internal class BLEDevice
    {
        #region Property
        public DateTimeOffset BroadcastTime { get; }
        public ulong Address { get; }
        public string Name { get; }
        public short SignalStrength { get; }

        public string MACAddress
        {
            get
            {
                string tempMac = Address.ToString("X");
                string regex = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})";
                string replace = "$1:$2:$3:$4:$5:$6";
                string macAddress = Regex.Replace(tempMac, regex, replace);
                return macAddress;
            }
        }
        #endregion


        #region Constructor
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="address"></param>
        /// <param name="name"></param>
        /// <param name="rssi"></param>
        /// <param name="broadcastTime"></param>
        public BLEDevice(ulong address, string name, short rssi, DateTimeOffset broadcastTime)
        {
            Address = address;
            Name = name;
            SignalStrength = rssi;
            BroadcastTime = broadcastTime;
        }

        #endregion

        /// <summary>
        /// User friendly ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $" {(string.IsNullOrEmpty(Name) ? "[No name]" : Name)} {Address} {MACAddress} {SignalStrength}";
        }
    }

}
