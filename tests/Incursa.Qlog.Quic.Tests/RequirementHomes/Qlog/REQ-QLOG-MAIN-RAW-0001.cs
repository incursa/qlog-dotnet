using Incursa.Qlog;
using Incursa.Qlog.Quic;
using Xunit;

namespace Incursa.Qlog.Quic.Tests.RequirementHomes.Qlog;

public sealed class REQ_QLOG_MAIN_RAW_0001
{
    [Fact]
    [Trait("Requirement", "REQ-QLOG-MAIN-S10-0001")]
    [Trait("CoverageType", "Edge")]
    public void Serialize_WritesLengthOnlyDataOnlyAndTruncatedRawInfoShapes()
    {
        QlogEvent lengthOnly = QlogQuicEvents.CreateDatagramDataMoved(
            0,
            new QuicDatagramDataMoved
            {
                Raw = new QuicRawInfo
                {
                    Length = 5,
                },
            });

        QlogEvent dataOnly = QlogQuicEvents.CreateDatagramDataMoved(
            1,
            new QuicDatagramDataMoved
            {
                Raw = new QuicRawInfo
                {
                    Data = "051428abff",
                },
            });

        QlogEvent truncated = QlogQuicEvents.CreateDatagramDataMoved(
            2,
            new QuicDatagramDataMoved
            {
                Raw = new QuicRawInfo
                {
                    Length = 5,
                    Data = "051428",
                },
            });

        Assert.Equal(QlogValue.Parse("""{"length":5}"""), lengthOnly.Data["raw"]);
        Assert.Equal(QlogValue.Parse("""{"data":"051428abff"}"""), dataOnly.Data["raw"]);
        Assert.Equal(QlogValue.Parse("""{"length":5,"data":"051428"}"""), truncated.Data["raw"]);
    }
}
