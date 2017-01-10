namespace TypedRpc.Client
{
	// Interface to build client code.
	public interface IClientBuilder
	{
		// Builds the client code.
		string BuildClient(Model model);
	}

	// Interface that build a client model.
	public interface IModelBuilder
	{
		Model BuildModel();
	}

	// Model for the Rpc Handlers.
	public class Model
	{
		public Handler[] Handlers { get; set; }
		public Interface[] Interfaces { get; set; }
	}

	// Class that represents a TypedRpcHandler.
	public class Handler
	{
		public string Name { get; set; }
		public Method[] Methods { get; set; }
	}

	// Methods in a handler.
	public class Method
	{
		public string Name { get; set; }
		public MType ReturnType { get; set; }
		public Parameter[] Parameters { get; set; }
	}

	// Parameters in a method.
	public class Parameter
	{
		public string Name { get; set; }
		public MType Type { get; set; }
		public bool IsOptional { get; set; }
	}

	// Custom interface.
	public class Interface
	{
		public string Name { get; set; }
		public Property[] Properties { get; set; }
	}

	// Properties in interface.
	public class Property
	{
		public string Name { get; set; }
		public MType Type { get; set; }
	}

	// A type in model.
	public class MType
	{
		// Model Type types.
		public enum MTType
		{
			Custom,
			OwinContext,
			System,
			Array,
			List,
			Dictionary,
			Task
		}

		public string Name { get; set; }
		public string FullName { get; set; }
		public MTType Type { get; set; }
		public MType GenericType { get; set; }
	}
}
