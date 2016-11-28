# TypedRpc

TypedRpc is a [JsonRpc](http://www.jsonrpc.org/specification) implementation in [OWIN](http://owin.org/) that automatically generates [TypeScript](https://www.typescriptlang.org/) files to be used by the client.

## Installation

To install from NuGet, run the following command in the Package Manager Console:

```
Install-Package TypedRpc
```

## Server

To make a class expose its methods for client requests, add the `TypedRpcHandler` attribute to it. All its public methods will be available at the client side.

You may use default values for parameters. They will be optional at the client side.

### Example
```C#
[TypedRpcHandler]
public class RpcServerExample
{
  public String Echo(String name = "Guest")
  {
    return "Hello, " + name + "!";
  }
}
```

## Client

Add a reference to the `Scripts/TypedRpc.ts` file to have access to server handlers and its methods.

To make a method call, create a new instance of the desired class.

If your TypeScript files are not finding the handlers or their methods, update the `Scripts/TypedRpc.ts` file. Click on it with the right mouse button and click on 'Run Custom Tool'.

### Example

```TypeScript
/// <reference path="Scripts/TypedRpc.ts" />

let rpc: TypedRpc.RpcServerExample = new TypedRpc.RpcServerExample();
rpc.Echo("New User").Done(function(data, jsonResponse) {
  console.log(data);
}).Fail(function(error, jsonResponse) {
  console.log(error);
});
```

## Contributions

Contributions are greatly appreciated.

## License

The contents of this repository are covered under the [Mit License](http://opensource.org/licenses/MIT).
