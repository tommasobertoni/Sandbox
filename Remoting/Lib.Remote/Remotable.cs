using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Lib.Remote
{
    public sealed class Remotable
    {
        /// <summary>
        /// Enables this host to register remote types that will be invoked using .NET Remoting (over TCP).
        /// </summary>
        /// <param name="connectionPort">The TCP port.</param>
        /// <param name="useSecureChannel">If true, only secure connections will be allowed.</param>
        /// <param name="channelName">The local TCP channel name.</param>
        public static void Enable(
            int connectionPort = Defaults.ConnectionPort,
            bool useSecureChannel = false,
            string channelName = Defaults.ChannelName)
        {
            RegisterChannel(connectionPort, useSecureChannel, channelName);

            var remotables = FindRemotables();
            RegisterRemotables(remotables);

            System.Console.WriteLine("Remotable enabled");
        }

        private static void RegisterChannel(int connectionPort, bool useSecureChannel, string channelName)
        {
            var properties = new Dictionary<string, object>
            {
                ["name"] = channelName,
                ["port"] = connectionPort,
            };

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

        private static void RegisterRemotables(IEnumerable<TypeInfo> remotables)
        {
            foreach (var remotable in remotables)
            {
                RemotingConfiguration.RegisterActivatedServiceType(remotable);
            }
        }
    }
}
