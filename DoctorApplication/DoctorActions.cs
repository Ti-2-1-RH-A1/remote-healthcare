using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorApplication
{
    class DoctorActions
    {
        private ClientManager clientManager;

        public DoctorActions()
        {
            clientManager = new ClientManager();
        }

        public async Task start()
        { 
            await clientManager.start();
        } 



    }
}