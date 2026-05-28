// Copyright (c) 2026 Incursa LLC.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Incursa.Qlog;

/// <summary>
/// Represents an entry in a contained qlog file's <c>traces</c> array.
/// </summary>
public abstract class QlogTraceComponent
{
    /// <summary>
    /// Gets additional members that should round-trip with the component.
    /// </summary>
    public IDictionary<string, QlogValue> ExtensionData { get; } = new Dictionary<string, QlogValue>(StringComparer.Ordinal);
}
