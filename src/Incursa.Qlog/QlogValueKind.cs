// Copyright (c) 2026 Incursa LLC.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Incursa.Qlog;

/// <summary>
/// Identifies the JSON-compatible kind carried by a <see cref="QlogValue"/>.
/// </summary>
public enum QlogValueKind
{
    /// <summary>
    /// The value is `null` or omitted.
    /// </summary>
    Null,

    /// <summary>
    /// The value is a JSON Boolean.
    /// </summary>
    Boolean,

    /// <summary>
    /// The value is a JSON number.
    /// </summary>
    Number,

    /// <summary>
    /// The value is a JSON string.
    /// </summary>
    String,

    /// <summary>
    /// The value is a JSON object.
    /// </summary>
    Object,

    /// <summary>
    /// The value is a JSON array.
    /// </summary>
    Array,
}
