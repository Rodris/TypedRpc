using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace JsonRpc
{
    // JSON serializer.
    public class JsonSerializer
    {
        // Serializes a response.
        public void SerializeJsonResponse(Stream stream, JsonResponse jResponse)
        {
            // Declarations
            String data;

            // Serializes response.
            data = JsonConvert.SerializeObject(jResponse);

            // Sends to stream.
            stream.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);
        }

        // Deserializes a request.
        public JsonRequest DeserializeJsonRequest(Stream stream)
        {
            // Declarations
            JsonRequest jRequest;

            try
            {
                // Deserializes request.
                jRequest = JsonConvert.DeserializeObject<JsonRequest>(new StreamReader(stream).ReadToEnd());
            } catch (Exception)
            {
                jRequest = null;
            }
            
            return jRequest;
        }
    }

}
