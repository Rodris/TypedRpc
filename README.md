# TypedRpc

TypedRpc is a [JsonRpc](http://www.jsonrpc.org/specification) implementation in [OWIN](http://owin.org/) that automatically generates [TypeScript](https://www.typescriptlang.org/) files to be used by the client.

## Installation

## Server

To create a class to expose methods and handle client requests, add the `RpcServerHandler` attribute to it. All its public methods will be available at the client side.

You may use default values for parameters. They will be optional at the client side.

### Example
```C#
[RpcServerHandler]
public class RpcServerExample
{
  public String Echo(String name = "Guest")
  {
    return "Hello, " + name + "!";
  }
}
```

## Client

Add a reference to the `Scripts/RpcClient.ts` file to have access to server handlers and its methods.

To make a method call, create a new instance of the desired class.

### Example

```TypeScript
/// <reference path="Scripts/RpcClient.ts" />

let rpc: RpcServer.RpcServerExample = new RpcServerNet.RpcServerExample();
rpc.Echo("New User").done(function(data, jsonResponse) {
  console.log(data);
});
```

## Contributions

Contributions are greatly appreciated.

## License

The contents of this repository are covered under the [Mit License](http://opensource.org/licenses/MIT).
