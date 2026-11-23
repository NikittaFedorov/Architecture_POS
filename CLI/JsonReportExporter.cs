using System.Text.Json;
using System.Text.Json.Serialization;
using Architecture_1.Application.Reports;

namespace Architecture_1.CLI;

internal sealed class JsonReportExporter : IReportExporter
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
        if (report.GroupBy == ReportGrouping.Student)
        {
            JsonSerializer.Serialize(output, new
            {
                report.GroupBy,
                report.AverageMethod,
                report.Students,
                report.Teachers,
                Criteria = report.Rubric,
                Scores = report.Groups.SelectMany(group => group.Scores),
                Summaries = report.Groups.Select(group => new
                {
                    StudentNumber = group.Number,
                    Criteria = group.StatisticsByCriterion.Select(criterion => new
                    {
                        criterion.CriterionNumber,
                        TeacherCount = criterion.ScoreCount,
                        criterion.Average,
                        criterion.LowestScore,
                        criterion.HighestScore
                    }),
                    group.Total,
                    group.AssessedCriteria
                })
            }, Options);
            return;
        }

        JsonSerializer.Serialize(output, new
        {
            report.GroupBy,
            report.AverageMethod,
            report.Students,
            report.Teachers,
            Criteria = report.Rubric,
            Groups = report.Groups.Select(group => new
            {
                group.Number,
                group.Scores,
                Criteria = group.StatisticsByCriterion,
                group.Total,
                group.AssessedCriteria
            })
        }, Options);
    }
}
