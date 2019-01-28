using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Lib.RemoteWCF
{
    public sealed class WCFClient
    {
        private static NetTcpBinding _binding;
        private static EndpointAddress _address;

        public static void Connect(
            string host = "localhost",
            int port = Defaults.ConnectionPort,
            string channelName = Defaults.ChannelName)
        {
            _binding = new NetTcpBinding();
            _address = new EndpointAddress($"net.tcp://{host}:{port}/{channelName}");
        }

        internal static T CreateInstance<T>()
        {
            var channelFactory = new ChannelFactory<T>(_binding, _address);
            return channelFactory.CreateChannel();
        }
    }
}
