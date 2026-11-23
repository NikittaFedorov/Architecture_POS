using Architecture_1.Application.Reports;
using Architecture_1.Domain;

namespace Architecture_1.Application;

internal sealed partial class AssessmentSession(
    IStudentRepository students,
    ITeacherRepository teachers,
    IRubricRepository rubric,
    IEvaluationRepository evaluations)
{
    public AssessmentScope Scope { get; private set; } = new(null, null);

    public Teacher SelectTeacher(int number)
    {
        if (!Scope.CanSelectTeacher)
            throw new InvalidOperationException("Сначала вернитесь в главный режим.");

        Teacher teacher = FindTeacher(number);
        Scope = new AssessmentScope(new Numbered<Teacher>(number, teacher), null);
        return teacher;
    }

    public Student SelectStudent(int number)
    {
        if (!Scope.CanSelectStudent)
            throw new InvalidOperationException("Сначала откройте режим преподавателя.");

        Student student = FindStudent(number);
        Scope = new AssessmentScope(Scope.Teacher, new Numbered<Student>(number, student));
        return student;
    }

    public void GoBack()
    {
        if (!Scope.CanGoBack)
            throw new InvalidOperationException("Вы уже в главном режиме.");

        Scope = new AssessmentScope(Scope.Student.HasValue ? Scope.Teacher : null, null);
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
        Scope.EnsureTeacherAllowed(teacher);
        Scope.EnsureStudentAllowed(student);
        return new EvaluationKey(student, teacher, criterion);
    }

    private void SetScore(SetScoreCommand command)
    {
        var key = CreateScopedEvaluationKey(command.Student, command.Teacher, command.Criterion);
        var evaluation = CreateEvaluation(key, command.Score);
        if (!evaluations.TryAdd(key, evaluation.Score))
            throw new InvalidOperationException("Оценка по этой тройке уже выставлена.");
    }

    private void CorrectScore(int student, int teacher, int criterion, decimal score)
    {
        var key = CreateScopedEvaluationKey(student, teacher, criterion);
        if (evaluations.FindScore(key) == null)
            throw new InvalidOperationException("Оценка по этой тройке не найдена.");

        var evaluation = CreateEvaluation(key, score);

        if (!evaluations.Update(key, evaluation.Score))
            throw new InvalidOperationException("Оценка по этой тройке не найдена.");
    }

    private void CancelScore(int student, int teacher, int criterion)
    {
        var key = CreateScopedEvaluationKey(student, teacher, criterion);
        if (!evaluations.Remove(key))
            throw new InvalidOperationException("Оценка по этой тройке не найдена.");
    }

    private Evaluation CreateEvaluation(EvaluationKey key, decimal score) =>
        new(FindStudent(key.Student), FindTeacher(key.Teacher), FindCriterion(key.Criterion), new Score(score));

    private Student FindStudent(int number) =>
        students.FindByNumber(number) ?? throw new InvalidOperationException($"Не найден студент №{number}.");

    private Teacher FindTeacher(int number) =>
        teachers.FindByNumber(number) ?? throw new InvalidOperationException($"Не найден преподаватель №{number}.");

    private Criterion FindCriterion(int number) =>
        rubric.FindByNumber(number) ?? throw new InvalidOperationException($"Не найден критерий №{number}.");

    public AssessmentReportBuilder CreateReportBuilder() =>
        new(students, teachers, rubric, evaluations, Scope);

    private IReadOnlyList<EvaluationRow> GetScores(int studentNumber)
    {
        Scope.EnsureStudentAllowed(studentNumber);
        FindStudent(studentNumber);
        return evaluations.ListScoresForStudent(studentNumber, Scope.TeacherNumber);
    }
}

internal readonly record struct SetScoreCommand(int Student, int Teacher, int Criterion, decimal Score);
