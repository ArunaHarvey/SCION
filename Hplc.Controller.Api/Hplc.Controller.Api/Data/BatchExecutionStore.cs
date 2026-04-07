using Hplc.Controller.Api.Models;

namespace Hplc.Controller.Api.Data;

public static class BatchExecutionStore
{
    public static DateTime? BatchStartTime;
    public static DateTime? BatchEndTime;

    // For summary calculation
    public static TimeSpan? EstimatedSequentialDuration;
}