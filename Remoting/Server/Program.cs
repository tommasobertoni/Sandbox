using Lib.Remote;
using Lib.RemoteWCF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerApp
{
    public class Server
    {
        public static void Main()
        {
            TestRemoting();
        }

        static void TestRemoting()
        {
            Console.WriteLine("Server.exe\n");

#if REMOTE
            RemotingServer.Enable(/*useSecureChannel: true*/);
            WCFServer.Enable();
#else
            Console.WriteLine("No remote");
#endif
            Console.ReadLine();
#if REMOTE
            WCFServer.Close();
#endif
        }
    }
}