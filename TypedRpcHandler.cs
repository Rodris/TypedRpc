using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TypedRpc
{
    // Handler in server.
    public class TypedRpcHandler : Attribute
    {
        // Available handlers.
        public static List<object> Handlers = FindHandlers();

        // Finds all handlers in project.
        private static List<object> FindHandlers()
        {
            // Declarations
            List<object> handlers;
            List<Type> types;
            Type handlerType;
            ConstructorInfo constructor;

            // Initializations
            handlers = new List<object>();

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
                handlers.Add(constructor.Invoke(null));
            }

            return handlers;
        }

    }
}