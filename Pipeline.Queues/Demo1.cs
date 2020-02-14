using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pipeline.Queues
{
    public static class Demo1
    {
        public static async Task Run()
        {
            IBuffer<string> buff = new ChannelsBuffer<string>();

            var cts = new CancellationTokenSource();


            var reader = ReadAndPrint(buff, cts.Token);

            while (!cts.IsCancellationRequested)
            {
                var str = Console.ReadLine();
                switch (str)
                {
                    case "quit":
                        cts.Cancel();
                        break;
                    case "exit":
                        buff.Dispose();
                        cts.Cancel();
                        break;
                    default:
                        await buff.Add(str);
                        break;
                }
            }

            await reader;
        }

        private static async Task ReadAndPrint(IBuffer<string> buffer, CancellationToken token)
        {

            try
            {
                await buffer.Read(s =>
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(s);
                    Console.ResetColor();
                    return new ValueTask();
                }, token);
                Console.WriteLine("Read to end");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Good bye");
            }

        }
    }
}
