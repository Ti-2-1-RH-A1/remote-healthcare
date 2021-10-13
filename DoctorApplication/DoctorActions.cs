namespace DoctorApplication
{
    public class DoctorActions
    {

        public static void HistoryWindow()
        {
            ClientHistoryWindow clientHistoryWindow = new ClientHistoryWindow();
            clientHistoryWindow.ShowDialog();
        }
        //public void HistoryWindow(object client)
        //{
        //    ClientHistoryWindow clientHistoryWindow = new ClientHistoryWindow();
        //    clientHistoryWindow.ShowDialog();
        //}

        //public void HistoryWindow(Client client)
        //{
        //    ClientHistoryWindow clientHistoryWindow = new ClientHistoryWindow(client.clientSerial);
        //    clientHistoryWindow.ShowDialog();
        //}
    }
}