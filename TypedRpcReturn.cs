using JsonRpc;

namespace TypedRpc
{
	// A customized handler return.
	public class TypedRpcReturn<T>
	{
		// The error.
		public JsonError Error { get; private set; }

		// The return data.
		public T Data { get; set; }

		// Implicit constructor for data.
		public static implicit operator TypedRpcReturn<T>(T data)
		{
			return new TypedRpcReturn<T>()
			{
				Data = data
			};
		}

		// Implicit constructor for error.
		public static implicit operator TypedRpcReturn<T>(JsonError error)
		{
			return new TypedRpcReturn<T>()
			{
				Error = error
			};
		}
	}

}
