using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.RemoteWCF
{
    internal class WCFService : IWCFService, IRemoteService
    {
        private readonly ConcreteService _service;
        private readonly string _id;

        public WCFService()
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

        public Task DoAsync()
        {
            Print("DoAsync");
            return _service.DoAsync();
        }

        public Task<int> GetIntAsync()
        {
            Print("GetIntAsync");
            return _service.GetIntAsync();
        }

        private void Print(string message)
        {
            message = $"{_id} {this.GetType().Name}: {message}";
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }
    }
}
