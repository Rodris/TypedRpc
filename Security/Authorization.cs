using JsonRpc;
using Microsoft.Owin;
using System;
using System.Linq;
using System.Reflection;

namespace TypedRpc.Security
{
	// Type of authorization.
	public enum AuthorizationType
	{
		NotRequired,
		Required,
		Default
	}
	
	// TypedRpc Authorization attribute.
	public class Authorization : Attribute
	{
		// The authentication errors.
		public static readonly JsonError
			ERROR_NOT_AUTHENTICATED = new JsonError() { Code = -31000, Message = "User not authenticated." },
			ERROR_NOT_AUTHORIZED = new JsonError() { Code = -31100, Message = "User not authorized." };

		// Type of authorization.
		public AuthorizationType Type { get; private set; }

		// Roles
		public string[] Roles { get; private set; }

		// Constructor
		public Authorization(params string[] roles) : this(AuthorizationType.Default, roles) { }

		// Constructor
		public Authorization(AuthorizationType type, params string[] roles)
		{
			// Initializations
			Type = type;
			Roles = roles;
		}

		// Validates a method authorization.
		internal static bool Authorized(IOwinContext context, MethodInfo methodInfo, bool defaultAuthorizationRequired, out JsonError error)
		{
			// Declarations
			Authorization authorization;
			bool authorizationRequired;

			// Initializations
			authorizationRequired = defaultAuthorizationRequired;
			error = null;

			// Checks if method specifies authentication rules.
			authorization = methodInfo.GetCustomAttribute<Authorization>();
			if (authorization != null)
			{
				// Retrieves authorization type.
				switch (authorization.Type)
				{
					case AuthorizationType.Required: authorizationRequired = true; break;
					case AuthorizationType.NotRequired: authorizationRequired = false; break;
				}
			}

			// Checks if authorization is not required.
			if (!authorizationRequired) return true;
			
			// Checks if user is not authenticated.
			if (!context.Authentication.User.Identity.IsAuthenticated)
			{
				error = ERROR_NOT_AUTHENTICATED;
				return false;
			}
			
			// Checks if roles are specified.
			if (authorization != null && authorization.Roles.Any())
			{
				// Checks if user has any required role.
				if (!authorization.Roles.Any(role => context.Authentication.User.IsInRole(role)))
				{
					error = ERROR_NOT_AUTHORIZED;
					return false;
				}
			}

			// User authorized.
			return true;
		}
	}
}
