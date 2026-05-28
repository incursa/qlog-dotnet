// Copyright (c) 2026 Incursa LLC.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Incursa.Qlog;

internal interface IQlogCaptureObserver
{
    ValueTask OnCapturedAsync(QlogCapturedEvent capturedEvent, CancellationToken cancellationToken);

    ValueTask OnSessionCompletedAsync(QlogCaptureSessionSnapshot sessionSnapshot, CancellationToken cancellationToken);
}
