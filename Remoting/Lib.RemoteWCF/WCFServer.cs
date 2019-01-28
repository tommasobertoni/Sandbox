using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Lib.RemoteWCF
{
    public class WCFServer
    {
        private static readonly List<ServiceHost> _hosts = new List<ServiceHost>();

        public static void Enable(
            string host = "localhost",
            int port = Defaults.ConnectionPort,
            string channelName = Defaults.ChannelName)
        {
            var binding = new NetTcpBinding();
            var baseAddress = new Uri($"net.tcp://{host}:{port}/{channelName}");

            WCFServer.Close();

            var remoteServiceTypes = FindRemoteServices();
            RegisterRemoteServiceTypes(remoteServiceTypes, binding, baseAddress);
        }

        public static void Close()
        {
            _hosts.ForEach(h => h?.Close());
            _hosts.Clear();
        }

        private static IEnumerable<TypeInfo> FindRemoteServices()
        {
            var remoteServiceType = typeof(IRemoteService);

            var remoteServiceTypes = remoteServiceType.Assembly.DefinedTypes
                .Where(t => t != remoteServiceType && remoteServiceType.IsAssignableFrom(t));

            return remoteServiceTypes;
        }

        private static void RegisterRemoteServiceTypes(IEnumerable<TypeInfo> remoteServiceTypes, NetTcpBinding binding, Uri baseAddress)
        {
            foreach (var remoteServiceType in remoteServiceTypes)
            {
                var serviceHost = new ServiceHost(typeof(WCFService), baseAddress);
                serviceHost.AddServiceEndpoint(typeof(IWCFService), binding, baseAddress);
                serviceHost.Open();
                _hosts.Add(serviceHost);
            }
        }
    }
}
