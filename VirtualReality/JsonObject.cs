using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualReality
{
    class JsonObject
    {
		private delegate void dataCallback(dynamic data);
		private static readonly Dictionary<string, dataCallback> callbacks = new();

		private Program program;

        public JsonObject(Program program)
        {
            this.program = program;
        }

        /// <summary>Handle handles<c>the id and data that is being received.</c>It searches for the callback
        /// in the dictionary and exeutes that callback with the given data.</summary>
        ///

        public static void Handle(string id, dynamic data)
		{
			callbacks[id](data);
		}

		/// <summary>Send sends<c>the given id and data as a JSON object string.</c>The id and data get serialized into
		/// an JSON object and than turned into a byte array. This byte array is than send to the server.</summary>
		///

		public void Send(string id, dynamic data)
		{
            JObject jObject = new()
            {
                { "id", id },
                { "data", new JObject(data) }
            };
            program.SendViaTunnel(jObject);
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
