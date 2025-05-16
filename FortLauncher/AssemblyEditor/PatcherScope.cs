using System;

namespace FortLauncher;

[Flags]
internal enum PatcherScope
{
    /// <summary>
    /// Used for editing assembly and module definition and flags.
    /// </summary>
    Assembly,
    /// <summary>
    /// Required Type scope to be used.
    /// </summary>
    Field,
    /// <summary>
    /// Required Type scope to be used.
    /// </summary>
    Method,
    /// <summary>
    /// Required Type scope to be used.
    /// </summary>
    Property,
    /// <summary>
    /// Used for editing type settings.
    /// </summary>
    Type,
    /// <summary>
    /// Required Type scope to be used.
    /// </summary>
    NestedType
}