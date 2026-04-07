namespace Hplc.Controller.Api.Models.Instrument;

public enum LcState
{
    Idle,
    Injecting,
    Running,
    WaitingForMS
}