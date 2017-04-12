using System.Collections.Generic;

namespace TypedRpc.Client
{
	// Interface to build client code.
	public interface IClientBuilder
	{
		// The builder type.
		string Type { get; }

		// The builder language.
		string Language { get; }

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
		public AEnum[] Enums { get; set; }

#if DEBUG
		public List<string> Debug = new List<string>();
#endif
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

	// Enum defined.
	public class AEnum
	{
		public string Name { get; set; }
		public EnumValue[] Values { get; set; }
	}

	// Enum value.
	public class EnumValue
	{
		public string Name { get; set; }
		public string Value { get; set; }
	}

	// A type in model.
	public class MType
	{
		// Model Type types.
		public enum MTType
		{
			Ignore,
			Custom,
			OwinContext,
			System,
			Array,
			List,
			Dictionary,
			Task,
			Generic
		}

		public string Name { get; set; }
		public string FullName { get; set; }
		public MTType Type { get; set; }
		public MType[] GenericTypes { get; set; }

		// Constructor
		public MType()
		{
			// Initializations
			GenericTypes = new MType[0];
		}
	}
}
