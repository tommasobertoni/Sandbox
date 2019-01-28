using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Lib.Remote
{
    public sealed class RemotingClient
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
        public static void Connect(
            string host = "localhost",
            int serverPort = Defaults.ConnectionPort,
            int? timeoutMillis = Defaults.TimeoutMillis,
            SecurityConfig security = default,
            string channelName = Defaults.ChannelName)
        {
            RegisterChannel(timeoutMillis, security, channelName);

            var remoteServiceTypes = FindRemoteServices();
            RegisterRemoteServiceTypes(remoteServiceTypes, host, serverPort);
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

        private static IEnumerable<TypeInfo> FindRemoteServices()
        {
            var remoteServiceType = typeof(IRemoteService);

            var remoteServiceTypes = remoteServiceType.Assembly.DefinedTypes
                .Where(t => t != remoteServiceType && remoteServiceType.IsAssignableFrom(t));

            return remoteServiceTypes;
        }

        private static void RegisterRemoteServiceTypes(IEnumerable<TypeInfo> remoteServiceTypes, string host, int serverPort)
        {
            var serverUrl = $"tcp://{host}:{serverPort}";

            foreach (var remoteServiceType in remoteServiceTypes)
            {
                ActivatedClientTypeEntry myActivatedClientTypeEntry = new ActivatedClientTypeEntry(remoteServiceType, serverUrl);
                RemotingConfiguration.RegisterActivatedClientType(myActivatedClientTypeEntry);
            }
        }
    }
}
