using Lib;
using System.ServiceModel;

namespace Lib.RemoteWCF
{
	[ServiceContract]
	public interface IWCFService
	{
		[OperationContract]
		System.Object CreateObject();
		[OperationContract]
		System.Threading.Tasks.Task DoAsync();
		[OperationContract]
		System.Threading.Tasks.Task<System.Int32> GetIntAsync();
	}
}
