using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pipeline.Queues
{
    public class Message
    {

    }

    public class HelloMessage : Message
    {
        public string Message { get; set; }
    }

    public class GoodBye : Message
    {
        public string Message { get; set; }
    }

    public interface IHandleMessages
    {
    }

    public class Handler<T> : IHandleMessages where T : Message
    {
        public async ValueTask Handle(T message)
        {
            Console.WriteLine(message);
        }
    }

    public class Handler2<T1, T2> : IHandleMessages where T1 : Message
    {
        public async ValueTask Handle(T1 message)
        {

        }
    }
}
