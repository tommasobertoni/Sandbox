using System;

namespace Lib.Remote
{
    internal class AnotherRemotableService : MarshalByRefObject, IRemotable
    {
        private ConcreteService _service;
        private readonly string _id;

        public AnotherRemotableService()
        {
            _id = this.GetHashCode().ToString();
            Print($"{_id} AnotherRemotableService created");
            _service = new ConcreteService();
        }

        public object CreateObject()
        {
            Print($"{_id} AnotherRemotable: CreateObject");
            return _service.CreateObject();
        }

        public int GetInt()
        {
            Print($"{_id} AnotherRemotable: GetInt");
            var result = _service.GetIntAsync().GetAwaiter().GetResult();
            return result;
        }

        private void Print(string message)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }
    }
}
