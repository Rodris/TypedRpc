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
		/// Set this value to true to make all methods to require authorization by default. Default is false.
		/// </summary>
		public bool DefaultAuthorizationRequired = false;

		/// <summary>
		/// Catches internal exceptions in TypedRpc.
		/// </summary>
		public Action<string, JsonRequest, IOwinContext, Exception> OnCatch { get; set; }
	}
}
