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
                // stringBuilder.Append(client.somethingelse);
                // stringBuilder.Append("|");
                stringBuilder.Append(client.UUID+"|"+client.Name);
                stringBuilder.Append(";");
            }

            return stringBuilder.ToString();
        }
    }
}