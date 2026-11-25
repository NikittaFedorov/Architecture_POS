using System.Text.Json;
using System.Text.Json.Serialization;
using POS.Application.Reports;
using POS.Application;
using POS.Domain;

namespace POS.Infrastructure;

public sealed class JsonReportExporter : IReportExporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public string Format => "json";

    public void Write(AssessmentReport report, Stream output)
    {
        JsonSerializer.Serialize(output, new
        {
            report.GroupBy,
            report.Order,
            report.AverageMethod,
            report.Students,
            report.Teachers,
            Criteria = report.Rubric,
            Groups = report.Groups.Select(SerializeGroup)
        }, Options);
    }

    private static object SerializeGroup(TableGroup<ReportScoreRow> group) => new
    {
        Number = group.Key,
        group.AppliedWeight,
        group.Average,
        group.Total,
        group.Minimum,
        group.Maximum,
        ScoreCount = group.Rows.Count,
        Scores = group.Children.Count == 0 ? group.Rows : [],
        Children = group.Children.Select(SerializeGroup)
    };
}
