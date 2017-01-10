using System;
using System.Linq;

namespace TypedRpc.Client
{
	public class ClientTS : IClientBuilder
	{
		// Name of the remote method file template.
		private const string FILENAME_REMOTE_METHOD = "TypedRpc.Client.RemoteMethod.ts";

		// File template.
		private const string TEMPLATE_TS = @"
namespace TypedRpc {{
{0}
{1}
{2}
}}
";

		// Class template.
		private const string TEMPLATE_CLASS = @"
	export class {0} {{
		{1}
	}}
";

		// Method template.
		private const string TEMPLATE_METHOD = @"
		{0}({1}): RemoteMethod {{
			{2}
		}}
";

		// Interface template.
		private const string TEMPLATE_INTERFACE = @"
	export interface {0} {{
{1}
	}}
";

		// Loads the RemoteMethod.ts content.
		private String ReadRemoteMethod()
		{
			string remoteMethod = new System.IO.StreamReader(this.GetType().Assembly.GetManifestResourceStream(FILENAME_REMOTE_METHOD)).ReadToEnd();
			remoteMethod = remoteMethod.Replace("\r\n", "\r\n\t");

			return remoteMethod;
		}

		// Builds the client code.
		public string BuildClient(Model model)
		{
			string remoteMethod = ReadRemoteMethod();

			string handlers = string.Concat(model.Handlers.Select(h => BuildHandler(h)));

			string interfaces = string.Concat(model.Interfaces.Select(i => BuildInterface(i)));

			// Builds client;
			string client = string.Format(TEMPLATE_TS, remoteMethod, handlers, interfaces);

			return client;
		}

		// Returns the name of a type.
		public string GetNameType(MType mType)
		{
			string name;

			if (mType.Type == MType.MTType.System)
			{
				switch (mType.FullName)
				{
					case "System.SByte":
					case "System.Byte":
					case "System.Char":
					case "System.Decimal":
					case "System.Double":
					case "System.Single":
					case "System.Int32":
					case "System.Int64":
						name = "number"; break;

					case "System.Boolean": name = "boolean"; break;
					case "System.String": name = "string"; break;
					case "System.Void": name = "void"; break;

					default: name = "any"; break;
				}
			}
			else
			{
				name = GetNameClass(mType);
			}

			return name;
		}

		// Returns the name of a class.
		private string GetNameClass(MType mType)
		{
			// Declarations
			string name;

			// Checks type type.
			switch (mType.Type) {
				case MType.MTType.Array:
				case MType.MTType.List: name = GetNameType(mType.GenericType) + "[]"; break;
				case MType.MTType.Dictionary: name = "any"; break;
				case MType.MTType.Task: name = (mType.GenericType == null) ? "void" : GetNameType(mType.GenericType); break;

				// Custom class.
				default: name = mType.Name; break;
			}
			return name;
		}

		// Builds a handler.
		private string BuildHandler(Handler handler)
		{
			string methods = string.Concat(handler.Methods.Select(m => BuildMethod(handler, m)));

			string clazz = string.Format(TEMPLATE_CLASS,
				handler.Name,
				methods);

			return clazz;
		}

		// Builds a method.
		private string BuildMethod(Handler handler, Method method)
		{
			string parameters = string.Join(", ", method.Parameters.Select(p => BuildParameter(p)));

			string methodBody = string.Format("return RemoteMethod.call<{0}>('{1}.{2}', arguments);",
				GetNameType(method.ReturnType),
				handler.Name,
				method.Name);

			string methodText = string.Format(TEMPLATE_METHOD,
				method.Name,
				string.Join(", ", parameters),
				methodBody);

			return methodText;
		}

		// Builds a method parameter.
		private string BuildParameter(Parameter parameter)
		{
			return string.Format("{0}{1}: {2}", parameter.Name, parameter.IsOptional ? "?" : string.Empty, GetNameType(parameter.Type));
		}

		// Builds an interface.
		private string BuildInterface(Interface theInterface)
		{
			string properties = string.Join("\r\n", theInterface.Properties.Select(p => BuildProperty(p)));
			return string.Format(TEMPLATE_INTERFACE, theInterface.Name, string.Join("\r\n", properties));
		}

		// Builds a property.
		private string BuildProperty(Property property)
		{
			string propertyText = string.Format("		{0}: {1};", property.Name, GetNameType(property.Type));

			return propertyText;
		}

	}
}