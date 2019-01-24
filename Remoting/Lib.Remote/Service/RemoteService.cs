using System;
using System.Threading.Tasks;

namespace Lib.Remote
{
    /// <summary>
    /// This class implements the shared interface and acts as proxy towards the remotable, "remoting enabled and registered" type.
    /// This class is used by the Client.
    /// </summary>
    public class RemoteService : IService
    {
        private readonly RemotableService _service;
        private readonly string _id;

        public RemoteService()
        {
            _id = this.GetHashCode().ToString();
            Print($"{_id} RemoteService created");
            _service = new RemotableService();
        }

        public object CreateObject()
        {
            Print($"{_id} Remote: CreateObject");
            return _service.CreateObject();
        }

        public Task DoAsync()
        {
            Print($"{_id} Remote: DoAsync");
            _service.Do();
            return Task.CompletedTask;
        }

        public Task<int> GetIntAsync()
        {
            Print($"{_id} Remote: GetIntAsync");
            return Task.FromResult(_service.GetInt());
        }

        private void Print(string message)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }
    }
}
