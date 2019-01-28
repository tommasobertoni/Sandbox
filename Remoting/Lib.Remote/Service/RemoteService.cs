using System;

namespace Lib.Remote
{
    /// <summary>
    /// This class is the .NET Remoting wrapper over the concrete implementation of the shared interface.
    /// This type cannot implement directly the interface, because some methods return types that are not serializable (i.e. Task<T>).
    /// Instances of this type will run on the Server, while on the Client there will be some proxy instances created by .NET Remoting.
    /// </summary>
    internal class RemoteService : MarshalByRefObject, IRemoteService
    {
        private IService _service;
        private readonly string _id;

        public RemoteService()
        {
            _id = this.GetHashCode().ToString();
            Print("created");
            _service = new ConcreteService();
        }

        public object CreateObject()
        {
            Print("CreateObject");
            return _service.CreateObject();
        }

        public void Do()
        {
            Print("Do");
            _service.DoAsync().Wait();
        }

        public int GetInt()
        {
            Print("GetInt");
            var result = _service.GetIntAsync().GetAwaiter().GetResult();
            return result;
        }

        private void Print(string message)
        {
            message = $"{_id} {this.GetType().Name}: {message}";
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }
    }
}
