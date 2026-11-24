using POS.Application.Reports;

namespace POS.Infrastructure;

public sealed class TextReportExporter(Action<AssessmentReport, TextWriter> writeReport) : IReportExporter
{
    public string Format => "txt";

    public void Write(AssessmentReport report, Stream output)
    {
        using var writer = new StreamWriter(output, leaveOpen: true);
        writeReport(report, writer);
    }
}
