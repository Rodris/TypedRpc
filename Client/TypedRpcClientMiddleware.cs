using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TypedRpc.Client
{
	// Server main class.
	public class TypedRpcClientMiddleware : OwinMiddleware
    {
		// Available client builders.
		private static IClientBuilder[] ClientBuilders = FindClientBuilders();

		// Default instructions.
		private static string DEFAULT_INSTRUCTIONS = CreateDefaultInstructions();

		// Finds all client builders in project.
		private static IClientBuilder[] FindClientBuilders()
		{
			// Declarations
			List<IClientBuilder> builders;
			List<Type> types;
			Type builderType;
			ConstructorInfo constructor;

			// Initializations
			builders = new List<IClientBuilder>();
			builderType = typeof(IClientBuilder);

			types = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => !p.IsAbstract && builderType.IsAssignableFrom(p))
				.ToList();

			// Creates builders.
			foreach (Type hType in types)
			{
				// Searches for default constructor.
				constructor = hType.GetConstructor(System.Type.EmptyTypes);

				// Validates constructor.
				if (constructor == null) throw new Exception(String.Format("{0} has no default constructor.", hType.FullName));

				// Creates builder.
				builders.Add((IClientBuilder)constructor.Invoke(null));
			}

			return builders.ToArray();
		}

		// Creates default instructions.
		private static string CreateDefaultInstructions()
		{
			string instructions = "List of available clients and their URLs:\r\n";

			instructions += String.Join("\r\n",
				ClientBuilders.Select(cb => String.Format("{0}: /typedrpc/client/{1}", cb.Language, cb.Type)));

			return instructions;
		}

		// Constructor
		public TypedRpcClientMiddleware(OwinMiddleware next)
            : base(next)
        {
        }
        
        // Handles requests.
        public override async Task Invoke(IOwinContext context)
        {
			// Declarations
			IClientBuilder builder;
			Model model;
			string type;
			string result;

			// Makes method asynchronous.
			await Task.Yield();

			// Initializations
			result = DEFAULT_INSTRUCTIONS;

			// Checks client type.
			if (context.Request.Path.HasValue)
			{
				// Retrieves type.
				type = context.Request.Path.Value.Substring(1);

				// Validates builder.
				builder = ClientBuilders.FirstOrDefault(cb => cb.Type == type);
				if (builder != null)
				{
					// Builds model.
					model = new ModelBuilderRuntime().BuildModel();

					// Builds client.
					result = builder.BuildClient(model);
				}
			}

			// Sends result.
			context.Response.ContentType = "text/plain; charset=utf-8";
			context.Response.Write(result);
		}
	}

}