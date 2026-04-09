using System.Collections.Concurrent;
using Hplc.Controller.Api.Models;

namespace Hplc.Controller.Api.Services;

public class BatchFileService
{
    private static readonly ConcurrentDictionary<string, BatchRunInfo> Batches = new();

    /* ======================================================
     * PUBLIC API – USED BY CONTROLLERS
     * ====================================================== */

    public List<BatchRunInfo> GetBatchRunQueue()
        => Batches.Values
                  .OrderBy(b => b.QueuePosition ?? int.MaxValue)
                  .ToList();

    public void EnqueueBatch(string batchName)
    {
        if (Batches.ContainsKey(batchName))
            return;

        var batch = new BatchRunInfo
        {
            BatchName = batchName,
            Status = BatchRunStatus.Queued,
            QueuePosition = Batches.Count + 1,
            OwnsMS = false,
            Samples = LoadSamples(batchName)
        };

        Batches.TryAdd(batchName, batch);
    }

    public void StartBatch(string batchName)
    {
        if (!Batches.TryGetValue(batchName, out var batch))
            return;

        // Only queued batches can be started
        if (batch.Status != BatchRunStatus.Queued)
            return;

        // Only one batch may run at a time
        if (Batches.Values.Any(IsBatchActive))
            return;

        ExecuteBatch(batch);
    }

    public void RemoveFromQueue(string batchName)
        => Batches.TryRemove(batchName, out _);

    public void ClearRunQueue()
        => Batches.Clear();

    public void SaveBatch(BatchRunInfo batch)
        => Batches[batch.BatchName] = batch;

    public List<SampleExecutionInfo> GetSamples(string batchName)
        => Batches.TryGetValue(batchName, out var batch)
           ? batch.Samples
           : new();

    /* ======================================================
     * CORE EXECUTION LOGIC – FIXED DESIGN
     * ====================================================== */

    private void ExecuteBatch(BatchRunInfo batch)
    {
        // ✅ CORRECT: Batch starts RUNNING, not Injecting
        batch.Status = BatchRunStatus.Running;
        batch.InjectionStartTime = DateTime.UtcNow;

        _ = Task.Run(async () =>
        {
            foreach (var sample in batch.Samples)
            {
                // --- Sample Injection ---
                sample.State = SampleExecutionState.Injecting;
                batch.Status = BatchRunStatus.Injecting;
                await Task.Delay(1000);

                // --- Sample Running ---
                sample.State = SampleExecutionState.Running;
                batch.Status = BatchRunStatus.Running;
                await Task.Delay(2000);

                // --- Sample Acquisition (MS) ---
                if (sample.UsesMS)
                {
                    sample.State = SampleExecutionState.Acquiring;
                    batch.Status = BatchRunStatus.Acquiring;
                    await Task.Delay(2000);
                }

                // --- Sample Completed ---
                sample.State = SampleExecutionState.Completed;
            }

            // ✅ Batch completes ONLY after ALL samples complete
            if (batch.Samples.All(s => s.State == SampleExecutionState.Completed))
            {
                batch.Status = BatchRunStatus.Completed;
                batch.CompletionTime = DateTime.UtcNow;

                batch.ActualDuration =
                    (batch.CompletionTime.Value - batch.InjectionStartTime!.Value)
                    .ToString(@"mm\:ss");
            }

            TryStartNextBatch();
        });
    }

    /* ======================================================
     * QUEUE MANAGEMENT
     * ====================================================== */

    private void TryStartNextBatch()
    {
        if (Batches.Values.Any(IsBatchActive))
            return;

        var next = Batches.Values
            .OrderBy(b => b.QueuePosition)
            .FirstOrDefault(b => b.Status == BatchRunStatus.Queued);

        if (next != null)
            ExecuteBatch(next);
    }

    private static bool IsBatchActive(BatchRunInfo b)
        => b.Status is BatchRunStatus.Injecting
                     or BatchRunStatus.Running
                     or BatchRunStatus.Acquiring;

    /* ======================================================
     * SAMPLE LOADER – STUB (SAFE TO REPLACE LATER)
     * ====================================================== */

    private static List<SampleExecutionInfo> LoadSamples(string batchName)
        => new()
        {
            new()
            {
                BatchName = batchName,
                SampleName = "Sample-1",
                UsesMS = true,
                State = SampleExecutionState.Queued
            },
            new()
            {
                BatchName = batchName,
                SampleName = "Sample-2",
                UsesMS = false,
                State = SampleExecutionState.Queued
            },
            new()
            {
                BatchName = batchName,
                SampleName = "Sample-3",
                UsesMS = true,
                State = SampleExecutionState.Queued
            }
        };
}