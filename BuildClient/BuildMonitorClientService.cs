using System;
using System.ServiceProcess;

namespace BuildClient
{
    partial class BuildMonitorClientService : ServiceBase
    {
        private readonly ClientApplication _clientApplication = null;

        public BuildMonitorClientService()
        {
            InitializeComponent();

            _clientApplication=new ClientApplication();
        }

        protected override void OnStart(string[] args)
        {
            _clientApplication.Start();
        }

        protected override void OnStop()
        {
            _clientApplication.Stop();
        }

        protected override void OnContinue()
        {
            _clientApplication.Start();
        }

    }
}