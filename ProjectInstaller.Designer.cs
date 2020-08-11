namespace RESSWATCH
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
            this.RSSWATCHProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.RSSWATCHInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // RSSWATCHProcessInstaller1
            // 
            this.RSSWATCHProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.RSSWATCHProcessInstaller1.Password = null;
            this.RSSWATCHProcessInstaller1.Username = null;
            // 
            // RSSWATCHInstaller
            // 
            this.RSSWATCHInstaller.DisplayName = "RSSWATCH";
            this.RSSWATCHInstaller.ServiceName = "RSSWATCH";
            this.RSSWATCHInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            this.RSSWATCHInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.RESSWATCHInstaller_AfterInstall);
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.RSSWATCHProcessInstaller1,
            this.RSSWATCHInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller RSSWATCHProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller RSSWATCHInstaller;
    }
}