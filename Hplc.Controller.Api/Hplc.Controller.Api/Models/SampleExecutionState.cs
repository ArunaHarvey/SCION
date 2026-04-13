namespace Hplc.Controller.Api.Models;

public enum SampleExecutionState
{
    Queued,
    Preparing,
    WaitingForMS,
    Injecting,
    Acquiring,
    Completed,
    Failed
}