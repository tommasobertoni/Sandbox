using Lib;
using Lib.Remote;
using System;
using System.Threading.Tasks;

namespace RemotingApp
{
    public class Client
    {
        public static async Task Main(string[] args)
        {
            await TestRemoting();
        }

        static async Task TestRemoting()
        {
#if REMOTE
            Remote.Enable(security: new Remote.SecurityConfig() /* Initialize with a remote user's credentials */);
            IService service = new RemoteService();
            IService anotherService = new AnotherRemoteService();
#else
            IService service = new ConcreteService();
#endif

            Console.WriteLine("CreateObject");
            service.CreateObject();
#if REMOTE
            anotherService.CreateObject();
#endif

            Console.WriteLine("GetIntAsync");
            var result = await service.GetIntAsync();
            Console.WriteLine("Result: " + result);
#if REMOTE
            var anotherResult = await anotherService.GetIntAsync();
            Console.WriteLine("AnotherResult: " + result);
#endif

            Console.ReadLine();
        }
    }
}