namespace DoctorApplication
{
    internal class DoctorActions
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