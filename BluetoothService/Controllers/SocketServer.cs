using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BluetoothService.Libs;

namespace BluetoothService.Controllers
{
    internal class SocketServer
    {
        private readonly SocketClient clientController;
        private readonly BluetoothController btController;
        private TcpListener server;
        //
        private readonly string ip;
        private readonly int port;
        public SocketServer(String ip, int port)
        {
            this.ip = ip;
            this.port = port;
            server = new TcpListener(IPAddress.Parse(ip), port);
            clientController = new SocketClient();
            btController = new BluetoothController();

            btController.OnNewDeviceDiscovered += (device) =>
            {
                //Console.WriteLine($"New discovered device => {device}");
                var discoveredDevices = btController.DiscoveredDevices;
                List<Dictionary<string, string>> dataList = new List<Dictionary<string, string>>();
                foreach (var e in discoveredDevices)
                {
                    Dictionary<string, string> data = new Dictionary<string, string>()
                    {
                        { "name", e.Name},
                        { "macAddress", e.MACAddress }
                    };
                    dataList.Add(data
                        );
                }
                Dictionary<string, dynamic> payload = new Dictionary<string, dynamic>()
                {
                    { "type", "data" },
                    { "data", dataList }
                };
                string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(dataList);
                clientController.SendMessageToClient(jsonString);

            };

            btController.OnStartListening += () =>
            {
                clientController.SendMessage("Started Scanning.");
            };
            btController.OnStoppedListening += () =>
            {
                clientController.SendMessage("Stopped Scanning.");
            };


            //btController.OnDeviceDiscovered += (device) =>
            //{
            //    //Console.WriteLine($"Discovered device => {device}");

            //    clientController.SendMessage(device.ToString());
            //};

        }

        public void Start()
        {
            Console.WriteLine("Initiating server");
            server.Start();
            Console.WriteLine($"Server has started on {ip}:{port}");
            while (true)
            {
                try
                {
                    Console.WriteLine("Waiting for a new connection...");
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("New client connected.");
                    clientController.AddClient(client);
                    Thread thread = new Thread(HandleClient);
                    thread.Start();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    clientController.Close();
                }
            }
        }

        private void HandleClient()
        {
            while (true)
            {
                if (!clientController.ClientAvailable) return;
                string message = clientController.Read();
                if (message == null) continue;
                HandleClientMessage(message);
            }
        }

        private void HandleClientMessage(String message)
        {
            // request data command

            if (message == MessageCommand.DETECT_NEARBY_DEVICES)
            {

                (new Thread(() =>
                {
                    Console.WriteLine("==> Client request scan device.");
                    if (btController.Scanning)
                    {
                        btController.StopScan();
                        Thread.Sleep(1000);
                    }
                    btController.StartScan();

                })).Start();

        }
            else if (message == MessageCommand.DISCONNECT)
            {
                Console.WriteLine("==> Client request disconnect.");
                clientController.Close();
                Console.WriteLine("==> Client disconnected.");

            }
            else
            {
                Console.WriteLine($"other command => {message}");
                clientController.SendMessage(message);

                //Console.WriteLine("Not supported command `{0}`.", message);
                //Console.WriteLine("Not supported request ==> `" + message + "` | Try one of these commands: " + String.Join(", ", MessageCommand.commandsList()));
            }




        }

        public void Stop()
        {
            clientController.Close();
            server.Stop();

        }
    }
}
