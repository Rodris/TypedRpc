using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace JsonRpc
{
    // JSON serializer.
    public static class JsonSerializer
    {
        // Serializes an object.
        public static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        // Serializes an object.
        public static void Serialize(Stream stream, object value)
        {
            // Declarations
            String data;
            byte[] bytes;

            // Serializes object.
            data = JsonConvert.SerializeObject(value);
            bytes = Encoding.UTF8.GetBytes(data);

            // Sends to stream.
            stream.Write(bytes, 0, bytes.Length);
        }

        // Deserializes a request.
        public static T Deserialize<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }
    }

}
