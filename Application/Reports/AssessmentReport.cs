using Architecture_1.Domain;

namespace Architecture_1.Application.Reports;

internal enum AverageMethod { Simple, Weighted }

internal enum ReportGrouping { Student, Teacher, Criterion }

internal sealed record ReportStudent(int Number, string Name);

internal sealed record ReportTeacher(int Number, string Name, decimal Weight);

internal sealed record ReportCriterion(int Number, string Name, decimal MaxScore, decimal Weight);

internal sealed record ReportGroup(
    int Number, IReadOnlyList<ReportScoreRow> Scores,
    IReadOnlyList<CriterionStatistics> StatisticsByCriterion, decimal? Total)
{
    public int AssessedCriteria => StatisticsByCriterion.Count(criterion => criterion.ScoreCount > 0);
}

internal sealed record AssessmentReport
{
    public required ReportGrouping GroupBy { get; init; }
    public required AverageMethod AverageMethod { get; init; }
    internal required bool UsesTeacherWeights { get; init; }
    public required IReadOnlyList<ReportStudent> Students { get; init; }
    public required IReadOnlyList<ReportTeacher> Teachers { get; init; }
    public required IReadOnlyList<ReportCriterion> Rubric { get; init; }
    public required IReadOnlyList<ReportGroup> Groups { get; init; }
}
