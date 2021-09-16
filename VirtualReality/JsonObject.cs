using Newtonsoft.Json;
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

		public static void Send(string id, dynamic data)
		{
			byte[] payload = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { id = id, data = data }));
	        // TODO[Jeroen] Actualy send the payload.
		}

		/// <summary>Init does<c>the initialisation of all the callbacks that are used.</c>The callbacks are added to
		/// a dictionary with the JSON object id as the key to easily process the data.</summary>
		///

		private static void Init()
		{
			callbacks["route/node/update"] = (data) =>
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
