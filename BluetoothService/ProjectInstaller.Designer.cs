namespace BluetoothService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.LocalSystem = new System.ServiceProcess.ServiceProcessInstaller();
            this.BluetoothService = new System.ServiceProcess.ServiceInstaller();
            // 
            // LocalSystem
            // 
            this.LocalSystem.Account = System.ServiceProcess.ServiceAccount.LocalService;
            this.LocalSystem.Password = null;
            this.LocalSystem.Username = null;
            // 
            // BluetoothService
            // 
            this.BluetoothService.Description = "The socket server service that manage the usage of bluetooth server from users\' l" +
    "ocal computer.";
            this.BluetoothService.DisplayName = "Abbot Bluetooth Service";
            this.BluetoothService.ServiceName = "BluetoothService";
            this.BluetoothService.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            this.BluetoothService.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceInstaller1_AfterInstall);
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.LocalSystem,
            this.BluetoothService});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller LocalSystem;
        private System.ServiceProcess.ServiceInstaller BluetoothService;
    }
}