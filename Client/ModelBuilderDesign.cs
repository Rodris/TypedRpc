using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypedRpc.Client
{
	// Creates the client model in design.
	public class ModelBuilderDesign : IModelBuilder
	{
		// DTE
		private EnvDTE.DTE Dte;

		// Current project.
		private EnvDTE.Project CurrentProject;

		// Found interfaces.
		private List<EnvDTE.CodeType> Interfaces = new List<EnvDTE.CodeType>();

		// A void type.
		private static readonly MType TypeVoid = new MType()
		{
			Name = "Void",
			FullName = "System.Void",
			Type = MType.MTType.System
		};
		
		// Constructor
		public ModelBuilderDesign(EnvDTE.DTE dte)
		{
			// Initializations
			Dte = dte;
		}

		// Builds model.
		public Model BuildModel()
		{
			Model model = new Model();
			model.Handlers = BuildHandlers();
			BuildInterfaces(model);

			return model;
		}

		// Returns a type from its name.
		private EnvDTE.CodeType GetCodeType(string name)
		{
			// Declarations
			EnvDTE.CodeType codeType;

			// Tries current project.
			codeType = CurrentProject.CodeModel.CodeTypeFromFullName(name);

			// Validates code type.
			if (codeType == null)
			{
				// Searches in all available projects.
				foreach (EnvDTE.Project project in Dte.Solution.Projects)
				{
					// Tries the project.
					codeType = project.CodeModel.CodeTypeFromFullName(name);

					// Found it?
					if (codeType != null) break;
				}
			}

			return codeType;
		}

		// Builds a model type.
		private MType BuildMType(EnvDTE.CodeType codeType)
		{
			// Declarations
			MType mType;

			// Validates type.
			if (codeType == null) return TypeVoid;
			
			// New type.
			mType = new MType();
			mType.Name = codeType.Name;
			mType.FullName = codeType.FullName;
			if (mType.FullName.EndsWith("?")) mType.FullName = mType.FullName.Substring(0, mType.FullName.Length - 1);

			// Checks type type.
			if (codeType.FullName.EndsWith("[]")) mType.Type = MType.MTType.Array;
			else if (codeType.FullName.StartsWith("System.Collections.Generic.List")) mType.Type = MType.MTType.List;
			else if (codeType.FullName.StartsWith("System.Threading.Tasks.Task")) mType.Type = MType.MTType.Task;
			else if (codeType.FullName.StartsWith("System.Collections.Generic.Dictionary")) mType.Type = MType.MTType.Dictionary;
			else if (codeType.FullName.StartsWith("Microsoft.Owin.IOwinContext")) mType.Type = MType.MTType.OwinContext;
			else if (codeType.FullName.StartsWith("System")) mType.Type = MType.MTType.System;
			else mType.Type = MType.MTType.Custom;

			// If array.
			if (mType.Type == MType.MTType.Array)
			{
				// Adds its type.
				string codeTypeName = codeType.FullName.Substring(0, codeType.FullName.Length - 2);
				mType.GenericType = BuildMType(GetCodeType(codeTypeName));
			}

			// If List or Task.
			if (mType.Type == MType.MTType.List || mType.Type == MType.MTType.Task)
			{
				mType.GenericType = BuildMType(GetGeneric(codeType.FullName));
			}

			// If custom.
			if (mType.Type == MType.MTType.Custom)
			{
				// Checks if type has not been added.
				if (!Interfaces.Any(it => it.FullName == codeType.FullName))
				{
					// Adds interface.
					Interfaces.Add(codeType);
				}
			}

			return mType;
		}

		// Returns the generic type.
		public EnvDTE.CodeType GetGeneric(string genericType)
		{
			int startIndex = genericType.IndexOf('<');
			int endIndex = genericType.LastIndexOf('>');

			if (startIndex < 0) return null;

			genericType = genericType.Substring(startIndex + 1, (endIndex - startIndex - 1));
			EnvDTE.CodeType codeType = GetCodeType(genericType);

			return codeType;
		}

		// Builds handlers.
		private Handler[] BuildHandlers()
		{
			List<Handler> handlers = new List<Handler>();

			foreach (EnvDTE.Project prj in Dte.Solution.Projects)
			{
				if (prj.CodeModel == null) continue;
				CurrentProject = prj;

				foreach (EnvDTE.CodeNamespace element in prj.CodeModel.CodeElements)
				{
					if (element.Name == "System") continue;
					if (element.Name == "Owin") continue;
					if (element.Name == "Newtonsoft") continue;
					if (element.Name == "MS") continue;
					if (element.Name == "Microsoft") continue;

					// Builds servers.
					handlers.AddRange(FindHandlers(element).Select(c => BuildHandler(c)));
				}
			}

			return handlers.ToArray();
		}

		// Finds handler classes.
		public List<EnvDTE.CodeClass> FindHandlers(EnvDTE.CodeNamespace container)
		{
			List<EnvDTE.CodeClass> handlers = new List<EnvDTE.CodeClass>();

			foreach (EnvDTE.CodeElement element in container.Members)
			{
				if (element.Kind == EnvDTE.vsCMElement.vsCMElementClass)
				{
					EnvDTE.CodeClass codeClass = (EnvDTE.CodeClass)element;
					if (!codeClass.IsAbstract && IsHandler(codeClass)) handlers.Add(codeClass);
				}
				else if (element.Kind == EnvDTE.vsCMElement.vsCMElementNamespace)
				{
					handlers.AddRange(FindHandlers((EnvDTE.CodeNamespace)element));
				}
			}

			return handlers;
		}

		// Checks if a class is a handler.
		public bool IsHandler(EnvDTE.CodeClass codeClass)
		{
			// Checks for handler attribute.
			foreach (EnvDTE.CodeAttribute attr in codeClass.Attributes) if (attr.FullName == "TypedRpc.TypedRpcHandler") return true;

			// Checks if any parent is handler.
			foreach (EnvDTE.CodeClass parentClass in codeClass.Bases)
			{
				if (IsHandler(parentClass)) return true;
			}

			return false;
		}

		// Builds a handler.
		private Handler BuildHandler(EnvDTE.CodeClass codeClass)
		{
			Method[] methods = BuildMethods(codeClass);

			Handler handler = new Handler()
			{
				Name = codeClass.Name,
				Methods = methods
			};

			return handler;
		}

		// Builds methods.
		public Method[] BuildMethods(EnvDTE.CodeClass codeClass)
		{
			List<Method> methods = new List<Method>();

			if (IsHandler(codeClass))
			{
				foreach (EnvDTE.CodeElement method in codeClass.Members)
				{
					if (method.Kind == EnvDTE.vsCMElement.vsCMElementFunction && method.Name != codeClass.Name)
					{
						if (((EnvDTE.CodeFunction)method).Access == EnvDTE.vsCMAccess.vsCMAccessPublic)
						{
							methods.Add(BuildMethod((EnvDTE.CodeFunction)method));
						}
					}
				}

				foreach (EnvDTE.CodeClass baseClass in codeClass.Bases)
				{
					methods.AddRange(BuildMethods(baseClass));
				}
			}

			return methods.ToArray();
		}

		// Builds a method.
		private Method BuildMethod(EnvDTE.CodeFunction codeFunction)
		{
			Parameter[] parameters = BuildParameters(codeFunction);

			Method method = new Method()
			{
				Name = codeFunction.Name,
				ReturnType = BuildMType(codeFunction.Type.CodeType),
				Parameters = parameters
			};

			return method;
		}

		// Builds parameters.
		public Parameter[] BuildParameters(EnvDTE.CodeFunction codeFunction)
		{
			List<Parameter> parameters = new List<Parameter>();

			foreach (EnvDTE80.CodeParameter2 parameter in codeFunction.Parameters)
			{
				if (parameter.Type.CodeType.FullName == "Microsoft.Owin.IOwinContext") continue;

				parameters.Add(BuildParameter(parameter));
			}

			return parameters.ToArray();
		}

		// Builds a method parameter.
		private Parameter BuildParameter(EnvDTE80.CodeParameter2 codeParameter)
		{
			Parameter parameter = new Parameter()
			{
				Name = codeParameter.Name,
				Type = BuildMType(codeParameter.Type.CodeType),
				IsOptional = (codeParameter.ParameterKind == EnvDTE80.vsCMParameterKind.vsCMParameterKindOptional)
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
			while (index < Interfaces.Count)
			{
				// Builds the interface.
				switch (Interfaces[index].Kind)
				{
					case EnvDTE.vsCMElement.vsCMElementClass: interfaces.Add(BuildInterface((EnvDTE.CodeClass)Interfaces[index])); break;
					case EnvDTE.vsCMElement.vsCMElementEnum: enums.Add(BuildEnum((EnvDTE.CodeEnum)Interfaces[index])); break;
				}

				// Next interface.
				index++;
			}

			// All interfaces generated.
			Interfaces.Clear();

			model.Interfaces = interfaces.ToArray();
			model.Enums = enums.ToArray();
		}

		// Builds an enum.
		private AEnum BuildEnum(EnvDTE.CodeEnum codeEnum)
		{
			EnumValue[] enumValues = BuildEnumValues(codeEnum);

			AEnum aEnum = new AEnum
			{
				Name = codeEnum.Name,
				Values = enumValues
			};

			return aEnum;
		}

		// Builds enum values.
		private EnumValue[] BuildEnumValues(EnvDTE.CodeEnum codeEnum)
		{
			List<EnumValue> enumValues = new List<EnumValue>();

			foreach (EnvDTE.CodeVariable codeVariable in codeEnum.Members)
			{
				enumValues.Add(BuildEnumValue(codeVariable));
			}

			return enumValues.ToArray();
		}

		// Builds an enum value.
		private EnumValue BuildEnumValue(EnvDTE.CodeVariable codeVariable)
		{
			EnumValue enumValue = new EnumValue()
			{
				Name = codeVariable.Name,
				Value = codeVariable.InitExpression != null ? codeVariable.InitExpression.ToString() : string.Empty
			};

			return enumValue;
		}

		// Builds an interface.
		private Interface BuildInterface(EnvDTE.CodeClass codeClass)
		{
			Property[] properties = BuildProperties(codeClass);

			Interface theInterface = new Interface
			{
				Name = codeClass.Name,
				Properties = properties
			};

			return theInterface;
		}

		// Builds properties.
		public Property[] BuildProperties(EnvDTE.CodeClass codeClass)
		{
			List<Property> properties = new List<Property>();

			foreach (EnvDTE.CodeElement codeElement in codeClass.Members)
			{
				if (codeElement.Kind != EnvDTE.vsCMElement.vsCMElementProperty) continue;

				properties.Add(BuildProperty((EnvDTE.CodeProperty)codeElement));
			}

			return properties.ToArray();
		}

		// Builds a property.
		private Property BuildProperty(EnvDTE.CodeProperty codeProperty)
		{
			Property property = new Property()
			{
				Name = codeProperty.Name,
				Type = BuildMType(codeProperty.Type.CodeType)
			};

			return property;
		}
	}
}
