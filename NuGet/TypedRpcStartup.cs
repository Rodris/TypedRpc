using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[assembly: OwinStartup(typeof(TypedRpc.TypedRpcStartup))]
namespace TypedRpc
{
    /**
     * Class to initialize OWIN.
     */
    public class TypedRpcStartup
    {
        public void Configuration(IAppBuilder app)
        {
            TypedRpcServer.Map(app);
        }
    }
}
