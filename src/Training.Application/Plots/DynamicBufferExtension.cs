using System;
using System.Linq;
using System.Reactive.Linq;

namespace Training.Application.Plots
{
    internal static class DynamicBufferExtension
    {
        public static IObservable<T[]> DynamicBuffer<T>(this IObservable<T> source, IObservable<int> bufferSize)
        {
            return Observable.Create<T[]>(observer =>
            {
                object _bufLck = new object();
                T[] buffer = new T[0];
                int currentBufferSize = 0;
                int count = 0;

                bufferSize.Subscribe(newSz =>
                {
                    lock (_bufLck)
                    {
                        if (newSz > currentBufferSize)
                        {
                            var newBuffer = new T[newSz];
                            buffer.CopyTo(newBuffer, 0);
                            buffer = newBuffer;
                            currentBufferSize = newSz;
                        }
                        else if (newSz < currentBufferSize)
                        {
                            currentBufferSize = newSz;

                            if (count >= newSz)
                            {
                                var toSkip = newSz == 0 ? 0 : newSz - 1;

                                var remainder = buffer.Skip(toSkip).Take(count - toSkip).ToArray();
                                if (remainder.Length > 0) observer.OnNext(remainder);

                                var newBuffer = new T[newSz];
                                buffer.Take(toSkip).ToArray().CopyTo(newBuffer, 0);
                                buffer = newBuffer;
                                count = toSkip;
                            }
                            else
                            {
                                var newBuffer = new T[newSz];
                                buffer.Take(count).ToArray().CopyTo(newBuffer, 0);
                                buffer = newBuffer;
                            }
                        }
                    }
                });

                return source.Subscribe(value =>
                {
                    lock (_bufLck)
                    {
                        if (buffer.Length == 0)
                        {
                            observer.OnNext(new[] {value});
                            return;
                        }

                        if (count == currentBufferSize)
                        {
                            observer.OnNext(buffer);
                            count = 0;
                        }

                        buffer[count] = value;
                        count++;
                    }
                }, observer.OnError, () =>
                {
                    if (count > 0)
                    {
                        observer.OnNext(buffer.Take(count).ToArray());
                        count = 0;
                    }

                    observer.OnCompleted();
                });
            });
        }
    }
}