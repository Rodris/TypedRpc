using Owin;
using TypedRpc.Client;

namespace TypedRpc
{
    // Extensions
    public static class TypedRpcExtensions
    {
        /// <summary>
        /// Maps RpcServer in OWIN.
        /// </summary>
        public static void MapTypedRpc(this IAppBuilder app, TypedRpcOptions options)
        {
			app.Map("/typedrpc/client", appBuilder => appBuilder.Use<TypedRpcClientMiddleware>(new object[0]));
			app.Map("/typedrpc", appBuilder => appBuilder.Use<TypedRpcMiddleware>(new object[] { options }));
        }
    }

}