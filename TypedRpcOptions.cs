using JsonRpc;
using Microsoft.Owin;
using System;

namespace TypedRpc
{
	/// <summary>
	/// Options used when mapping TypedRpc.
	/// </summary>
	public class TypedRpcOptions
	{
		/// <summary>
		/// Catches internal exceptions in TypedRpc.
		/// </summary>
		public Action<string, IOwinContext, Exception> OnCatch { get; set; }
	}
}
