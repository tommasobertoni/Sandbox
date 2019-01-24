using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Remote
{
    /// <summary>
    /// This type aims to partially solve the problem of not beign able to use only one type between the Client and the Server, because the
    /// shared interface contains methods returning types that are not serializable (i.e. Task<T>).
    /// Since every returned object is binary serialized by .NET Remoting, this class uses the new type SerializableTask<TResult> when a Task<T>
    /// should be returned.
    /// SerializableTask<T> extends Task<T> and implements ISerializable (along with the declaration of the [Serializable] attribute) in order to
    /// explicitly provide the Result to the binary formatter trying to serialize the data when the method is invoked by .NET Remoting.
    /// Since it must not return any Task<T>, there cannot be any await, and the result is resolved synchronously when the method is invoked.
    /// 
    /// This workaround should be applicable to every type that doesn't support serialization.
    /// </summary>
    public class RemoteComplexService : MarshalByRefObject, IService, IRemotable
    {
        private IService _service;
        private readonly string _id;

        public RemoteComplexService()
        {
            _id = this.GetHashCode().ToString();
            Print($"{_id} RemoteComplexService created");
            _service = new ConcreteService();
        }

        public object CreateObject()
        {
            Print($"{_id} RemoteComplexService: CreateObject");
            return _service.CreateObject();
        }

        public Task DoAsync()
        {
            Print($"{_id} RemoteComplexService: DoAsync");
            return new SerializableTask(_service.DoAsync());
        }

        public Task<int> GetIntAsync()
        {
            Print($"{_id} RemoteComplexService: GetIntAsync");
            return new SerializableTask<int>(_service.GetIntAsync()); // Blocks and resolves the result!
        }

        private void Print(string message)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }
    }
}
