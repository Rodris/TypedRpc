# TypedRpc

TypedRpc is a [JsonRpc](http://www.jsonrpc.org/specification) implementation in [OWIN](http://owin.org/). It automatically generates [TypeScript](https://www.typescriptlang.org/) files to be used by the client.

## Installation

To install from NuGet, run the following command in the Package Manager Console:

```
Install-Package TypedRpc
```

## Server

To make a class expose its methods for client requests, add the `TypedRpcHandler` attribute to it. All its public methods will be available in TypeScript.

You may use default values for parameters. They will be optional at the client side.

If you need the IOwinContext in your method, add it as a parameter. The generated TypeScript class will ignore it.

### Example

Create a `RpcServerExample.cs` file to your project.

```C#
[TypedRpc.TypedRpcHandler]
public class RpcServerExample
{
  public String Greet(String name = "Guest")
  {
    return "Hello, " + name + "!";
  }

  public String GreetMe(Microsoft.Owin.IOwinContext context)
  {
  	  return "Hello, " + context.Authentication.User.Identity.Name + "!";
  }
}
```

## Client

In your TypeCript files, add a reference to the `Scripts/TypedRpc.ts` file to have access to server handlers and its methods.

To make a method call, create a new instance of the desired handler class.

If your TypeScript files are not finding the handlers or their methods, update the `Scripts/TypedRpc.ts` file. Click on it with the right mouse button and click on 'Run Custom Tool'.

### Example

Create an `index.ts` file to your project and add a reference to the `TypedRpc.ts` file.

```TypeScript
/// <reference path="Scripts/TypedRpc.ts" />

let rpc: TypedRpc.RpcServerExample = new TypedRpc.RpcServerExample();

rpc.Greet("New User").done(function(data, jsonResponse) {
  console.log(data);
}).fail(function(error, jsonResponse) {
  console.log(error);
});

rpc.GreetMe().done(function(data, jsonResponse) {
  console.log(data);
}).fail(function(error, jsonResponse) {
  console.log(error);
});
```

Create an `index.html` file to your project and import the `TypedRpc.js` and `index.js` file to it.

```html
<!DOCTYPE html>
<html>
<head>
    <title>TypedRpc Example</title>
	<meta charset="utf-8" />
    <script src="Scripts/TypedRpc.js"></script>
    <script src="index.js"></script>
</head>
<body>

</body>
</html>
```

## License

The contents of this repository are covered under the [Mit License](http://opensource.org/licenses/MIT).
