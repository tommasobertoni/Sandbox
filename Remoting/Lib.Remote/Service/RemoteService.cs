using System;
using System.Threading.Tasks;

namespace Lib.Remote
{
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
