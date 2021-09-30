using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualReality
{
    /// <summary>
    /// Possible class to use in case Json usage has to be refactored again
    /// </summary>
    class JsonObject
    {
        private delegate void dataCallback(dynamic data);

        private static readonly Dictionary<string, dataCallback> callbacks = new Dictionary<string, dataCallback>();

        private Connection connection;

        public JsonObject(Connection connection)
        {
            this.connection = connection;
        }

        /// <summary>Handle handles<c>the id and data that is being received.</c>It searches for the callback
        /// in the dictionary and exeutes that callback with the given data.</summary>
        ///
        public static void Handle(string id, dynamic data)
        {
            callbacks[id](data);
        }

        /// <summary>Send sends<c>the given id and data as a JSON object string.</c>It calls the SendViaTunnel function
        /// of the VRManager class giving it the id and date. That function than sends that on to the server.</summary>
        ///
        public void Send(string id, dynamic data)
        {
            connection.SendViaTunnel(new JObject()
            {
                {"id", id},
                {"data", new JObject(data)}
            });
        }

        /// <summary>Init does<c>the initialisation of all the callbacks that are used.</c>The callbacks are added to
        /// a dictionary with the JSON object id as the key to easily process the data.</summary>
        ///
        private static void Init()
        {
            callbacks[JsonID.SCENE_NODE_UPDATE] = (data) =>
            {
                if (data.status != "ok")
                {
                    // TODO[Jeroen] Add implementation.
                }
            };

            // TODO[Jeroen] Implement the rest of the callbacks.
        }
    }
}