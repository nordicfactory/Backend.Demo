using System.Threading.Tasks;

namespace DI
{
    public interface IMessageHandler<T> where T : IMessage
    {
        Task Handle(T message);
    }
}