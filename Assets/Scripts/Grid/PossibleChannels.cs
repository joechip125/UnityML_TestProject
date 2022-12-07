namespace DefaultNamespace.Grid
{
    [System.Flags]
    public enum PossibleChannels
    {
        None        = 0,
        Collectable = 1 << 0,
        Poison      = 1 << 1,
        Wall        = 1 << 2,
    }
}