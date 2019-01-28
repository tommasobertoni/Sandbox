using System;
using System.Threading.Tasks;

namespace Lib.Remote
{
    /// <summary>
    /// This class implements the shared interface and acts as proxy towards the remote service type.
    /// This class is used by the Client.
    /// </summary>
    public class RemoteProxyService : IService
    {
        private readonly RemoteService _service;
        private readonly string _id;

        public RemoteProxyService()
        {
            _id = this.GetHashCode().ToString();
            Print("created");
            _service = new RemoteService();
        }

        public object CreateObject()
        {
            Print("CreateObject");
            return _service.CreateObject();
        }

        public Task DoAsync()
        {
            Print("DoAsync");
            _service.Do();
            return Task.CompletedTask;
        }

        public Task<int> GetIntAsync()
        {
            Print("GetIntAsync");
            return Task.FromResult(_service.GetInt());
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
