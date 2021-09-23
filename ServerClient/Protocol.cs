using System.Collections.Generic;

namespace ServerClient
{
    class Protocol
    {
        private static string DictToString(Dictionary<string, string> data, string separator)
        {
            string result = (char)data.Count + "";
            foreach (var item in data)
            {
                result += item.Key + separator + item.Value + separator;
            }
            return result;
        }

        private static (Dictionary<string, string>, string) StringToDict(string dataString, string separator)
        {
            while (dataString.StartsWith("\r\n")) dataString = dataString[2..];
            int length = (byte)dataString[0];
            Dictionary<string, string> resultData = new();
            string rawData = dataString[1..];
            for (int i = 0; i < length; i++)
            {
                string dataKey = rawData.Substring(0, rawData.IndexOf(separator));
                rawData = rawData[(rawData.IndexOf(separator) + separator.Length)..];
                string dataValue = rawData.Contains(separator) ? rawData.Substring(0, rawData.IndexOf(separator)) : rawData;
                rawData = rawData.Contains(separator) ? rawData[(rawData.IndexOf(separator) + separator.Length)..] : rawData;
                resultData.Add(dataKey, dataValue);
            }
            return (resultData, rawData);
        }

        public static string StringifyHeaders(Dictionary<string, string> headers) => DictToString(headers, "\r\n");

        public static (Dictionary<string, string>, string) ParseHeaders(string headersString) => StringToDict(headersString, "\r\n");

        public static string StringifyData(Dictionary<string, string> data) => DictToString(data, "\r\n\r\n");

        public static Dictionary<string, string> ParseData(string dataString) => StringToDict(dataString, "\r\n\r\n").Item1;

        public static (Dictionary<string, string>, Dictionary<string, string>) ParsePacket(string packetData)
        {
            (Dictionary<string, string> headers, string rawdata) = ParseHeaders(packetData);
            Dictionary<string, string> data = ParseData(rawdata);

            return (headers, data);
        }
    }
}
