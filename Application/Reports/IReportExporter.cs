namespace Architecture_1.Application.Reports;

internal interface IReportExporter
{
    string Format { get; }
    void Write(AssessmentReport report, Stream output);
}
