using System.Threading.Tasks;

namespace Lib
{
    public interface IService
    {
        Task<int> GetIntAsync();

        object CreateObject();
    }
}
