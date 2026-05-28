// Copyright (c) 2026 Incursa LLC.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Incursa.Qlog;

/// <summary>
/// Represents an explicit trace-conversion error entry in a contained qlog file.
/// </summary>
public sealed class QlogTraceError : QlogTraceComponent
{
    /// <summary>
    /// Gets or sets the required description of the conversion error.
    /// </summary>
    public string ErrorDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original URI or path used during attempted discovery.
    /// </summary>
    public string? Uri { get; set; }

    /// <summary>
    /// Gets or sets the optional vantage point associated with the failed trace.
    /// </summary>
    public QlogVantagePoint? VantagePoint { get; set; }
}
