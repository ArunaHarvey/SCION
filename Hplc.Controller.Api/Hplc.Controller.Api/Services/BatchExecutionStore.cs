using Hplc.Controller.Api.Models;

namespace Hplc.Controller.Api.Services;

public static class BatchExecutionStore
{
    public static List<SampleExecutionInfo> Executions { get; } = new();

    public static void Clear()
    {
        Executions.Clear();
    }
}
