using System.Collections.Generic;
using System.Text;

namespace ServerClient
{
    public class Util
    {
        public static string StringifyClients(List<ClientHandler> clients)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (ClientHandler client in clients)
            {
                stringBuilder.Append(client.IsDoctor);
                stringBuilder.Append("|");
                stringBuilder.Append(client.authKey);
                stringBuilder.Append(";");
            }
        }
    }
}