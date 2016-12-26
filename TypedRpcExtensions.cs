using Owin;

namespace TypedRpc
{
    // Extensions
    public static class TypedRpcExtensions
    {
        /// <summary>
        /// Maps RpcServer in OWIN.
        /// </summary>
        public static void MapTypedRpc(this IAppBuilder app)
        {
            app.Map("/typedrpc", appBuilder => appBuilder.Use<TypedRpcMiddleware>(new object[0]));
        }
    }

}