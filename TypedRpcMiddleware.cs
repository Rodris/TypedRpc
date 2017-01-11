using JsonRpc;
using Microsoft.Owin;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TypedRpc
{
	// Server main class.
	public class TypedRpcMiddleware : OwinMiddleware
    {
        // Constructor
        public TypedRpcMiddleware(OwinMiddleware next)
            : base(next)
        {
        }
        
        // Handles requests.
        public override async Task Invoke(IOwinContext context)
        {
            // Declarations
            JsonResponse jResponse;

            // Invokes method.
            jResponse = await InvokeMethod(context);
            
            // Sends response.
            context.Response.ContentType = "application/json; charset=utf-8";
            JsonSerializer.Serialize(context.Response.Body, jResponse);
        }

        // Handles requests.
        public virtual async Task<JsonResponse> InvokeMethod(IOwinContext context)
        {
            // Declarations
            JsonRequest jRequest;
            JsonResponse jResponse;
            object handler;
            MethodInfo methodInfo;
            object[] parameters;
            object result;

            // Initializations
            jRequest = null;
            handler = null;
            methodInfo = null;
            parameters = null;

            // Extracts request.
            jRequest = JsonSerializer.Deserialize<JsonRequest>(context.Request.Body);

            // Validates message.
            if (jRequest == null) return MountError(null, JsonError.ERROR_PARSE);

            // Validates request.
            if (jRequest.Method == null) return MountError(jRequest.Id, JsonError.ERROR_INVALID_REQUEST);
            
            // Searches and validates handle.
            GetMethod(jRequest.Method, out handler, out methodInfo);

            // Validates handler and method.
            if (handler == null || methodInfo == null) return MountError(jRequest.Id, JsonError.ERROR_METHOD_NOT_FOUND);
            
            // Extracts parameters.
            if (jRequest.Params is JArray) parameters = ExtractParametersAsArray(context, methodInfo.GetParameters(), jRequest.Params as JArray);

            // Validates para meters.
            if (parameters == null) return MountError(jRequest.Id, JsonError.ERROR_INVALID_PARAMS);
            
            try
            {
                // Invokes method.
                result = InvokeMethod(handler, methodInfo, parameters);

                // Checks if result is async.
                if (result is Task)
                {
                    // Awaits method.
                    await (Task)result;

                    // Retrieves result value.
                    result = result.GetType().GetProperty("Result").GetValue(result);

                    // Checks if it is void.
                    if (result.ToString() == "System.Threading.Tasks.VoidTaskResult") result = null;
                }

                // Initializes response.
                jResponse = new JsonResponse();
                jResponse.Id = jRequest.Id;

                // Checks if result is error.
                if (result is JsonError) jResponse.Error = (JsonError)result;
                else jResponse.Result = result;

                return jResponse;
            }
            catch (Exception exception)
            {
                // Handles error.
                return MountError(jRequest.Id, JsonError.ERROR_INTERNAL);
            }
        }

        // Invokes a method.
        protected virtual object InvokeMethod(object handler, MethodInfo methodInfo, object[] parameters)
        {
            // Invokes method directly.
            return methodInfo.Invoke(handler, parameters);
        }

        // Creates an error response.
        protected JsonResponse MountError(Object id, JsonError error)
        {
            return new JsonResponse()
            {
                Id = id,
                Error = error
            };
        }

        // Retrieves a method and its handler.
        private void GetMethod(String fullMethodName, out Object handler, out MethodInfo methodInfo)
        {
            // Declarations
            String methodName;

            // Initializations
            methodInfo = null;

            // Looks for handler in project.
            handler = TypedRpcHandler.Handlers.FirstOrDefault(h => fullMethodName.StartsWith(h.GetType().Name));

            // Validates handler.
            if (handler != null)
            {
                // Searches method.
                methodName = fullMethodName.Substring(handler.GetType().Name.Length + 1);
                methodInfo = handler.GetType().GetMethod(methodName);
            }
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