using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothService.Libs
{
    internal class MessageUtil
    {
        public static string Status(string sts)
        {
            Dictionary<string, dynamic> json = new Dictionary<string, dynamic>
            {
                { "type", "status" },
                { "data", sts }
            };
            return DictionaryToJson(json);
        }
        public static string MonitoringStatus(string sts)
        {
            Dictionary<string, dynamic> json = new Dictionary<string, dynamic>
            {
                { "type", "monitoring" },
                { "data", sts }
            };
            return DictionaryToJson(json);
        }
        public static string Devices(string[] devices)
        {
            Dictionary<string, dynamic> json = new Dictionary<string, dynamic>
            {
                { "type", "devices" },
                { "data", "[" + string.Join(",", devices.ToArray()) + "]" }
            };
            return DictionaryToJson(json);
        }
        public static string Message(string msg)
        {
            Dictionary<string, dynamic> json = new Dictionary<string, dynamic>
            {
                { "type", "message" },
                { "data", msg }
            };
            return DictionaryToJson(json);
        }
        public static string Data(Dictionary<string, dynamic> data)
        {
            Dictionary<string, dynamic> json = new Dictionary<string, dynamic>
            {
                { "type", "data" },
                { "data", data }
            };
            return DictionaryToJson(json);
        }
        public static string Error(string msg)
        {
            Dictionary<string, dynamic> json = new Dictionary<string, dynamic>
            {
                { "type", "error" },
                { "data", msg }
            };
            return DictionaryToJson(json);
        }


        public static string DictionaryToJson(Dictionary<string, dynamic> data)
        {
            var entries = data.Select(d =>
            {
                if (d.Value.GetType() == typeof(Dictionary<string, dynamic>))
                {
                    return string.Format("\"{0}\": {1}", d.Key, DictionaryToJson(d.Value));
                }
                else if (d.Value.GetType() == typeof(bool))
                {
                    return string.Format("\"{0}\": {1}", d.Key, d.Value);
                }
                else if (d.Value.GetType() == typeof(int))
                {
                    return string.Format("\"{0}\": {1}", d.Key, d.Value);
                }
                else
                {
                    return string.Format("\"{0}\": \"{1}\"", d.Key, string.Join(",", GetUnicodeValue(d.Value)));
                }

            });
            return "{" + string.Join(",", entries) + "}";
        }

        private static string GetUnicodeValue(string value)
        {
            if (Encoding.UTF8.GetByteCount(value) != value.Length)
            {
                string result = "";
                _ = Encoding.UTF8.GetBytes(value);
                for (int i = 0; i < value.Length; i++)
                {
                    result += string.Format("\\u{0:x4}", (int)value[i]);
                }
                return result;
            }
            else return value;
        }
    }

}
