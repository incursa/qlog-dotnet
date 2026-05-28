// Copyright (c) 2026 Incursa LLC.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Incursa.Qlog;

/// <summary>
/// Represents shared per-trace qlog fields.
/// </summary>
public sealed class QlogCommonFields
{
    /// <summary>
    /// Gets or sets the optional tuple identifier.
    /// </summary>
    public string? Tuple { get; set; }

    /// <summary>
    /// Gets or sets the optional time-format identifier.
    /// </summary>
    public string? TimeFormat { get; set; }

    /// <summary>
    /// Gets or sets the optional reference-time metadata.
    /// </summary>
    public QlogReferenceTime? ReferenceTime { get; set; }

    /// <summary>
    /// Gets or sets the optional group identifier.
    /// </summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// Gets unknown or extension fields that should survive round-tripping.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}
