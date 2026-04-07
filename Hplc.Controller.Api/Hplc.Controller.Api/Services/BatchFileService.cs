using System.Text.Json;
using Hplc.Controller.Api.Models;

namespace Hplc.Controller.Api.Services;

public class BatchFileService
{
    private readonly string _batchDirectory;
    private readonly string _runQueueFile;

    public BatchFileService(IWebHostEnvironment env)
    {
        _batchDirectory = Path.Combine(env.ContentRootPath, "Data", "Batches");
        _runQueueFile = Path.Combine(_batchDirectory, "run-queue.json");
        Directory.CreateDirectory(_batchDirectory);
    }

    /* =========================
       Batch APIs
       ========================= */

    public List<string> GetAllBatches()
        => Directory.GetFiles(_batchDirectory, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => n != "run-queue")
            .ToList();

    public Batch GetBatch(string batchName)
    {
        var path = GetBatchPath(batchName);
        if (!File.Exists(path))
            throw new FileNotFoundException(batchName);

        return JsonSerializer.Deserialize<Batch>(
            File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        )!;
    }

    public List<Sample> GetSamples(string batchName)
        => GetBatch(batchName).Samples;

    public void SaveBatch(Batch batch)
    {
        File.WriteAllText(
            GetBatchPath(batch.BatchName),
            JsonSerializer.Serialize(batch,
                new JsonSerializerOptions { WriteIndented = true })
        );
    }

    /* =========================
       Run Queue
       ========================= */

    public List<BatchRunInfo> GetBatchRunQueue()
    {
        if (!File.Exists(_runQueueFile))
            return new();

        return JsonSerializer.Deserialize<List<BatchRunInfo>>(
            File.ReadAllText(_runQueueFile),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        )!;
    }

    public void EnqueueBatch(string batchName)
    {
        var runs = GetBatchRunQueue();
        if (runs.Any(r => r.BatchName == batchName))
            return;

        runs.Add(new BatchRunInfo
        {
            BatchName = batchName,
            Status = BatchRunStatus.Queued,
            QueuedAt = DateTime.UtcNow
        });

        SaveRunQueue(runs);
    }

    public void RemoveFromQueue(string batchName)
    {
        var runs = GetBatchRunQueue();
        var target = runs.SingleOrDefault(r => r.BatchName == batchName);
        if (target == null)
            return;

        // Safety: do not delete running batch
        if (target.Status == BatchRunStatus.Running ||
            target.Status == BatchRunStatus.Acquiring)
            return;

        runs.Remove(target);
        SaveRunQueue(runs);
    }

    /* =========================
       Manual Run
       ========================= */

    public void StartBatch(string batchName)
    {
        var runs = GetBatchRunQueue();

        if (runs.Any(r => r.Status == BatchRunStatus.Running))
            return;

        var run = runs.Single(r => r.BatchName == batchName);
        run.Status = BatchRunStatus.Running;
        run.InjectionStartTime = DateTime.UtcNow;

        BatchExecutionStore.Clear();

        var samples = GetSamples(batchName);
        var lcs = new[] { "HPLC1", "HPLC2", "HPLC3", "HPLC4" };

        for (int i = 0; i < samples.Count && i < lcs.Length; i++)
        {
            BatchExecutionStore.Executions.Add(new SampleExecutionInfo
            {
                BatchName = batchName,
                SampleName = samples[i].SampleName,
                LcId = lcs[i],
                UsesMS = i == 0
            });
        }

        SaveRunQueue(runs);
    }

    public void ClearRunQueue()
    {
        SaveRunQueue(new List<BatchRunInfo>());
        BatchExecutionStore.Clear();
    }

    private void SaveRunQueue(List<BatchRunInfo> runs)
    {
        File.WriteAllText(
            _runQueueFile,
            JsonSerializer.Serialize(
                runs,
                new JsonSerializerOptions { WriteIndented = true })
        );
    }

    private string GetBatchPath(string batchName)
        => Path.Combine(_batchDirectory, $"{batchName}.json");
}
