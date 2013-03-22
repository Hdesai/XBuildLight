using System.ServiceProcess;

namespace BuildClient
{
    partial class BuildMonitorClientService : ServiceBase
    {
        public BuildMonitorClientService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ClientApplication.Start();
        }

        protected override void OnStop()
        {
            ClientApplication.Stop();
        }

        protected override void OnContinue()
        {
            ClientApplication.Start();
        }
    }
}