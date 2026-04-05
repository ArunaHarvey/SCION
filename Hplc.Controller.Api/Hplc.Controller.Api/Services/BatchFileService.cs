using System.Text.Json;
using Hplc.Controller.Api.Models;

namespace Hplc.Controller.Api.Services;

public class BatchFileService
{
    private readonly string _batchDirectory;
    private readonly string _runQueueFile;

    public BatchFileService(IWebHostEnvironment env)
    {
        _batchDirectory = Path.Combine(
            env.ContentRootPath,
            "Data",
            "Batches"
        );

        _runQueueFile = Path.Combine(
            _batchDirectory,
            "run-queue.json"
        );

        Directory.CreateDirectory(_batchDirectory);
    }

    /* =========================
       Batch APIs
       ========================= */

    public List<string> GetAllBatches()
    {
        return Directory.GetFiles(_batchDirectory, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => !string.IsNullOrWhiteSpace(n) && n != "run-queue")
            .Cast<string>()
            .ToList();
    }

    public Batch GetBatch(string batchName)
    {
        var path = GetBatchPath(batchName);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Batch '{batchName}' not found");

        var json = File.ReadAllText(path);

        // ✅ CRITICAL FIX: Case‑insensitive deserialization
        return JsonSerializer.Deserialize<Batch>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        ) ?? new Batch();
    }

    public List<Sample> GetSamples(string batchName)
    {
        return GetBatch(batchName).Samples;
    }

    public void SaveBatch(Batch batch)
    {
        var json = JsonSerializer.Serialize(
            batch,
            new JsonSerializerOptions { WriteIndented = true }
        );

        File.WriteAllText(GetBatchPath(batch.BatchName), json);
    }

    /* =========================
       Batch Run Queue
       ========================= */

    public List<BatchRunInfo> GetBatchRunQueue()
    {
        if (!File.Exists(_runQueueFile))
            return new();

        var json = File.ReadAllText(_runQueueFile);

        return JsonSerializer.Deserialize<List<BatchRunInfo>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        ) ?? new();
    }

    public void EnqueueBatch(string batchName)
    {
        GetBatch(batchName); // ensure batch exists

        var queue = GetBatchRunQueue();

        if (queue.Any(q => q.BatchName == batchName))
            return;

        queue.Add(new BatchRunInfo
        {
            BatchName = batchName,
            Status = "Queued",
            QueuePosition = queue.Count + 1
        });

        File.WriteAllText(
            _runQueueFile,
            JsonSerializer.Serialize(
                queue,
                new JsonSerializerOptions { WriteIndented = true }
            )
        );
    }

    /* =========================
       Helpers
       ========================= */

    private string GetBatchPath(string batchName)
        => Path.Combine(_batchDirectory, $"{batchName}.json");
}
