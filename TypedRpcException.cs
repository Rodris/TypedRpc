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
		public static TypedRpcException FindInChain(Exception exception)
		{
			// Looks for TypedRpc Exceptions.
			while (exception != null && !(exception is TypedRpcException))
			{
				// Keeps searching.
				exception = exception.InnerException;
			}

			return exception as TypedRpcException;
		}
	}

}
