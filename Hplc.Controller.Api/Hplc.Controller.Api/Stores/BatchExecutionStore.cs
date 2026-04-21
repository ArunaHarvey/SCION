using System.Collections.Concurrent;
using Hplc.Controller.Api.Services;

namespace Hplc.Controller.Api.Stores;

public static class BatchExecutionStore
{
    private static readonly ConcurrentDictionary<string,
        ConcurrentQueue<ChromPoint>> Buffers = new();

    private static string Key(string batch, string sample)
        => $"{batch}:{sample}";

    public static void Push(string batch, string sample, ChromPoint p)
    {
        var q = Buffers.GetOrAdd(Key(batch, sample),
            _ => new ConcurrentQueue<ChromPoint>());
        q.Enqueue(p);
    }

    public static List<ChromPoint> Pull(string batch, string sample)
    {
        if (!Buffers.TryGetValue(Key(batch, sample), out var q))
            return new();

        var list = new List<ChromPoint>();
        while (q.TryDequeue(out var p))
            list.Add(p);

        return list;
    }
}