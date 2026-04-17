using System.Threading;
using System.Threading.Tasks;

namespace Incursa.Qlog;

internal interface IQlogCaptureObserver
{
    ValueTask OnCapturedAsync(QlogCapturedEvent capturedEvent, CancellationToken cancellationToken);

    ValueTask OnSessionCompletedAsync(QlogCaptureSessionSnapshot sessionSnapshot, CancellationToken cancellationToken);
}
