using System;

namespace Lib.Remote
{
    /// <summary>
    /// This class is the .NET Remoting wrapper over the concrete implementation of the shared interface.
    /// This type cannot implement directly the interface, because some methods return types that are not serializable (i.e. Task<T>).
    /// Instances of this type will run on the Server, while on the Client there will be some proxy instances created by .NET Remoting.
    /// </summary>
    internal class RemotableService : MarshalByRefObject, IRemotable
    {
        private IService _service;
        private readonly string _id;

        public RemotableService()
        {
            _id = this.GetHashCode().ToString();
            Print($"{_id} RemotableService created");
            _service = new ConcreteService();
        }

        public object CreateObject()
        {
            Print($"{_id} Remotable: CreateObject");
            return _service.CreateObject();
        }

        public void Do()
        {
            Print($"{_id} Remotable: Do");
            _service.DoAsync().Wait();
        }

        public int GetInt()
        {
            Print($"{_id} Remotable: GetInt");
            var result = _service.GetIntAsync().GetAwaiter().GetResult();
            return result;
        }

        private void Print(string message)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }
    }
}
