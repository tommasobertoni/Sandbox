# Remoting

Demo app exploring different solutions for remotely execute methods on an instance
of a class implementing a shared interface.

This test comes from the need of running a "*provider reference*" remotely,
from a *VM* trusted by the provider's web API, by an IP whitelist.

The solution includes these implementations:

1. **using .NET Remoting with a proxy class:** the remote service class extends `MarshalByRefObject`
in order to be invokable by .NET Remoting and provides appropriate methods to invoke the
`ConcreteService`'s methods. <br />
The proxy implements the shared interface and is used by the client app. <br />
NOTE: *all return types must be binary serializable!*

2. **using .NET Remoting with a self proxy class:** this somewhat hacky implementation aims to use a
single class for implementing the shared interface and `MarshalByRefObject`, in order to be
registered as a *remotable* type with .NET Remoting. <br />
The difference from the previous implementation with the proxy class,
is that this type must be able to return all serializable types. Types that aren't serializable (i.e. `Task<T>`)
are wrapped by custom serializable classes (i.e. `SerializableTask<T>`).

3. **using WCF with a proxy class:** since .NET Remoting has been deprecated, this implementation uses WCF with
a proxy-service division similar to the first implementation with .NET Remoting. <br />
The explicit proxy type is required because WCF only allows to invoke methods decorated with the `[OperationContract]` attribute
and declared inside an interface decorated with the `[ServiceContract]` attribute.
Those attributes can't be applied directly to the target interface (`IService`) because this demo aims to invoke a remote execution
without changing the shared contract, and seamlessly enabling DI capabilities. <br />
Another required step for this implementation is to create a copy of the sared interface and decorate it with the
attributes required by WCF. The `IWCFService` interface is created using a T4 template that read the target interface
and creates a copy. The `IWCFService` will be implemented by the `WCFService` type (which is the type registered and
remotely invoked by WCF), while the `IService` shared interface will be implemented by the `WCFProxyService` used by the client app. <br />
NOTE: *`WCFService` can't implement both `IService` and `IWCFService` because, on the client app, WCF would create an `IService` proxy
and would throw exception because the methods declared on this interface don't have the required attributes.*
<br /><br />
### The demo:

Launch `Server.exe` and `Client.exe` to create a **local**, **unsecure** TCP connection. <br />
You can configure the host, security and other info by setting values in the parameters of `RemotingClient.Connect`/`WCFClient.Connect` and `RemotingServer.Enable`/`WCFServer.Enable` methods.
<br /><br />

![Screenshot](https://user-images.githubusercontent.com/8939890/51844472-a588c480-2315-11e9-8838-4180e2018d6a.png)

1. The Client creates an instance of `RemoteProxyService`, `SelfProxyRemoteService` and `WCFProxyService`, all of which implement the `IService` interface. <br />
Here you can see that the `RemoteProxyService` and `WCFProxyService` types are beign executed on the client app, but `SelfProxyRemoteService` is only invoked remotely.
Each remote type wraps and uses an instance of `ConcreteService`, and that's why its constructor is invoked immediately after the remote service's. Note that WCF deferres the creation of
the remote object to the first method invokation, therefore at this step no remote WCF object is created.

2. `CreateObject` gets invoked on each instance on the client app, every proxy logs the method's invokation and every remote service receives the instruction.
In this step the WCF remote object is created.

3. `DoAsync` gets invoked, the remote service implemented with .NET Remoting with the first method can't return a `Task<T>` (because it's not serializable), therefore
the method `Do` is invoked, which synchronously resolved the `Task` returned by the `ConcreteService`.

4. `GetIntAsync` gets invoked, and the execution is similar to `DoAsync`.
<br /><br />
#### Tech
C#7+, VS2017, .NET 4.7.2