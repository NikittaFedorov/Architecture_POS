using POS.Application.Reports;
using POS.Domain;

namespace POS.Application;

internal sealed partial class AssessmentSession(
    IStudentRepository students,
    ITeacherRepository teachers,
    IRubricRepository rubric,
    IEvaluationRepository evaluations)
{
    public AssessmentScope AuthScope { get; private set; } = new(null, null);

    public Teacher SelectTeacher(int number)
    {
        if (!AuthScope.CanSelectTeacher)
            throw new InvalidOperationException("Сначала вернитесь в главный режим.");

        Teacher teacher = FindTeacher(number);
        AuthScope = new AssessmentScope(new Numbered<Teacher>(number, teacher), null);
        return teacher;
    }

    public Student SelectStudent(int number)
    {
        if (!AuthScope.CanSelectStudent)
            throw new InvalidOperationException("Сначала откройте режим преподавателя.");

        Student student = FindStudent(number);
        AuthScope = new AssessmentScope(AuthScope.Teacher, new Numbered<Student>(number, student));
        return student;
    }

    public void GoBack()
    {
        if (!AuthScope.CanGoBack)
            throw new InvalidOperationException("Вы уже в главном режиме.");

        AuthScope = new AssessmentScope(AuthScope.Student.HasValue ? AuthScope.Teacher : null, null);
    }

    private IReadOnlyList<Numbered<Student>> Students => students.List();
    private IReadOnlyList<Numbered<Teacher>> Teachers => teachers.List();
    private IReadOnlyList<Numbered<Criterion>> Criteria => rubric.List();

    private int AddStudent(string name) => students.Add(new Student(Guid.NewGuid(), name));

    private int AddTeacher(string name, decimal weight = 1m) =>
        teachers.Add(new Teacher(Guid.NewGuid(), name, weight));

    private int AddCriterion(string name, decimal maxScore, decimal weight) =>
        rubric.Add(new Criterion(Guid.NewGuid(), name, maxScore, weight));

    private EvaluationKey CreateScopedEvaluationKey(int student, int teacher, int criterion)
    {
        AuthScope.EnsureTeacherAllowed(teacher);
        AuthScope.EnsureStudentAllowed(student);
        return new EvaluationKey(student, teacher, criterion);
    }

    private void SetScore(SetScoreCommand command)
    {
        var key = CreateScopedEvaluationKey(command.Student, command.Teacher, command.Criterion);
        var evaluation = CreateEvaluation(key, command.Score);
        if (!evaluations.TryAdd(key, evaluation.Score))
            throw EvaluationError(key, "уже выставлена");
    }

    private Score CorrectScore(int student, int teacher, int criterion, decimal score)
    {
        var key = CreateScopedEvaluationKey(student, teacher, criterion);
        Score previousScore = evaluations.FindScore(key)
            ?? throw EvaluationError(key, "не найдена");

        var evaluation = CreateEvaluation(key, score);

        if (!evaluations.Update(key, evaluation.Score))
            throw EvaluationError(key, "не найдена");

        return previousScore;
    }

    private Score CancelScore(int student, int teacher, int criterion)
    {
        var key = CreateScopedEvaluationKey(student, teacher, criterion);
        Score previousScore = evaluations.FindScore(key)
            ?? throw EvaluationError(key, "не найдена");
        if (!evaluations.Remove(key))
            throw EvaluationError(key, "не найдена");

        return previousScore;
    }

    private InvalidOperationException EvaluationError(EvaluationKey key, string problem) =>
        new($"Оценка {problem}: студент №{key.Student} {FindStudent(key.Student).Name} | преподаватель №{key.Teacher} {FindTeacher(key.Teacher).Name} | критерий №{key.Criterion} {FindCriterion(key.Criterion).Name}.");

    private Evaluation CreateEvaluation(EvaluationKey key, decimal score) =>
        new(FindStudent(key.Student), FindTeacher(key.Teacher), FindCriterion(key.Criterion), new Score(score));

    private Student FindStudent(int number) =>
        students.FindByNumber(number) ?? throw new InvalidOperationException($"Не найден студент №{number}.");

    private Teacher FindTeacher(int number) =>
        teachers.FindByNumber(number) ?? throw new InvalidOperationException($"Не найден преподаватель №{number}.");

    private Criterion FindCriterion(int number) =>
        rubric.FindByNumber(number) ?? throw new InvalidOperationException($"Не найден критерий №{number}.");

    public AssessmentReportBuilder CreateReportBuilder() =>
        new(students, teachers, rubric, evaluations, AuthScope);

    private IReadOnlyList<EvaluationRow> GetScores(int studentNumber)
    {
        AuthScope.EnsureStudentAllowed(studentNumber);
        FindStudent(studentNumber);
        return evaluations.ListScoresForStudent(studentNumber, AuthScope.TeacherNumber);
    }
}

internal readonly record struct SetScoreCommand(int Student, int Teacher, int Criterion, decimal Score);
