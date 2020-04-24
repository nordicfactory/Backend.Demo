using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Pipeline.Queues
{
    struct Test
    {
        public int T;
    }
    class Program
    {

        static async Task Main(string[] args)
        {
            var t = new Test();
            t.T = 9;

            await Demo2.Run();
        }
    }
    
    public interface IBuffer<T> : IDisposable
    {
        ValueTask Read(Func<T, ValueTask> action, CancellationToken token);
        ValueTask Add(T e);
    }

    public class Buffer<T> : IBuffer<T>
    {
        private readonly BlockingCollection<T> _buffer = new BlockingCollection<T>();

        public async IAsyncEnumerable<T> ReadAllAsync()
        {
            await Task.Yield();
            foreach (var e in _buffer.GetConsumingEnumerable())
            {
                yield return e;
            }
        }

        public async ValueTask Read(Func<T, ValueTask> action, CancellationToken token)
        {
            foreach (var e in _buffer.GetConsumingEnumerable(token))
            {
                await action.Invoke(e);
            }
        }

        public ValueTask Add(T e)
        {
            _buffer.Add(e);
            return new ValueTask();
        }

        public void Dispose()
        {
            _buffer.CompleteAdding();
            _buffer?.Dispose();
        }
    }

    public class ChannelsBuffer<T> : IBuffer<T>
    {

        private readonly ChannelWriter<T> _writer;
        private readonly ChannelReader<T> _reader;

        public ChannelsBuffer()
        {
            //how to handle back-pressure
            var opts = new BoundedChannelOptions(1)
            {
                FullMode = BoundedChannelFullMode.Wait,
                
            };
            //Unbounded is normally not the correct option.
            var channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions()
            {
                SingleReader = true,
                //SingleWriter = true
            });
            _reader = channel.Reader;
            _writer = channel.Writer;
        }

        public async ValueTask Read(Func<T, ValueTask> action, CancellationToken token)
        {
            //avoid buzy spin
            while (await _reader.WaitToReadAsync(token))
            {
                // read all available data, as it keeps coming
                while (_reader.TryRead(out var item))
                {
                    await action.Invoke(item);
                }
            }
        }


        public ValueTask Add(T e)
        {
            //if (await _writer.WaitToWriteAsync()) _writer.TryWrite(e);
            //might block, if the queue is full and set to wait
            return _writer.WriteAsync(e);
        }

        public void Dispose()
        {
            _writer.Complete();
        }
    }

    public class PipeBuffer
    {
        private PipeReader _reader;
        private PipeWriter _writer;

        public PipeBuffer()
        {
            var pipe = new Pipe();
            _writer = pipe.Writer;
            _reader = pipe.Reader;
        }
        public async ValueTask Read(CancellationToken innerToken)
        {
            var data = await _reader.ReadAsync(innerToken);
            if (data.Buffer.IsSingleSegment)
            {

            }
        }

        public async ValueTask Add(byte[] e)
        {
            void Write()
            {
                var span = _writer.GetSpan(e.Length);
                for (var i = 0; i < e.Length; i++)
                {
                    span[i] = e[i];
                }

                _writer.Advance(e.Length);
            }

            Write();
            await _writer.FlushAsync();
        }
    }
}
