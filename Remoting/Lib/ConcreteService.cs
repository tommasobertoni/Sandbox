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
            Print($"{_id} ConcreteService created");
        }

        public object CreateObject()
        {
            Print($"{_id} Concrete: CreateObject");
            return "test";
        }

        public async Task DoAsync()
        {
            Print($"{_id} Concrete: DoAsync");
            await Task.Yield();
        }

        public async Task<int> GetIntAsync()
        {
            Print($"{_id} Concrete: GetIntAsync");
            await Task.Yield();
            return 42;
        }

        private void Print(string message)
        {
            Console.WriteLine(message);
        }
    }
}
