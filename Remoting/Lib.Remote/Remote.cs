using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Lib.Remote
{
    public sealed class Remote
    {
        public class SecurityConfig
        {
            public string Domain { get; set; }

            public string Username { get; set; }

            public string Password { get; set; }
        }

        /// <summary>
        /// Enables this client to use remote types using .NET Remoting (over TCP).
        /// </summary>
        /// <param name="host">The remote host.</param>
        /// <param name="serverPort">The remote TCP port.</param>
        /// <param name="timeoutMillis">A connection timeout.</param>
        /// <param name="security">Security configuration: if an instance is passed to the method the connection will be secure.</param>
        /// <param name="channelName">The local TCP channel name.</param>
        public static void Enable(
            string host = "localhost",
            int serverPort = Defaults.ConnectionPort,
            int? timeoutMillis = Defaults.TimeoutMillis,
            SecurityConfig security = default,
            string channelName = Defaults.ChannelName)
        {
            RegisterChannel(timeoutMillis, security, channelName);

            var remotables = FindRemotables();
            RegisterRemotables(remotables, host, serverPort);

            Console.WriteLine("Remote enabled");
        }

        private static void RegisterChannel(int? timeoutMillis, SecurityConfig security, string channelName)
        {
            var properties = new Dictionary<string, object>
            {
                ["name"] = channelName
            };

            if (timeoutMillis.HasValue) properties["timeout"] = timeoutMillis.Value;

            if (security?.Domain != null) properties["domain"] = security.Domain;
            if (security?.Username != null) properties["username"] = security.Username;
            if (security?.Password != null) properties["password"] = security.Password;

            bool useSecureChannel = security != default;
            ChannelServices.RegisterChannel(new TcpChannel(properties, null, null)
            {
                IsSecured = useSecureChannel
            }, ensureSecurity: useSecureChannel);
        }

        private static IEnumerable<TypeInfo> FindRemotables()
        {
            var remotableType = typeof(IRemotable);

            var remotables = remotableType.Assembly.DefinedTypes
                .Where(t => t != remotableType && remotableType.IsAssignableFrom(t));

            return remotables;
        }

        private static void RegisterRemotables(IEnumerable<TypeInfo> remotables, string host, int serverPort)
        {
            var serverUrl = $"tcp://{host}:{serverPort}";

            foreach (var remotable in remotables)
            {
                ActivatedClientTypeEntry myActivatedClientTypeEntry = new ActivatedClientTypeEntry(remotable, serverUrl);
                RemotingConfiguration.RegisterActivatedClientType(myActivatedClientTypeEntry);
            }
        }
    }
}
