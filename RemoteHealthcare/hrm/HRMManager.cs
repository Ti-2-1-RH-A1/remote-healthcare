using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteHealthcare.hrm
{
    class HRMManager
    {
        private readonly IServiceProvider services;
        private HRM hrm;

        public HRMManager(IServiceProvider services)
        {
            this.services = services;
            this.hrm = new HRM(services);
        }

        public void StartHRM()
        {
            hrm.Start();
        }
    }
}
