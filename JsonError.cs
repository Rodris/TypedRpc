using System.Runtime.Serialization;

namespace JsonRpc
{
    // Error in server.
    [DataContract]
    public class JsonError
    {
        // Error codes.
        public const int
            ERROR_PARSE = -32700,
            ERROR_INVALID_REQUEST = -32600,
            ERROR_METHOD_NOT_FOUND = -32601,
            ERROR_INVALID_PARAMS = -32602,
            ERROR_INTERNAL = -32603;

        [DataMember(Name = "code")]
        public int Code { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "data")]
        public object Data { get; set; }

        // Implicit constructor.
        public static implicit operator JsonError(int code) {
            return new JsonError() { Code = code };
        }
    }

}
