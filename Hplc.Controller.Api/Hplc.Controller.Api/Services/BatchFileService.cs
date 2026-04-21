using Hplc.Controller.Api.Models;
using Hplc.Controller.Api.Models.Instrument;
using Hplc.Controller.Api.Services;
using Hplc.Controller.Api.Stores;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Hplc.Controller.Api.Services;

public class BatchFileService
{
    private readonly string _batchDir;
    private const int LC_COUNT = 4;

    private static readonly ConcurrentDictionary<string, BatchRunInfo> RunQueue = new();

    private readonly ChromatogramService _chromService;
    private readonly InstrumentStatusStore _status;

    public BatchFileService(
        ChromatogramService chromService,
        InstrumentStatusStore status)
    {
        _chromService = chromService;
        _status = status;

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

        return Directory
            .GetFiles(_batchDir, "*.json")
            .Select(f => Path.GetFileNameWithoutExtension(f)!)
            .Where(n => !string.IsNullOrWhiteSpace(n));
    }

    public Batch LoadBatchDefinition(string batchName)
    {
        var path = Path.Combine(_batchDir, $"{batchName}.json");

        if (!File.Exists(path))
            throw new FileNotFoundException(path);

        return JsonSerializer.Deserialize<Batch>(
            File.ReadAllText(path)
        )!;
    }

    public void SaveBatchDefinition(Batch batch)
    {
        var path = Path.Combine(_batchDir, $"{batch.BatchName}.json");

        File.WriteAllText(
            path,
            JsonSerializer.Serialize(batch, new JsonSerializerOptions
            {
                WriteIndented = true
            })
        );
    }

    /* =========================
       Run Queue
       ========================= */

    public List<BatchRunInfo> GetBatchRunQueue()
    {
        return RunQueue.Values
            .OrderBy(r => r.QueuePosition)
            .Select(CloneRun)
            .ToList();
    }

    public void EnqueueBatch(Batch batch)
    {
        if (string.IsNullOrWhiteSpace(batch.BatchName))
            throw new ArgumentException("BatchName cannot be empty");

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
       Start Batch
       ========================= */

    public void StartBatch(string batchName)
    {
        if (!RunQueue.TryGetValue(batchName, out var run))
            return;

        if (run.Status != BatchRunStatus.Queued)
            return;

        run.Status = BatchRunStatus.Running;

        var pending = new ConcurrentQueue<SampleExecutionInfo>(
            run.Samples
        );

        var msLock = new SemaphoreSlim(1, 1);

        for (int lcId = 1; lcId <= LC_COUNT; lcId++)
        {
            int capturedLcId = lcId;

            Task.Run(() => RunLcWorker(
                capturedLcId,
                run,
                pending,
                msLock
            ));
        }

        // ✅ Monitor batch completion
        Task.Run(async () =>
        {
            while (run.Samples.Any(
                s => s.State != SampleExecutionState.Completed))
            {
                await Task.Delay(200);
            }

            run.Status = BatchRunStatus.Completed;

            // ✅ Release chromatogram metadata only after batch fully ends
            _status.SetCurrentChrom(null);
        });
    }

    /* =========================
       LC Worker
       ========================= */

    private async Task RunLcWorker(
        int lcId,
        BatchRunInfo run,
        ConcurrentQueue<SampleExecutionInfo> pending,
        SemaphoreSlim msLock)
    {
        while (pending.TryDequeue(out var sample))
        {
            sample.AssignedLC = lcId;
            sample.State = SampleExecutionState.WaitingForMS;

            UpdateLcStatus(sample);

            await msLock.WaitAsync();

            try
            {
                sample.OwnsMS = true;
                sample.State = SampleExecutionState.Acquiring;

                _status.SetMsBusy(true, lcId);
                _status.SetCurrentChrom(new ChromatogramStatus
                {
                    BatchName = run.BatchName,
                    SampleName = sample.SampleName,
                    StartTime = DateTime.UtcNow
                });

                var chromatogram = _chromService.Load();
                var start = DateTime.UtcNow;

                foreach (var point in chromatogram)
                {
                    var elapsed = DateTime.UtcNow - start;
                    var expected = TimeSpan.FromSeconds(point.Time);

                    if (expected > elapsed)
                        await Task.Delay(expected - elapsed);

                    BatchExecutionStore.Push(
                        run.BatchName,
                        sample.SampleName,
                        point
                    );
                }

                sample.State = SampleExecutionState.Completed;
            }
            finally
            {
                sample.OwnsMS = false;
                _status.SetMsBusy(false, null);
                UpdateLcStatus(sample);

                // ❗ DO NOT clear CurrentChrom here
                // UI still needs it during last points

                msLock.Release();
            }
        }
    }

    /* =========================
       Helpers
       ========================= */

    private void UpdateLcStatus(SampleExecutionInfo s)
    {
        lock (_status)
        {
            _status.Lcs.RemoveAll(l => l.LcId == s.AssignedLC);

            _status.Lcs.Add(new LcStatus
            {
                LcId = s.AssignedLC,
                State = s.State.ToString(),
                Sample = s.SampleName
            });
        }
    }

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

    /* =========================
       Derived MS Info
       ========================= */

    public SampleExecutionInfo? GetActiveMsSample()
    {
        return RunQueue.Values
            .Where(r => r.Status == BatchRunStatus.Running)
            .SelectMany(r => r.Samples)
            .FirstOrDefault(s => s.OwnsMS);
    }
}