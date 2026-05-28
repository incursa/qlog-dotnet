// Copyright (c) 2026 Incursa LLC.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Incursa.Qlog;

/// <summary>
/// Represents the qlog event importance levels used by authored event schemas.
/// </summary>
public enum QlogEventImportanceLevel
{
    /// <summary>
    /// Core importance level.
    /// </summary>
    Core = 0,

    /// <summary>
    /// Base importance level.
    /// </summary>
    Base = 1,

    /// <summary>
    /// Extra importance level.
    /// </summary>
    Extra = 2,
}
