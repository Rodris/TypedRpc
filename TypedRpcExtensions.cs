using Owin;

namespace TypedRpc
{
    // Server main class.
    public static class TypedRpcExtensions
    {
        /// <summary>
        /// Maps RpcServer in OWIN.
        /// </summary>
        public static void MapTypedRpc(this IAppBuilder app)
        {
            app.Map("/typedrpc", appBuilder => appBuilder.Use<TypedRpcServer>(new object[0]));
        }
    }

}