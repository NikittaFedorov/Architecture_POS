using POS.Domain;

namespace POS.Application.Reports;

public enum AverageMethod { Simple, Weighted }

public enum ReportGrouping { Student, Teacher, Criterion }

public sealed record ReportStudent(int Number, string Name);

public sealed record ReportTeacher(int Number, string Name, decimal Weight);

public sealed record ReportCriterion(int Number, string Name, decimal MaxScore, decimal Weight);

public sealed record ReportGroup(
    int Number, IReadOnlyList<ReportScoreRow> Scores,
    IReadOnlyList<CriterionStatistics> StatisticsByCriterion, decimal? Total)
{
    public int AssessedCriteria => StatisticsByCriterion.Count(criterion => criterion.ScoreCount > 0);
}

public sealed record AssessmentReport
{
    public required ReportGrouping GroupBy { get; init; }
    public required AverageMethod AverageMethod { get; init; }
    public required bool UsesTeacherWeights { get; init; }
    public required IReadOnlyList<ReportStudent> Students { get; init; }
    public required IReadOnlyList<ReportTeacher> Teachers { get; init; }
    public required IReadOnlyList<ReportCriterion> Rubric { get; init; }
    public required IReadOnlyList<ReportGroup> Groups { get; init; }
}
