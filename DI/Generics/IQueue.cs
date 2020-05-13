using System.Threading.Tasks;

namespace DI
{
    public interface IQueue<T> where T : IMessage
    {
        Task Send(T message);
    }
}