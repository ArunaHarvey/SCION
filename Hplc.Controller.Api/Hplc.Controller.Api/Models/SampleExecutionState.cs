namespace Hplc.Controller.Api.Models;

public enum SampleExecutionState
{
    Queued,
    Injecting,
    Running,
    WaitingForMS,
    Acquiring,
    Completed
}