using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;

namespace BluetoothService.Controllers
{
    internal class BluetoothController
    {

        #region Private members
        /// <summary>
        /// Bluetooth watcher
        /// </summary>
        private readonly BluetoothLEAdvertisementWatcher btWatcher;

        /// <summary>
        /// Dictionary of discovered devices
        /// </summary>
        private readonly Dictionary<ulong, BLEDevice> discoveredDevices = new Dictionary<ulong, BLEDevice>();

        /// <summary>
        /// Thread locker of this class
        /// </summary>
        private readonly object threadLock = new object();
        #endregion

        #region Public properties
        /// <summary>
        /// Indicator if watcher is scanning for the bluetooth device;
        /// </summary>
        public bool Scanning => btWatcher.Status == BluetoothLEAdvertisementWatcherStatus.Started;

        public IReadOnlyCollection<BLEDevice> DiscoveredDevices
        {
            get
            {
                lock (threadLock)
                {
                    return discoveredDevices.Values.ToList().AsReadOnly();
                }
            }
        }

        #endregion

        #region Public events
        /// <summary>
        /// Fire this event when stop listening
        /// </summary>
        public event Action OnStoppedListening = () => { };

        /// <summary>
        /// Fire this event when start listening
        /// </summary>
        public event Action OnStartListening = () => { };

        /// <summary>
        /// Fire this event when new device discovered
        /// </summary>
        public event Action<BLEDevice> OnNewDeviceDiscovered = (newDevice) => { };

        /// <summary>
        /// Fire this event when device discovered
        /// </summary>
        public event Action<BLEDevice> OnDeviceDiscovered = (newDevice) => { };

        public event Action<BLEDevice, string, string> OnDeviceNameChanged = (device, oldName, newName) => { };




        #endregion

        public BluetoothController()
        {
            btWatcher = new BluetoothLEAdvertisementWatcher()
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };
            btWatcher.Received += OnReceived;
            btWatcher.Stopped += OnStopped;
        }

        #region Private methods
        private void OnStopped(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementWatcherStoppedEventArgs args)
        {
            Console.WriteLine("Scan stopped");
            discoveredDevices.Clear();
            OnStoppedListening();
        }

        private void OnReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            BLEDevice device;

            bool isNewDevice = !discoveredDevices.ContainsKey(args.BluetoothAddress);

            bool nameChanged;
            string oldName;
            string newName = args.Advertisement.LocalName;
            if (string.IsNullOrEmpty(newName)) return;
            if (!isNewDevice)
            {
                oldName = discoveredDevices[args.BluetoothAddress].Name;
                nameChanged = !isNewDevice && string.IsNullOrEmpty(args.Advertisement.LocalName) && oldName != newName;

            }
            else
            {
                oldName = newName;
                nameChanged = false;
            }

            lock (threadLock)
            {
                string name = args.Advertisement.LocalName;
                ulong address = args.BluetoothAddress;
                device = new BLEDevice
                    (
                        address: address,
                        name: name,
                        rssi: args.RawSignalStrengthInDBm,
                        broadcastTime: args.Timestamp
                    );
                discoveredDevices[address] = device;
            }
            OnDeviceDiscovered(device);

            if (nameChanged) OnDeviceNameChanged(device, oldName, newName);

            if (isNewDevice) OnNewDeviceDiscovered(device);
        }

        #endregion

        #region Public method
        public void StartScan()
        {
            if (Scanning) return;
            btWatcher.Start();
            OnStartListening();
            (new Thread(() =>
            {

                btWatcher.Start();
                OnStartListening();
                Thread.Sleep(20000);
                btWatcher.Stop();
            })).Start();


        }

        public void StopScan()
        {
            if (!Scanning) return;
            btWatcher.Stop();

        }

        public async void Connect(ulong deviceBtAddress)
        {
            BluetoothLEDevice bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(deviceBtAddress);
        }

        #endregion

    }

}
