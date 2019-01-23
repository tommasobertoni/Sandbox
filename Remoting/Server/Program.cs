using Lib.Remote;
using System;

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
#if REMOTE
            Remotable.Enable(useSecureChannel: true);
#else
            Console.WriteLine("No remote");
#endif
            Console.ReadLine();
        }
    }
}