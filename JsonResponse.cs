using System.Runtime.Serialization;

namespace JsonRpc
{
    // Response returned by server.
    [DataContract]
    public class JsonResponse
    {
        [DataMember(Name = "jsonrpc")]
        public string JsonRpc { get { return "2.0"; } set { } }

        [DataMember(Name = "result")]
        public object Result { get; set; }

        [DataMember(Name = "error")]
        public JsonError Error { get; set; }

        [DataMember(Name = "id")]
        public object Id { get; set; }
    }

}
