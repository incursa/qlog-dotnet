using System.Text.Json;
using Incursa.Qlog;
using Xunit;

namespace Incursa.Qlog.Quic.Tests.Fixtures;

internal static class QlogFixtureAssertions
{
    internal static void AssertJsonEquivalent(string expectedJson, QlogValue actual)
    {
        using JsonDocument expectedDocument = JsonDocument.Parse(expectedJson);
        using JsonDocument actualDocument = JsonDocument.Parse(actual.ToJson());

        Assert.True(
            JsonElement.DeepEquals(expectedDocument.RootElement, actualDocument.RootElement),
            $"Expected JSON value {expectedDocument.RootElement.GetRawText()} but received {actualDocument.RootElement.GetRawText()}.");
    }
}
