public enum SampleExecutionState
{
    // Initial state
    Queued,

    // LC-only phases (parallelizable)
    LcPreparing,     // equilibration, injection, gradient setup
    WaitingForMS,    // LC ready, waiting to acquire MS

    // MS phase (serialized)
    Acquiring,       // MS acquisition in progress

    // LC-only post phase
    LcFinishing,     // column wash, re-equilibration

    // Final state
    Completed
}
