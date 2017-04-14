using System;
using System.Linq;

namespace TypedRpc.Client
{
	public class ClientTS : IClientBuilder
	{
		// Name of the remote method file.
		private const string FILENAME_REMOTE_METHOD = "TypedRpc.Client.ts.RemoteMethod.ts";

		// File template.
		private const string TEMPLATE_TS = @"
namespace TypedRpc {{
{0}
{1}
{2}
{3}
}}
";

		// Class template.
		private const string TEMPLATE_CLASS = @"
	export class {0} {{
		{1}
	}}
";

		// Interface template.
		private const string TEMPLATE_INTERFACE = @"
	export interface {0} {{
{1}
	}}
";

		// Enum template.
		private const string TEMPLATE_ENUM = @"
	export enum {0} {{
{1}
	}}
";

		// The builder type.
		public string Type { get { return "ts"; } }

		// The builder language.
		public string Language { get { return "TypeScript"; } }

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

			string enums = string.Concat(model.Enums.Select(e => BuildEnum(e)));

			// Builds client;
			string client = string.Format(TEMPLATE_TS, remoteMethod, handlers, enums, interfaces);

#if DEBUG
			client += string.Join("\r\n", model.Debug);
#endif

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
				case MType.MTType.List: name = GetNameType(mType.GenericTypes[0]) + "[]"; break;
				case MType.MTType.Dictionary: name = "any"; break;
				case MType.MTType.Task: name = mType.GenericTypes.Any() ? GetNameType(mType.GenericTypes[0]) : "void"; break;
				
				// Custom class.
				default:
					name = string.Empty;
					if (mType.GenericTypes.Length > 0)
					{
						name = string.Join(", ", mType.GenericTypes.Select(gt => GetNameType(gt)));
						name = string.Format("<{0}>", name);
					}
					name = mType.Name + name;
					break;
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

		// Method template.
		private const string TEMPLATE_METHOD_FUNC = @"
		{1}({2}): RemoteFunc<{3}> {{
			return RemoteMethod.callFunc<{3}>('{0}.{1}', arguments);
		}}
";

		private const string TEMPLATE_METHOD_ACTION = @"
		{1}({2}): RemoteAction {{
			return RemoteMethod.callAction('{0}.{1}', arguments);
		}}
";

		// Builds a method.
		private string BuildMethod(Handler handler, Method method)
		{
			string parameters = string.Join(", ", method.Parameters.Select(p => BuildParameter(p)));
			string returnType = GetNameType(method.ReturnType);
			string templateMethod = (returnType == "void") ? TEMPLATE_METHOD_ACTION : TEMPLATE_METHOD_FUNC;

			string methodText = string.Format(templateMethod,
				handler.Name,
				method.Name,
				string.Join(", ", parameters),
				returnType);

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
			// Declarations
			string generics;

			generics = string.Join(", ", theInterface.Properties.Where(p => p.Type.Type == MType.MTType.Generic).Select(p => p.Type.Name));
			if (!string.IsNullOrEmpty(generics)) generics = string.Format("<{0}>", generics);

			string properties = string.Join("\r\n", theInterface.Properties.Select(p => BuildProperty(p)));

			return string.Format(TEMPLATE_INTERFACE, theInterface.Name + generics, string.Join("\r\n", properties));
		}

		// Builds a property.
		private string BuildProperty(Property property)
		{
			string propertyText = string.Format("		{0}: {1};", property.Name, GetNameType(property.Type));

			return propertyText;
		}

		// Builds an enum.
		private string BuildEnum(AEnum aEnum)
		{
			string values = string.Join(",\r\n", aEnum.Values.Select(ev => BuildEnumValue(ev)));
			return string.Format(TEMPLATE_ENUM, aEnum.Name, string.Join("\r\n", values));
		}

		// Builds an enum value.
		private string BuildEnumValue(EnumValue enumValue)
		{
			string enumValueText;

			if (string.IsNullOrWhiteSpace(enumValue.Value)) enumValueText = string.Format("		{0}", enumValue.Name);
			else enumValueText = string.Format("		{0} = {1}", enumValue.Name, enumValue.Value);

			return enumValueText;
		}

	}
}