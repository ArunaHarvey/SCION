namespace Hplc.Controller.Api.Models;

public enum BatchRunStatus
{
    Queued,
    Injecting,
    Running,
    WaitingForMS,
    Acquiring,   // MS owning
    Completed
}