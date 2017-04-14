using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TypedRpc.Client
{
	// Creates the client model in runtime.
	public class ModelBuilderRuntime : IModelBuilder
	{
		// Model under construction.
		private Model Model;

		// Found interfaces.
		private List<Type> InterfacesTypes = new List<Type>();

		// Builds model.
		public Model BuildModel()
		{
			// Builds model.
			Model = new Model();
			Model.Handlers = TypedRpcHandler.Handlers.Select(h => BuildHandler(h.GetType())).ToArray();
			BuildInterfaces(Model);

			return Model;
		}

		// Builds a model type.
		private MType BuildMType(Type type)
		{
			// Declarations
			MType mType;

			// New type.
			mType = new MType();
			mType.Name = type.Name.Split('`').First();
			mType.FullName = type.FullName ?? type.Name;
			mType.FullName = mType.FullName.Split('`').First();

			// Checks type type.
			if (type.IsArray) mType.Type = MType.MTType.Array;
			else if (mType.FullName.StartsWith("System.Collections.Generic.List")) mType.Type = MType.MTType.List;
			else if (mType.FullName.StartsWith("System.Threading.Tasks.Task")) mType.Type = MType.MTType.Task;
			else if (mType.FullName.StartsWith("System.Collections.Generic.Dictionary")) mType.Type = MType.MTType.Dictionary;
			else if (mType.FullName.StartsWith("Microsoft.Owin.IOwinContext")) mType.Type = MType.MTType.OwinContext;
			else if (mType.FullName.StartsWith("System")) mType.Type = MType.MTType.System;
			else if (mType.FullName == ("JsonRpc.JsonError")) mType.Type = MType.MTType.Ignore;
			else if (type.IsGenericParameter) mType.Type = MType.MTType.Generic;
			else mType.Type = MType.MTType.Custom;

			// If array.
			if (mType.Type == MType.MTType.Array)
			{
				// Adds its type.
				mType.GenericTypes = new MType[] { BuildMType(type.GetElementType()) };
			}

			// If List or Task.
			if (mType.Type == MType.MTType.List || mType.Type == MType.MTType.Task)
			{
				if (type.GenericTypeArguments.Any()) mType.GenericTypes = new MType[] { BuildMType(type.GenericTypeArguments[0]) };
			}

			// If custom.
			if (mType.Type == MType.MTType.Custom)
			{
				// Finds its generics.
				mType.GenericTypes = type.GenericTypeArguments.Select(t => BuildMType(t)).ToArray();

				// Checks if type has already been added.
				if (!InterfacesTypes.Any(it => it.Name == type.Name))
				{
					// Adds interface.
					InterfacesTypes.Add(type);
				}
			}

			return mType;
		}
		
		// Checks if a type is a handler.
		private bool IsHandler(Type type)
		{
			return type.IsDefined(typeof(TypedRpcHandler), true);
		}

		// Builds a handler.
		private Handler BuildHandler(Type type)
		{
			Handler handler = new Handler()
			{
				Name = type.Name.Split('`').First(),
				Methods = type.GetMethods().Where(m => IsHandler(m.DeclaringType)).Select(m => BuildMethod(m)).ToArray()
			};

			return handler;
		}

		// Builds a method.
		private Method BuildMethod(MethodInfo methodInfo)
		{
			Parameter[] parameters = methodInfo.GetParameters()
				.Where(p => p.ParameterType != typeof(IOwinContext))
				.Select(p => BuildParameter(p))
				.ToArray();
			
			Method method = new Method()
			{
				Name = methodInfo.Name.Split('`').First(),
				ReturnType = BuildMType(methodInfo.ReturnType),
				Parameters = parameters
			};

			return method;
		}

		// Builds a method parameter.
		private Parameter BuildParameter(ParameterInfo parameterInfo)
		{
			Parameter parameter = new Parameter()
			{
				Name = parameterInfo.Name,
				Type = BuildMType(parameterInfo.ParameterType),
				IsOptional = parameterInfo.IsOptional
			};

			return parameter;
		}

		// Builds the interfaces.
		private void BuildInterfaces(Model model)
		{
			// Declarations
			List<Interface> interfaces;
			List<AEnum> enums;
			int index;

			// Initializations
			interfaces = new List<Interface>();
			enums = new List<AEnum>();
			index = 0;

			// The code generation might find new interfaces, so we use the 'while' loop to check for updated 'Count'.
			while (index < InterfacesTypes.Count)
			{
				// Builds the interface.
				if (InterfacesTypes[index].IsClass) interfaces.Add(BuildInterface(InterfacesTypes[index]));
				else if (InterfacesTypes[index].IsEnum) enums.Add(BuildEnum(InterfacesTypes[index]));

				// Next interface.
				index++;
			}

			// All interfaces generated.
			InterfacesTypes.Clear();

			model.Interfaces = interfaces.ToArray();
			model.Enums = enums.ToArray();
		}
		
		// Builds an enum.
		private AEnum BuildEnum(Type mType)
		{
			EnumValue[] enumValues = BuildEnumValues(mType);

			AEnum aEnum = new AEnum
			{
				Name = mType.Name,
				Values = enumValues
			};

			return aEnum;
		}

		// Builds enum values.
		private EnumValue[] BuildEnumValues(Type mType)
		{
			List<EnumValue> enumValues = new List<EnumValue>();
			
			for (int i = 0; i < mType.GetEnumNames().Length; i++)
			{
				string name = mType.GetEnumNames()[i];
				object value = mType.GetEnumValues().GetValue(i);
				value = Convert.ChangeType(value, mType.GetEnumUnderlyingType());
				
				enumValues.Add(new EnumValue()
				{
					Name = name,
					Value = value.ToString()
				});
			}

			return enumValues.ToArray();
		}

		// Builds an interface.
		private Interface BuildInterface(Type type)
		{
			// Checks if type has generics.
			if (type.IsGenericType) type = type.GetGenericTypeDefinition();

			Interface theInterface = new Interface
			{
				Name = type.Name.Split('`').First(),
				Properties = type.GetProperties().Select(p => BuildProperty(p)).ToArray()
			};
			
			return theInterface;
		}

		// Builds a property.
		private Property BuildProperty(PropertyInfo propertyInfo)
		{
			Type type = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;

			Property property = new Property()
			{
				Name = propertyInfo.Name,
				Type = BuildMType(type)
			};

			return property;
		}
	}
}