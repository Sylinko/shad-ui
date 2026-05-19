namespace ShadUI;

/// <summary>
///     Defines the result of a toast notification interaction.
/// </summary>
public enum ToastResult
{
    /// <summary>
    ///     The toast was not shown because no host was found.
    /// </summary>
    HostNotFound = -1,

    /// <summary>
    ///     The toast was dismissed by the user.
    /// </summary>
    Dismissed,

    /// <summary>
    ///     The toast was dismissed after the timer elapsed.
    /// </summary>
    TimerElapsed,

    /// <summary>
    ///     The toast's action button was clicked.
    /// </summary>
    ActionButtonClicked
}