using System.Threading.Tasks;

namespace Lib
{
    public interface IService
    {
        object CreateObject();

        Task DoAsync();

        Task<int> GetIntAsync();
    }
}
