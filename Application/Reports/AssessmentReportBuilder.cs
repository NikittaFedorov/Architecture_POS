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
        int? studentNumber = _studentNumbers.Count == 1 ? _studentNumbers.Single() : null;
        int? teacherNumber = _teacherNumbers.Count == 1 ? _teacherNumbers.Single() : null;
        var scores = evaluations.ListReportScores(studentNumber, teacherNumber);
        var selectedStudents = SelectByNumber(students.List(), _studentNumbers);
        var selectedTeachers = SelectByNumber(teachers.List(), _teacherNumbers);
        var selectedCriteria = SelectByNumber(rubric.List(), _criterionNumbers);
        var selectedScores = scores.Where(score =>
            Includes(_studentNumbers, score.StudentNumber) &&
            Includes(_teacherNumbers, score.TeacherNumber) &&
            Includes(_criterionNumbers, score.CriterionNumber)).ToArray();

        AverageMethod averageMethod = _averageMethod ?? AverageMethod.Simple;
        ReportGrouping grouping = _grouping ?? ReportGrouping.Student;
        IAverageStrategy average = averageMethod == AverageMethod.Weighted
            ? new WeightedAverageStrategy()
            : new SimpleAverage();

        var calculator = new ResultCalculator(average);
        bool useTeacherWeights = averageMethod == AverageMethod.Weighted
            && grouping is ReportGrouping.Student or ReportGrouping.Criterion;
        var weightsByTeacher = useTeacherWeights
            ? selectedTeachers.ToDictionary(teacher => teacher.Number, teacher => teacher.Value.Weight)
            : null;
        var criteriaByNumber = selectedCriteria.ToDictionary(criterion => criterion.Number, criterion => criterion.Value);

        ReportGroup[] reportGroups = grouping switch
        {
            ReportGrouping.Student => BuildStudentGroups(),
            ReportGrouping.Teacher => BuildTeacherGroups(),
            ReportGrouping.Criterion => BuildCriterionGroups(),
            _ => throw new ArgumentOutOfRangeException(nameof(grouping))
        };

        return new AssessmentReport
        {
            GroupBy = grouping,
            AverageMethod = averageMethod,
            UsesTeacherWeights = useTeacherWeights,
            Students = selectedStudents.Select(entry => new ReportStudent(entry.Number, entry.Value.Name)).ToArray(),
            Teachers = selectedTeachers.Select(entry =>
                new ReportTeacher(entry.Number, entry.Value.Name, entry.Value.Weight)).ToArray(),
            Rubric = selectedCriteria.Select(entry =>
                new ReportCriterion(entry.Number, entry.Value.Name, entry.Value.MaxScore, entry.Value.Weight)).ToArray(),
            Groups = reportGroups
        };

        ReportGroup[] BuildStudentGroups()
        {
            var scoresByStudent = selectedScores.ToLookup(score => score.StudentNumber);
            var groups = new List<ReportGroup>(selectedStudents.Length);
            foreach (var student in selectedStudents)
            {
                var studentScores = scoresByStudent[student.Number].ToArray();
                groups.Add(CreateGroup(student.Number, selectedCriteria, studentScores));
            }

            return groups.ToArray();
        }

        ReportGroup[] BuildTeacherGroups()
        {
            var scoresByTeacher = selectedScores.ToLookup(score => score.TeacherNumber);
            var groups = new List<ReportGroup>(selectedTeachers.Length);
            foreach (var teacher in selectedTeachers)
            {
                var teacherScores = scoresByTeacher[teacher.Number].ToArray();
                groups.Add(CreateGroup(teacher.Number, selectedCriteria, teacherScores));
            }

            return groups.ToArray();
        }

        ReportGroup[] BuildCriterionGroups()
        {
            var scoresByCriterion = selectedScores.ToLookup(score => score.CriterionNumber);
            var groups = new List<ReportGroup>(selectedCriteria.Length);
            foreach (var criterion in selectedCriteria)
            {
                var criterionScores = scoresByCriterion[criterion.Number].ToArray();
                groups.Add(CreateGroup(criterion.Number, [criterion], criterionScores));
            }

            return groups.ToArray();
        }

        ReportGroup CreateGroup(int number, IReadOnlyList<Numbered<Criterion>> groupCriteria,
            IReadOnlyList<ReportScoreRow> groupScores)
        {
            var scoresByCriterion = groupScores.ToLookup(score => score.CriterionNumber);
            var statistics = new List<CriterionStatistics>(groupCriteria.Count);
            foreach (var criterion in groupCriteria)
            {
                var criterionScores = scoresByCriterion[criterion.Number]
                    .Select(score => new WeightedScore(new Score(score.Score),
                        weightsByTeacher?[score.TeacherNumber] ?? 1m))
                    .ToArray();
                statistics.Add(calculator.CalculateCriterionStatistics(criterion.Number, criterionScores));
            }

            decimal? total = grouping is ReportGrouping.Student or ReportGrouping.Teacher
                ? calculator.CalculateTotal(statistics, criteriaByNumber)
                : null;
            return new ReportGroup(number, groupScores, statistics, total);
        }
    }

    private static Numbered<T>[] SelectByNumber<T>(IReadOnlyList<Numbered<T>> entries, IReadOnlySet<int> numbers) =>
        entries.Where(entry => Includes(numbers, entry.Number)).ToArray();

    private static bool Includes(IReadOnlySet<int> numbers, int number) =>
        numbers.Count == 0 || numbers.Contains(number);
}
