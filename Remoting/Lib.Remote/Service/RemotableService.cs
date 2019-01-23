using System;

namespace Lib.Remote
{
    internal class RemotableService : MarshalByRefObject, IRemotable
    {
        private ConcreteService _service;
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
