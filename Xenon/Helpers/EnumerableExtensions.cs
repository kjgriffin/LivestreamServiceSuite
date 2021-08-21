using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenon.Helpers
{
    public static class EnumerableExtensions
    {
        // Adapted from https://blogs.msdn.microsoft.com/pfxteam/2012/03/05/implementing-a-simple-foreachasync-part-2/
        public static Task ForEachAsync<T>(this IEnumerable<T> source, int degreeOfParallelism, Func<T, Task> body, IProgress<T> progress = null)
        {
            return Task.WhenAll(
                Partitioner.Create(source).GetPartitions(degreeOfParallelism)
                    .Select(partition => Task.Run(async () =>
                    {
                        using (partition)
                            while (partition.MoveNext())
                            {
                                await body(partition.Current);
                                progress?.Report(partition.Current);
                            }
                    }))
            );
        }

        public static IEnumerable<T> ItemAsEnumerable<T>(this T item)
        {
            yield return item;
        }

    }
}
