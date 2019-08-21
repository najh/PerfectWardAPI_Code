namespace PwTaskService
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
            this.processInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.Installer = new System.ServiceProcess.ServiceInstaller();
            // 
            // processInstaller
            // 
            this.processInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.processInstaller.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.Installer});
            this.processInstaller.Password = null;
            this.processInstaller.Username = null;
            // 
            // Installer
            // 
            this.Installer.Description = "Perfect Ward API Connector Service";
            this.Installer.DisplayName = "Perfect Ward Connector";
            this.Installer.ServiceName = "PwTask";
            this.Installer.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.processInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller processInstaller;
        private System.ServiceProcess.ServiceInstaller Installer;
    }
}