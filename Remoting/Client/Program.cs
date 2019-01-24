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
            Remote.Enable(/*security: new Remote.SecurityConfig() */);
            IService service = new RemoteService();
            IService complexService = new RemoteComplexService();
#else
            IService service = new ConcreteService();
#endif

            Console.WriteLine("CreateObject");
            var obj = service.CreateObject();
            Console.WriteLine("Result: " + obj);
#if REMOTE
            var complexObj = complexService.CreateObject();
            Console.WriteLine("ComplexResult: " + complexObj);
#endif

            Console.WriteLine("DoAsync");
            await service.DoAsync();
#if REMOTE
            var complexTask = complexService.DoAsync();
            await complexTask;
#endif

            Console.WriteLine("GetIntAsync");
            var result = await service.GetIntAsync();
            Console.WriteLine("Result: " + result);
#if REMOTE
            var complexTaskWithResult = complexService.GetIntAsync();
            var complexResult = await complexTaskWithResult;
            Console.WriteLine("ComplexResult: " + complexResult);
#endif

            Console.ReadLine();
        }
    }
}