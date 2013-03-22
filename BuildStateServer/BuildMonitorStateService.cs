using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace BuildStateServer
{
    partial class BuildMonitorStateService : ServiceBase
    {
        public BuildMonitorStateService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ServerApplication.Start();
        }

        protected override void OnStop()
        {
            ServerApplication.Stop();
        }

        protected override void OnContinue()
        {
            ServerApplication.Start();
        }
    }
}
