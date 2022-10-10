namespace Reactor.Utilities;

/// <summary>
/// Class containing common priorities to be used in various places across Reactor.
/// </summary>
public static class ReactorPriority
{
    /// <summary>
    /// Lowest priority, will happen last.
    /// </summary>
    public const int Last = -800;

    /// <summary>
    /// Very low priority.
    /// </summary>
    public const int VeryLow = -600;

    /// <summary>
    /// Low priority.
    /// </summary>
    public const int Low = -400;

    /// <summary>
    /// Lower than normal priority.
    /// </summary>
    public const int LowerThanNormal = -200;

    /// <summary>
    /// Normal priority.
    /// </summary>
    public const int Normal = 0;

    /// <summary>
    /// Higher than normal priority.
    /// </summary>
    public const int HigherThanNormal = 200;

    /// <summary>
    /// High priority.
    /// </summary>
    public const int High = 400;

    /// <summary>
    /// Very high priority.
    /// </summary>
    public const int VeryHigh = 600;

    /// <summary>
    /// Highest priority, will happen first.
    /// </summary>
    public const int First = 800;
}
