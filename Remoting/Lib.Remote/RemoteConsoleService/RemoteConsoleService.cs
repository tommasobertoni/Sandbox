using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Remote
{
    /// <summary>
    /// This class uses the .NET Remoting self proxy implementation.
    /// Enables to remotely write on the server's console.
    /// It's used for formatting the demo results and separating the different operations / invoked methods.
    /// </summary>
    public class RemoteConsoleService : MarshalByRefObject, IRemoteService
    {
        public void WriteLine(string message = null)
        {
            Console.WriteLine(message);
        }
    }
}
