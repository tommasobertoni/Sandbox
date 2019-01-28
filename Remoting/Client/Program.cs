using Lib;
using Lib.Remote;
using Lib.RemoteWCF;
using System;
using System.Threading.Tasks;

namespace RemotingApp
{
    public class Client
    {
#if REMOTE
        private static RemoteConsoleService _remoteConsole;
#endif

        public static async Task Main(string[] args)
        {
            await TestRemoting();
        }

        static async Task TestRemoting()
        {
            Console.WriteLine("Client.exe\n");

#if REMOTE
            RemotingClient.Connect(/*security: new Remote.SecurityConfig() */);
            WCFClient.Connect();

            IService service = new RemoteProxyService();
            IService selfProxyService = new SelfProxyRemoteService();
            IService wcfService = new WCFProxyService();

            _remoteConsole = new RemoteConsoleService();
#else
            IService service = new ConcreteService();
#endif
            
            WriteLine("\n---- CreateObject ----------------");
            var obj = service.CreateObject();
            Console.WriteLine("Result: " + obj);
#if REMOTE
            var selfProxyObj = selfProxyService.CreateObject();
            Console.WriteLine("SelfProxyResult: " + selfProxyObj);
            var wcfObject = wcfService.CreateObject();
            Console.WriteLine("WcfResult: " + wcfObject);
#endif

            WriteLine("\n---- DoAsync ----------------");
            await service.DoAsync();
#if REMOTE
            var selfProxyTask = selfProxyService.DoAsync();
            await selfProxyTask;
            var wcfTask = wcfService.DoAsync();
            await wcfTask;
#endif
            
            WriteLine("\n---- GetIntAsync ----------------");
            var result = await service.GetIntAsync();
            Console.WriteLine("Result: " + result);
#if REMOTE
            var selfProxyTaskWithResult = selfProxyService.GetIntAsync();
            var selfProxyResult = await selfProxyTaskWithResult;
            Console.WriteLine("SelfProxyResult: " + selfProxyResult);
            var wcfTaskWithResult = wcfService.GetIntAsync();
            var wcfResult = await wcfTaskWithResult;
            Console.WriteLine("WcfResult: " + wcfResult);
#endif
            Console.ReadLine();
        }

        private static void WriteLine(string message = null)
        {
#if REMOTE
            _remoteConsole.WriteLine(message);
#endif
            Console.WriteLine(message);
        }
    }
}