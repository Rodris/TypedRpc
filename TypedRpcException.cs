using JsonRpc;
using System;

namespace TypedRpc
{
	// Error in server.
	public class TypedRpcException : Exception
	{
		// The error.
		public JsonError Error { get; private set; }

		// Constructor
		public TypedRpcException(string message, string data = null) : this(0, message, data)
		{

		}

		// Constructor
		public TypedRpcException(int code, string message, string data = null) : base(message)
		{
			// Initialization
			Error = new JsonError()
			{
				Code = code,
				Message = message,
				Data = data
			};
		}

		// Searches for a TypedRpcException in an exception chain.
		internal static bool FindInChain(Exception exception, out TypedRpcException typedRpcException)
		{
			// Initializations
			typedRpcException = null;

			// Looks for TypedRpc Exceptions.
			while (exception != null && !(exception is TypedRpcException))
			{
				// Keeps searching.
				exception = exception.InnerException;
			}

			return (typedRpcException != null);
		}
	}

}
