using BluetoothService.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BluetoothService
{
    public partial class BluetoothService : ServiceBase
    {
        private readonly SocketServer server;
        public BluetoothService()
        {
            InitializeComponent();
            string ip = "127.0.0.1";
            int port = 7024;
            server = new SocketServer(ip, port);
        }

        protected override void OnStart(string[] args)
        {
            new Thread(InitServer).Start();
        }

        public void InitServer()
        {
            server.Start();

        }

        protected override void OnStop()
        {
            server.Stop();
        }
    }
}
