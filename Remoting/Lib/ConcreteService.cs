using System;
using System.Threading.Tasks;

namespace Lib
{
    public class ConcreteService : IService
    {
        private readonly string _id;

        public ConcreteService()
        {
            _id = this.GetHashCode().ToString();
            Print("created");
        }

        public object CreateObject()
        {
            Print("CreateObject");
            return "test";
        }

        public async Task DoAsync()
        {
            Print("DoAsync");
            await Task.Yield();
        }

        public async Task<int> GetIntAsync()
        {
            Print("GetIntAsync");
            await Task.Yield();
            return 42;
        }

        private void Print(string message)
        {
            message = $"{_id} {this.GetType().Name}: {message}";
            Console.WriteLine(message);
        }
    }
}
