using JsonRpc;
using Microsoft.Owin;
using Newtonsoft.Json.Linq;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TypedRpc
{
    // Server main class.
    public class TypedRpcServer : OwinMiddleware
    {
        // Serializer
        private JsonSerializer Serializer = new JsonSerializer();

        // Maps RpcServer in OWIN.
        public static void Map(IAppBuilder app)
        {
            app.Map("/rpc", appBuilder => appBuilder.Use<TypedRpcServer>(new object[0]));
        }

        // Available handlers.
        private List<Object> Handlers = new List<Object>();

        // Constructor
        public TypedRpcServer(OwinMiddleware next)
            : base(next)
        {
            // Initializations
            Init();
        }

        // Finds all handlers in project.
        private void Init()
        {
            // Declarations
            List<Type> types;
            Type handlerType;
            ConstructorInfo constructor;

            handlerType = typeof(TypedRpcHandler);
            types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => !p.IsAbstract && Attribute.IsDefined(p, handlerType))
                .ToList();

            // Creates handles.
            foreach (Type hType in types)
            {
                // Searches for default constructor.
                constructor = hType.GetConstructor(System.Type.EmptyTypes);

                // Validates constructor.
                if (constructor == null) throw new Exception(String.Format("{0} has no default constructor.", hType.FullName));

                // Creates handle.
                Handlers.Add(constructor.Invoke(null));
            }
        }

        // Retrieves a method and its handler.
        private void GetMethod(String fullMethodName, out Object handler, out MethodInfo methodInfo)
        {
            // Declarations
            String methodName;

            // Initializations
            methodInfo = null;

            // Looks for handler in project.
            handler = Handlers.FirstOrDefault(h => fullMethodName.StartsWith(h.GetType().Name));

            // Validates handler.
            if (handler != null)
            {
                // Searches method.
                methodName = fullMethodName.Substring(handler.GetType().Name.Length + 1);
                methodInfo = handler.GetType().GetMethod(methodName);
            }
        }

        // Handles requests.
        public override Task Invoke(IOwinContext context)
        {
            // Runs task.
            return Task.Run(() =>
            {
                // Declarations
                JsonRequest jRequest;
                JsonResponse jResponse;
                Object handler;
                MethodInfo methodInfo;
                Object[] parameters;

                // Initializations
                jRequest = null;
                jResponse = null;
                handler = null;
                methodInfo = null;
                parameters = null;

                // Extracts request.
                jRequest = Serializer.DeserializeJsonRequest(context.Request.Body);

                // Validates request.
                if (jRequest == null)
                {
                    // Parse error.
                    jResponse = MountError(null, JsonError.ERROR_PARSE);
                }
                else if (jRequest.Method == null)
                {
                    // Invalid request.
                    jResponse = MountError(jRequest.Id, JsonError.ERROR_INVALID_REQUEST);
                }
                else
                {
                    // Searches and validates handle.
                    GetMethod(jRequest.Method, out handler, out methodInfo);

                    // Validates handler and method.
                    if (handler == null || methodInfo == null)
                    {
                        // Method not found.
                        jResponse = MountError(jRequest.Id, JsonError.ERROR_METHOD_NOT_FOUND);
                    }
                    else
                    {
                        // Extracts parameters.
                        if (jRequest.Params is JArray) parameters = ExtractParametersAsArray(context, methodInfo.GetParameters(), jRequest.Params as JArray);

                        // Validates para meters.
                        if (parameters == null)
                        {
                            // Invalid params.
                            jResponse = MountError(jRequest.Id, JsonError.ERROR_INVALID_PARAMS);
                        }
                        else
                        {
                            try
                            {
                                // Invokes method.
                                jResponse = new JsonResponse()
                                {
                                    Id = jRequest.Id,
                                    Result = methodInfo.Invoke(handler, parameters)
                                };
                            }
                            catch (Exception)
                            {
                                // Handles error.
                                jResponse = MountError(jRequest.Id, JsonError.ERROR_INTERNAL);
                            }
                        }
                    }
                }

                // Sends response.
                context.Response.ContentType = "application/json; charset=utf-8";
                Serializer.SerializeJsonResponse(context.Response.Body, jResponse);
            });
        }

        // Creates an error response.
        private JsonResponse MountError(Object id, int errorCode)
        {
            return new JsonResponse()
            {
                Id = id,
                Error = errorCode
            };
        }
        
        // Extracts parameters as array.
        private Object[] ExtractParametersAsArray(IOwinContext context, ParameterInfo[] paramsInfo, JArray paramsReceived)
        {
            // Declarations
            Object[] parameters;
            int paramCount;
            int paramReceivedCount;

            // Initializations
            parameters = new Object[paramsInfo.Length];
            paramCount = 0;
            paramReceivedCount = 0;

            // Extracts all parameters.
            foreach (ParameterInfo paramInfo in paramsInfo)
            {
                // Checks if param is the context.
                if (paramInfo.ParameterType == typeof(IOwinContext))
                {
                    parameters[paramCount] = context;
                    paramCount++;
                }

                // Checks if parameter is available.
                else if (paramReceivedCount < paramsReceived.Count)
                {
                    parameters[paramCount] = paramsReceived[paramReceivedCount].ToObject(paramInfo.ParameterType);
                    paramCount++;
                    paramReceivedCount++;
                }

                // Checks if parameter is optional.
                else if (paramInfo.IsOptional)
                {
                    parameters[paramCount] = Type.Missing;
                    paramCount++;
                }

                else
                {
                    // Wrong number of parameters.
                    parameters = null;
                    break;
                }
            }

            return parameters;
        }
    }

}