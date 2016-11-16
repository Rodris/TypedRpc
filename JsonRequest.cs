using System.Runtime.Serialization;

namespace JsonRpc
{
    // Request handled by server.
    [DataContract]
    public class JsonRequest
    {
        [DataMember(Name = "jsonrpc")]
        public string JsonRpc { get { return "2.0"; } set { } }

        [DataMember(Name = "method")]
        public string Method { get; set; }

        [DataMember(Name = "params")]
        public object Params { get; set; }

        [DataMember(Name = "id")]
        public object Id { get; set; }
    }

}
