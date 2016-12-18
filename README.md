# TypedRpc

TypedRpc is a [JsonRpc](http://www.jsonrpc.org/specification) implementation in [OWIN](http://owin.org/). It automatically generates [TypeScript](https://www.typescriptlang.org/) files to be used by the client.

## Installation

To install from NuGet, run the following command in the Package Manager Console:

```
Install-Package TypedRpc
```

## Server

To make a class expose its methods for client requests, add the `TypedRpcHandler` attribute to it. All its public methods will be available in TypeScript.

```C#
[TypedRpc.TypedRpcHandler]
public class RpcServerExample
{
	public String HelloWorld()
	{
		return "Hello World!";
	}
}
```

You may use default values for parameters. They will be optional at the client side.

```C#
public String Greet(String name = "Guest")
{
	return "Hello, " + name + "!";
}
```

If you need the IOwinContext in your method, add it as a parameter and it will be injected in runtime. The generated TypeScript class will ignore it.

```C#
public String GreetMe(Microsoft.Owin.IOwinContext context)
{
	return "Hello, " + context.Authentication.User.Identity.Name + "!";
}
```

Asynchronous methods are supported.

```C#
public async Task<String> GreetAsync(Microsoft.Owin.IOwinContext context)
{
	await Task.Delay(5000);
	return "Hello async!";
}
```

## Client

Create an `index.ts` file to your project and add a reference to the `Scripts/TypedRpc.ts` file to have access to the server handlers and its methods.

Create a new instance of the desired handler class.

If your TypeScript files are not finding the handlers or their methods, update the `Scripts/TypedRpc.ts` file. Click on it with the right mouse button and click on 'Run Custom Tool'.

```TypeScript
/// <reference path="Scripts/TypedRpc.ts" />

let rpc: TypedRpc.RpcServerExample = new TypedRpc.RpcServerExample();

var callback = function(data, jsonResponse) {
	console.log(data);
};

rpc.HelloWorld().done(callback).fail(callback);

rpc.Greet("New User").done(callback).fail(callback);

rpc.GreetMe().done(callback).fail(callback);

rpc.GreetAsync().done(callback).fail(callback);
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
