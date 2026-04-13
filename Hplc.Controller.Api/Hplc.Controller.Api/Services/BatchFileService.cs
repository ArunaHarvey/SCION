using System.Collections.Concurrent;
using System.Text.Json;
using Hplc.Controller.Api.Models;

namespace Hplc.Controller.Api.Services;

public class BatchFileService
{
    private readonly string _batchDir;
    private readonly string _queueDir;
    private readonly string _queueFile;

    // In-memory execution store
    private static readonly ConcurrentDictionary<string, BatchRunInfo> RunQueue = new();

    public BatchFileService()
    {
        var baseDataDir = Path.Combine(Directory.GetCurrentDirectory(), "Data");

        _batchDir = Path.Combine(baseDataDir, "Batches");
        _queueDir = Path.Combine(baseDataDir, "RunQueue");
        _queueFile = Path.Combine(_queueDir, "queue.json");

        Directory.CreateDirectory(_batchDir);
        Directory.CreateDirectory(_queueDir);

        EnsureQueueFileExists();
    }

    /* =========================
       Batch Definitions
       ========================= */

    public IEnumerable<string> GetAvailableBatchNames()
    {
        if (!Directory.Exists(_batchDir))
            return Enumerable.Empty<string>();

        return Directory.GetFiles(_batchDir, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => !string.IsNullOrWhiteSpace(n));
    }

    public Batch LoadBatchDefinition(string batchName)
    {
        var path = Path.Combine(_batchDir, $"{batchName}.json");

        if (!File.Exists(path))
            throw new FileNotFoundException($"Batch '{batchName}' not found", path);

        return JsonSerializer.Deserialize<Batch>(File.ReadAllText(path))!;
    }

    public void SaveBatchDefinition(Batch batch)
    {
        var path = Path.Combine(_batchDir, $"{batch.BatchName}.json");
        File.WriteAllText(path,
            JsonSerializer.Serialize(batch, new JsonSerializerOptions { WriteIndented = true })
        );
    }

    /* =========================
       Run Queue (SAFE)
       ========================= */

    public void EnqueueBatch(Batch batch)
    {
        RunQueue.TryAdd(batch.BatchName, new BatchRunInfo
        {
            BatchName = batch.BatchName,
            Status = BatchRunStatus.Queued,
            QueuePosition = RunQueue.Count + 1,
            Samples = batch.Samples.Select(s => new SampleExecutionInfo
            {
                SampleName = s.SampleName,
                MethodId = s.MethodId,
                State = SampleExecutionState.Queued
            }).ToList()
        });

        RecalculateQueuePositions();
        PersistQueue();
    }

    /* ✅ MOST IMPORTANT FIX
       Return IMMUTABLE SNAPSHOTS — never live objects */
    public List<BatchRunInfo> GetBatchRunQueue()
    {
        return RunQueue.Values
            .OrderBy(r => r.QueuePosition)
            .Select(CloneRun)
            .ToList();
    }

    /* ✅ Idempotent + thread-safe */
    public void StartBatch(string batchName)
    {
        if (!RunQueue.TryGetValue(batchName, out var run))
            return;

        lock (run)
        {
            if (run.Status != BatchRunStatus.Queued)
                return;

            run.Status = BatchRunStatus.Running;
            PersistQueue();
        }

        Task.Run(async () =>
        {
            foreach (var s in run.Samples)
            {
                s.State = SampleExecutionState.Preparing;
                await Task.Delay(300);

                s.State = SampleExecutionState.WaitingForMS;
                await Task.Delay(300);

                s.State = SampleExecutionState.Injecting;
                await Task.Delay(300);

                s.State = SampleExecutionState.Acquiring;
                await Task.Delay(300);

                s.State = SampleExecutionState.Completed;
            }

            run.Status = BatchRunStatus.Completed;
            PersistQueue();
        });
    }

    public void ClearRunQueue()
    {
        RunQueue.Clear();
        PersistQueue();
    }

    /* =========================
       Helpers
       ========================= */

    private static BatchRunInfo CloneRun(BatchRunInfo r)
        => new BatchRunInfo
        {
            BatchName = r.BatchName,
            Status = r.Status,
            QueuePosition = r.QueuePosition,
            Samples = r.Samples.Select(s => new SampleExecutionInfo
            {
                SampleName = s.SampleName,
                MethodId = s.MethodId,
                State = s.State
            }).ToList()
        };

    private void EnsureQueueFileExists()
    {
        if (!File.Exists(_queueFile))
            File.WriteAllText(_queueFile, "[]");
        else
            LoadQueueFromDisk();
    }

    private void PersistQueue()
    {
        File.WriteAllText(_queueFile,
            JsonSerializer.Serialize(
                RunQueue.Values.OrderBy(r => r.QueuePosition),
                new JsonSerializerOptions { WriteIndented = true }
            ));
    }

    private void LoadQueueFromDisk()
    {
        var list = JsonSerializer.Deserialize<List<BatchRunInfo>>(
            File.ReadAllText(_queueFile)
        ) ?? new();

        RunQueue.Clear();
        foreach (var r in list)
            RunQueue[r.BatchName] = r;
    }

    private void RecalculateQueuePositions()
    {
        int pos = 1;
        foreach (var r in RunQueue.Values.OrderBy(r => r.QueuePosition))
            r.QueuePosition = pos++;
    }
    public void RemoveFromQueue(string batchName)
    {
        if (!RunQueue.TryRemove(batchName, out _))
            return;

        RecalculateQueuePositions();
        PersistQueue();
    }
}