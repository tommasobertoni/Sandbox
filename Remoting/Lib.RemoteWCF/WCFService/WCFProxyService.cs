using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.RemoteWCF
{
    public class WCFProxyService : IService
    {
        private readonly IWCFService _wcfService;
        private readonly string _id;

        public WCFProxyService()
        {
            _id = this.GetHashCode().ToString();
            Print("created");
            _wcfService = WCFClient.CreateInstance<IWCFService>();
        }

        public object CreateObject()
        {
            Print("CreateObject");
            return _wcfService.CreateObject();
        }

        public Task DoAsync()
        {
            Print("DoAsync");
            return _wcfService.DoAsync();
        }

        public Task<int> GetIntAsync()
        {
            Print("GetIntAsync");
            return _wcfService.GetIntAsync();
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
