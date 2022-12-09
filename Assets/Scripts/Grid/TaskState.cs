namespace Grid
{
    [System.Flags]
    public enum TaskState
    {
        None         = 0,
        Assigned     = 1 << 0,
        Completed    = 1 << 1,
        MaxCompleted = 1 << 2,
    }
}