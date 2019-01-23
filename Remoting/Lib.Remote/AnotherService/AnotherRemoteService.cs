using System;
using System.Threading.Tasks;

namespace Lib.Remote
{
    public class AnotherRemoteService : IService
    {
        private readonly AnotherRemotableService _service;
        private readonly string _id;

        public AnotherRemoteService()
        {
            _id = this.GetHashCode().ToString();
            Print($"{_id} AnotherRemoteService created");
            _service = new AnotherRemotableService();
        }

        public object CreateObject()
        {
            Print($"{_id} AnotherRemote: CreateObject");
            return _service.CreateObject();
        }

        public Task<int> GetIntAsync()
        {
            Print($"{_id} AnotherRemote: GetIntAsync");
            return Task.FromResult(_service.GetInt());
        }

        private void Print(string message)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }
    }
}
