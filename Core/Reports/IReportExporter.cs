namespace POS.Application.Reports;

public interface IReportExporter
{
    string Format { get; }
    void Write(AssessmentReport report, Stream output);
}
