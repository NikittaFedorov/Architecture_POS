using POS.Domain;

namespace POS.Application;

public interface IStudentRepository
{
    int Add(Student student);
    Student? FindByNumber(int number);
    IReadOnlyList<Numbered<Student>> List();
}

public interface ITeacherRepository
{
    int Add(Teacher teacher);
    Teacher? FindByNumber(int number);
    IReadOnlyList<Numbered<Teacher>> List();
}

public interface IRubricRepository
{
    int Add(Criterion criterion);
    Criterion? FindByNumber(int number);
    IReadOnlyList<Numbered<Criterion>> List();
}

public interface IEvaluationRepository
{
    Score? FindScore(EvaluationKey key);
    bool TryAdd(EvaluationKey key, Score score);
    bool Update(EvaluationKey key, Score score);
    bool Remove(EvaluationKey key);
    IReadOnlyList<EvaluationRow> ListScoresForStudent(int studentNumber, int? teacherNumber);
    IReadOnlyList<ReportScoreRow> ListReportScores(int? studentNumber, int? teacherNumber);
}

public readonly record struct Numbered<T>(int Number, T Value);

public readonly record struct EvaluationKey(int Student, int Teacher, int Criterion);

public readonly record struct EvaluationRow(
    int TeacherNumber, string Teacher, int CriterionNumber, string Criterion, Score Score);

public readonly record struct ReportScoreRow(
    int StudentNumber, int TeacherNumber, int CriterionNumber, decimal Score);
