using POS.Domain;

namespace POS.Application.Reports;

internal sealed class AssessmentReportBuilder(
    IStudentRepository students,
    ITeacherRepository teachers,
    IRubricRepository rubric,
    IEvaluationRepository evaluations,
    AssessmentScope scope)
{
    private readonly HashSet<int> _studentNumbers = scope.StudentNumber is int student ? [student] : [];
    private readonly HashSet<int> _teacherNumbers = scope.TeacherNumber is int teacher ? [teacher] : [];
    private readonly HashSet<int> _criterionNumbers = [];
    private AverageMethod? _averageMethod;
    private ReportGrouping? _grouping;

    public void IncludeStudent(int number)
    {
        scope.EnsureStudentAllowed(number);
        _studentNumbers.Add(number);
    }

    public void IncludeTeacher(int number)
    {
        scope.EnsureTeacherAllowed(number);
        _teacherNumbers.Add(number);
    }

    public void IncludeCriterion(int number) => _criterionNumbers.Add(number);

    public void UseAverage(AverageMethod method)
    {
        if (_averageMethod.HasValue)
            throw new InvalidOperationException("Способ усреднения указывается один раз.");

        _averageMethod = method;
    }

    public void GroupBy(ReportGrouping grouping)
    {
        if (_grouping.HasValue)
            throw new InvalidOperationException("Группировка указывается один раз.");

        _grouping = grouping;
    }

    public AssessmentReport Build()
    {
        var selectedStudents = SelectByNumber(students.List(), _studentNumbers);
        var selectedTeachers = SelectByNumber(teachers.List(), _teacherNumbers);
        var selectedCriteria = SelectByNumber(rubric.List(), _criterionNumbers);
        var criteriaByNumber = selectedCriteria.ToDictionary(criterion => criterion.Number, criterion => criterion.Value);

        AverageMethod averageMethod = _averageMethod ?? AverageMethod.Simple;
        ReportGrouping grouping = _grouping ?? ReportGrouping.Student;
        bool weighted = averageMethod == AverageMethod.Weighted;
        IAverageStrategy average = weighted
            ? new WeightedAverageStrategy()
            : new SimpleAverage();

        var studentAxis = CreateAxis(selectedStudents, row => row.StudentNumber, _ => null, weighted);
        var teacherAxis = CreateAxis(selectedTeachers, row => row.TeacherNumber, teacher => teacher.Weight, weighted);
        var criterionAxis = CreateAxis(selectedCriteria, row => row.CriterionNumber, criterion => criterion.Weight, weighted)
            with { IncludeEmpty = true };

        ReportGrouping[] order = grouping switch
        {
            ReportGrouping.Student => [ReportGrouping.Student, ReportGrouping.Criterion, ReportGrouping.Teacher],
            ReportGrouping.Teacher => [ReportGrouping.Teacher, ReportGrouping.Student, ReportGrouping.Criterion],
            ReportGrouping.Criterion => [ReportGrouping.Criterion, ReportGrouping.Teacher, ReportGrouping.Student],
            _ => throw new ArgumentOutOfRangeException(nameof(grouping))
        };
        var axes = order.Select(kind => kind switch
        {
            ReportGrouping.Student => studentAxis,
            ReportGrouping.Teacher => teacherAxis,
            ReportGrouping.Criterion => criterionAxis,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        }).ToArray();
        axes[0] = axes[0] with { IncludeEmpty = true };

        var calculator = new ResultCalculator(average);
        var groups = calculator.Calculate(
            evaluations.ListReportScores(_studentNumbers, _teacherNumbers, _criterionNumbers),
            axes,
            getRawValue: row => row.Score,
            getNormalizedValue: row => row.Score / criteriaByNumber[row.CriterionNumber].MaxScore);
        return new AssessmentReport
        {
            Order = order,
            AverageMethod = averageMethod,
            Students = selectedStudents.Select(entry => new ReportStudent(entry.Number, entry.Value.Name)).ToArray(),
            Teachers = selectedTeachers.Select(entry =>
                new ReportTeacher(entry.Number, entry.Value.Name, entry.Value.Weight)).ToArray(),
            Rubric = selectedCriteria.Select(entry =>
                new ReportCriterion(entry.Number, entry.Value.Name, entry.Value.MaxScore, entry.Value.Weight)).ToArray(),
            Groups = groups
        };
    }

    private static TableAxis<ReportScoreRow> CreateAxis<T>(
        IReadOnlyList<Numbered<T>> entries, Func<ReportScoreRow, int> getGroupNumber,
        Func<T, decimal?> getMemberWeight, bool weighted)
    {
        var members = new Dictionary<int, decimal?>(entries.Count);
        foreach (var entry in entries)
        {
            decimal? weight = weighted ? getMemberWeight(entry.Value) : null;
            members.Add(entry.Number, weight);
        }

        return new TableAxis<ReportScoreRow>(getGroupNumber, members);
    }

    private static Numbered<T>[] SelectByNumber<T>(IReadOnlyList<Numbered<T>> entries, IReadOnlySet<int> numbers) =>
        entries.Where(entry => Includes(numbers, entry.Number)).ToArray();

    private static bool Includes(IReadOnlySet<int> numbers, int number) =>
        numbers.Count == 0 || numbers.Contains(number);
}
