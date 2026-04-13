using Hplc.Controller.Api.Models;
using Hplc.Controller.Api.Models.Instrument;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Hplc.Controller.Api.Services;

public class BatchFileService
{
    private readonly string _batchDir;

    /* =========================
       Constants
       ========================= */

    private const int LC_COUNT = 4;

    /* =========================
       In‑memory batch run queue
       ========================= */

    private static readonly ConcurrentDictionary<string, BatchRunInfo> RunQueue = new();

    public BatchFileService()
    {
        _batchDir = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Data",
            "Batches"
        );

        Directory.CreateDirectory(_batchDir);
    }

    /* =========================
       Batch Definitions
       ========================= */

    public IEnumerable<string> GetAvailableBatchNames()
    {
        if (!Directory.Exists(_batchDir))
            return Enumerable.Empty<string>();

        return Directory.GetFiles(_batchDir, "*.json")
            .Select(Path.GetFileNameWithoutExtension);
    }

    public Batch LoadBatchDefinition(string batchName)
    {
        var path = Path.Combine(_batchDir, $"{batchName}.json");

        if (!File.Exists(path))
            throw new FileNotFoundException(path);

        return JsonSerializer.Deserialize<Batch>(File.ReadAllText(path))!;
    }

    public void SaveBatchDefinition(Batch batch)
    {
        var path = Path.Combine(_batchDir, $"{batch.BatchName}.json");

        File.WriteAllText(
            path,
            JsonSerializer.Serialize(batch, new JsonSerializerOptions { WriteIndented = true })
        );
    }

    /* =========================
       Run Queue
       ========================= */

    public List<BatchRunInfo> GetBatchRunQueue()
        => RunQueue.Values
            .OrderBy(r => r.QueuePosition)
            .Select(CloneRun)
            .ToList();

    public void EnqueueBatch(Batch batch)
    {
        if (RunQueue.ContainsKey(batch.BatchName))
            return;

        var run = new BatchRunInfo
        {
            BatchName = batch.BatchName,
            Status = BatchRunStatus.Queued,
            QueuePosition = RunQueue.Count + 1,
            Samples = batch.Samples.Select(s => new SampleExecutionInfo
            {
                SampleName = s.SampleName,
                MethodId = s.MethodId,
                State = SampleExecutionState.Queued,
                AssignedLC = 0,
                OwnsMS = false
            }).ToList()
        };

        RunQueue[batch.BatchName] = run;
    }

    /* =========================
       Start Batch (CORRECT LOGIC)
       ========================= */


    public void StartBatch(string batchName)
    {
        if (!RunQueue.TryGetValue(batchName, out var run))
            return;

        if (run.Status != BatchRunStatus.Queued)
            return;

        Console.WriteLine(
            $"[START-BATCH] Batch={batchName}, Samples={run.Samples.Count}, Time={DateTime.UtcNow:O}"
        );

        run.Status = BatchRunStatus.Running;

        // ✅ Per-batch pending queue
        var pending = new ConcurrentQueue<SampleExecutionInfo>();
        foreach (var s in run.Samples)
        {
            pending.Enqueue(s);

            Console.WriteLine(
                $"[ENQUEUE] Batch={batchName}, Sample={s.SampleName}"
            );
        }

        // ✅ One MS per batch
        var msLock = new SemaphoreSlim(1, 1);

        // ✅ Start exactly 4 LC workers for THIS batch
        for (int lcId = 1; lcId <= LC_COUNT; lcId++)
        {
            Console.WriteLine(
                $"[LC-WORKER-START] Batch={batchName}, LC={lcId}"
            );

            int capturedLcId = lcId; // ✅ explicit capture for logging clarity

            Task.Run(() => RunLCWorker(
                capturedLcId,
                run,
                pending,
                msLock
            ));
        }

        // ✅ Monitor completion safely
        Task.Run(async () =>
        {
            while (run.Samples.Any(s => s.State != SampleExecutionState.Completed))
                await Task.Delay(200);

            run.Status = BatchRunStatus.Completed;

            Console.WriteLine(
                $"[BATCH-COMPLETE] Batch={batchName}, Time={DateTime.UtcNow:O}"
            );
        });
    }

    /* =========================
       LC Worker (PER BATCH)
       ========================= */

    private async Task RunLCWorker(
     int lcId,
     BatchRunInfo run,
     ConcurrentQueue<SampleExecutionInfo> pending,
     SemaphoreSlim msLock)
    {
        Console.WriteLine(
            $"[LC-WORKER-ENTER] Batch={run.BatchName}, WorkerLC={lcId}, Thread={Environment.CurrentManagedThreadId}"
        );

        while (pending.TryDequeue(out var sample))
        {
            Console.WriteLine(
                $"[LC-DEQUEUE] Batch={run.BatchName}, WorkerLC={lcId}, Sample={sample.SampleName}, Thread={Environment.CurrentManagedThreadId}"
            );

            // ✅ LC picks sample
            sample.AssignedLC = lcId;

            Console.WriteLine(
                $"[LC-ASSIGN] Batch={run.BatchName}, Sample={sample.SampleName}, AssignedLC={sample.AssignedLC}, WorkerLC={lcId}, Thread={Environment.CurrentManagedThreadId}"
            );

            sample.State = SampleExecutionState.Preparing;

            // LC preparation
            await Task.Delay(500);

            sample.State = SampleExecutionState.WaitingForMS;

            Console.WriteLine(
                $"[MS-WAIT] Batch={run.BatchName}, Sample={sample.SampleName}, LC={sample.AssignedLC}"
            );

            // ✅ Compete for single MS
            await msLock.WaitAsync();
            try
            {
                sample.OwnsMS = true;

                Console.WriteLine(
                    $"[MS-ACQUIRED] Batch={run.BatchName}, Sample={sample.SampleName}, LC={sample.AssignedLC}"
                );

                sample.State = SampleExecutionState.Injecting;
                await Task.Delay(300);

                sample.State = SampleExecutionState.Acquiring;
                await Task.Delay(800);

                sample.State = SampleExecutionState.Completed;

                Console.WriteLine(
                    $"[SAMPLE-COMPLETE] Batch={run.BatchName}, Sample={sample.SampleName}, LC={sample.AssignedLC}"
                );
            }
            finally
            {
                sample.OwnsMS = false;
                msLock.Release();

                Console.WriteLine(
                    $"[MS-RELEASED] Batch={run.BatchName}, Sample={sample.SampleName}, LC={sample.AssignedLC}"
                );
            }
        }

        Console.WriteLine(
            $"[LC-WORKER-EXIT] Batch={run.BatchName}, WorkerLC={lcId}, Thread={Environment.CurrentManagedThreadId}"
        );
    }


    public void RemoveFromQueue(string batchName)
    {
        RunQueue.TryRemove(batchName, out _);

        int pos = 1;
        foreach (var r in RunQueue.Values.OrderBy(r => r.QueuePosition))
            r.QueuePosition = pos++;
    }

    public void ClearRunQueue()
    {
        RunQueue.Clear();
    }

    /* =========================
       Helpers
       ========================= */

    private static BatchRunInfo CloneRun(BatchRunInfo r)
        => new()
        {
            BatchName = r.BatchName,
            Status = r.Status,
            QueuePosition = r.QueuePosition,
            Samples = r.Samples.Select(s => new SampleExecutionInfo
            {
                SampleName = s.SampleName,
                MethodId = s.MethodId,
                State = s.State,
                AssignedLC = s.AssignedLC,
                OwnsMS = s.OwnsMS
            }).ToList()
        };
    public MsStatus GetMsStatus()
    {
        // Find the running batch (only one MS exists)
        var runningBatch = RunQueue.Values
            .FirstOrDefault(b => b.Status == BatchRunStatus.Running);

        if (runningBatch == null)
        {
            return new MsStatus
            {
                IsAcquiring = false,
                ActiveLcId = null,
                BatchName = null
            };
        }

        // Find the sample currently owning the MS
        var activeSample = runningBatch.Samples
            .FirstOrDefault(s => s.OwnsMS);

        if (activeSample == null)
        {
            return new MsStatus
            {
                IsAcquiring = false,
                ActiveLcId = null,
                BatchName = runningBatch.BatchName
            };
        }

        return new MsStatus
        {
            IsAcquiring = true,
            ActiveLcId = $"LC-{activeSample.AssignedLC}",
            BatchName = runningBatch.BatchName
        };
    }
}