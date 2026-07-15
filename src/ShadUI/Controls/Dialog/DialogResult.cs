// ReSharper disable once CheckNamespace

namespace ShadUI;

public enum DialogResult
{
    /// <summary>The dialog was not shown because no host was registered.</summary>
    HostNotFound = -1,

    Primary,
    Secondary,
    Tertiary,
    Cancel
}