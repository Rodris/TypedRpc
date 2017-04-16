using System.Runtime.Serialization;

namespace JsonRpc
{
	// Error in server.
	[DataContract]
	public class JsonError
	{
		// Error codes.
		public static readonly JsonError
			ERROR_PARSE = new JsonError() { Code = -32700, Message = "Invalid JSON was received by the server." },
			ERROR_INVALID_REQUEST = new JsonError() { Code = -32600, Message = "The JSON sent is not a valid Request object." },
			ERROR_METHOD_NOT_FOUND = new JsonError() { Code = -32601, Message = "The method does not exist / is not available." },
			ERROR_INVALID_PARAMS = new JsonError() { Code = -32602, Message = "Invalid method parameter(s)." },
			ERROR_INTERNAL = new JsonError() { Code = -32603, Message = "Internal JSON-RPC error." };
		
		// Default errors.
		public static readonly JsonError[] ERRORS = new JsonError[]
		{
			ERROR_PARSE,
			ERROR_INVALID_REQUEST,
			ERROR_METHOD_NOT_FOUND,
			ERROR_INVALID_PARAMS,
			ERROR_INTERNAL
		};

		[DataMember(Name = "code")]
		public int Code { get; set; }

		[DataMember(Name = "message")]
		public string Message { get; set; }

		[DataMember(Name = "data")]
		public object Data { get; set; }

		// Implicit constructor for code.
		public static implicit operator JsonError(int code)
		{
			return new JsonError() { Code = code };
		}

		// Implicit constructor for message.
		public static implicit operator JsonError(string message)
		{
			return new JsonError() { Message = message };
		}
	}

}
