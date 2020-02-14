using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipeline.Queues
{
    public static class Demo2
    {
        public static async Task Run()
        {


            IBuffer<Message> buff = new ChannelsBuffer<Message>();
            var read = ReadAndPrint(buff);
            await buff.Add(new HelloMessage
            {
                Message = "Hello"
            });
            buff.Dispose();
            await read;
        }



        private static async Task ReadAndPrint(IBuffer<Message> buffer)
        {
            var handlers = new List<IHandleMessages>
            {
                new Handler<HelloMessage>()
            };
            await buffer.Read(async m =>
            {
                foreach (var g in handlers)
                {
                    await Handle(g, m);
                }
            }, CancellationToken.None);

        }

        private static ValueTask Handle(IHandleMessages handler, Message m)
        {
            return new ValueTask();
        }

        private static ValueTask Handle<T>(Handler<T> handler, T m) where T : Message
        {
       
            {
                return handler.Handle(m);
            }
            return new ValueTask();
        }


    }
}