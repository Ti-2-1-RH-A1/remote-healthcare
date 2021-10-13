using System;

namespace RemoteHealthcare.Hrm
{
    class HRMManager
    {
        private HRM hrm;

        public HRMManager(IServiceProvider services)
        {
            this.hrm = new HRM(services);
        }

        public void StartHRM()
        {
            hrm.Start();
        }
    }
}
