using JsonRpc;
using Microsoft.Owin;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TypedRpc.Security;

namespace TypedRpc
{
	// Server main class.
	public class TypedRpcMiddleware : OwinMiddleware
    {
		// The options.
		private TypedRpcOptions Options;

        // Constructor
        public TypedRpcMiddleware(OwinMiddleware next, TypedRpcOptions options)
            : base(next)
        {
			// Initializations
			Options = options ?? new TypedRpcOptions();
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
			string data;
			JsonError error;
            JsonRequest jRequest;
            JsonResponse jResponse;
            object handler;
            MethodInfo methodInfo;
            object[] parameters;
            object result;

			// Initializations
			data = null;
			jRequest = null;

			try
			{
				// Retrieves method.
				error = GetMethod(context, out data, out jRequest, out handler, out methodInfo, out parameters);

				// Checks for any error.
				if (error != null) return MountError(jRequest == null ? null : jRequest.Id, error);

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

				// Checks if is a wrapped result.
				if (result != null && result.GetType().IsGenericType && result.GetType().GetGenericTypeDefinition() == typeof(TypedRpcReturn<>))
				{
					// Checks if result is error.
					jResponse.Error = result.GetType().GetProperty("Error").GetValue(result) as JsonError;
					if (jResponse.Error == null)
					{
						// Retrieves result value.
						jResponse.Result = result.GetType().GetProperty("Data").GetValue(result);
					}
				}
				else jResponse.Result = result;

				return jResponse;
			}
			catch (Exception exception)
			{
				// Declarations
				TypedRpcException typedRpcException;

				// Looks for a TypedRpcExceptions.
				if (TypedRpcException.FindInChain(exception, out typedRpcException)) return MountError(jRequest.Id, typedRpcException.Error);

				// Triggers exception event.
				Options.OnCatch?.Invoke(data, jRequest, context, exception);

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

		private JsonError GetMethod(IOwinContext context, out string data, out JsonRequest jRequest,
			out Object handler, out MethodInfo methodInfo, out Object[] parameters)
		{
			// Declarations
			JsonError error;
			string fullMethodName;
			string methodName;

			// Initializations
			data = null;
			jRequest = null;
			handler = null;
			methodInfo = null;
			parameters = null;

			// Extracts data.
			data = new StreamReader(context.Request.Body).ReadToEnd();

			// Builds request.
			jRequest = JsonSerializer.Deserialize<JsonRequest>(data);

			// Validates message.
			if (jRequest == null) return JsonError.ERROR_PARSE;

			// Validates request.
			if (jRequest.Method == null) return JsonError.ERROR_INVALID_REQUEST;

			// Looks for handler in project.
			fullMethodName = jRequest.Method;
			handler = TypedRpcHandler.Handlers.FirstOrDefault(h => fullMethodName.StartsWith(h.GetType().Name));

			// Validates handler.
			if (handler == null) return JsonError.ERROR_METHOD_NOT_FOUND;

			// Searches method.
			methodName = fullMethodName.Substring(handler.GetType().Name.Length + 1);
			methodInfo = handler.GetType().GetMethod(methodName);

			// Validates method.
			if (methodInfo == null) return JsonError.ERROR_METHOD_NOT_FOUND;

			// Extracts parameters.
			if (jRequest.Params is JArray) parameters = ExtractParametersAsArray(context, methodInfo.GetParameters(), jRequest.Params as JArray);

			// Validates parameters.
			if (parameters == null) return JsonError.ERROR_INVALID_PARAMS;

			// Authorizes user.
			if (!Authorization.Authorized(context, methodInfo, Options.DefaultAuthorizationRequired, out error)) return error;

			return null;
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