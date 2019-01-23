# Remoting

Demo app exploring .NET Remoting and it's potential use for a remote execution
of a class implementing a shared interface.

This test comes from the need of running a "*provider reference*" remotely,
from a vm trusted by the provider's web API (by IP whitelist).

Using .NET Remoting we can use the provider's web API without executing
the whole program on the trusted VM: we just need to inject the *remote*
implementation of `IService` and run a remote console app with the *remotable* types.
<br><br>
### The demo:

Launch `Server.exe` and `Client.exe` to create a **local**, **unsecure** TCP connection.
<br />
You can configure the host, security and other info by setting values in the parameters of `Remote.Enable` and `Remotable.Enable` methods.

The `Remote` class is used by the client to configure the connection to the server, the `Remotable` class is used by the .NET Remoting host.

The client app uses two different types of remote services (`RemoteService` and `AnotherRemoteService`)
<br><br>
#### Tech
C#7+, VS2017, .NET 4.7.2