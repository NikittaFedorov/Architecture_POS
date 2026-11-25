using System.Data.Common;
using POS.Application;
using POS.Domain;

namespace POS.Infrastructure;

public class SqlEvaluationRepository(RelationalDatabase database) : IEvaluationRepository
{
    public Score? FindScore(EvaluationKey key)
    {
        var scores = database.Query(FindScoreSql, ReadScore,
            ("student", key.Student), ("teacher", key.Teacher), ("criterion", key.Criterion));
        if (scores.Count == 0)
            return null;

        return scores.Single();
    }

    public bool TryAdd(EvaluationKey key, Score score) =>
        database.Query(InsertScoreSql, row => row.GetInt32(0), ("student", key.Student), ("teacher", key.Teacher), ("criterion", key.Criterion),
            ("score", ToTenths(score))).Count == 1;

    public bool Update(EvaluationKey key, Score score) =>
        database.Query(UpdateScoreSql, row => row.GetInt32(0), ("student", key.Student), ("teacher", key.Teacher), ("criterion", key.Criterion),
            ("score", ToTenths(score))).Count == 1;

    public bool Remove(EvaluationKey key) =>
        database.Query(DeleteScoreSql, row => row.GetInt32(0), ("student", key.Student), ("teacher", key.Teacher), ("criterion", key.Criterion)).Count == 1;

    public IReadOnlyList<EvaluationRow> ListScoresForStudent(int studentNumber, int? teacherNumber) =>
        database.Query(ListScoresForStudentSql, ReadEvaluationRow,
            ("student", studentNumber), ("teacher", teacherNumber));

    public IReadOnlyList<ReportScoreRow> ListReportScores(
        IReadOnlyCollection<int> studentNumbers,
        IReadOnlyCollection<int> teacherNumbers,
        IReadOnlyCollection<int> criterionNumbers)
    {
        var conditions = new List<string>();
        var parameters = new List<(string Name, object? Value)>();
        AddFilter("student", studentNumbers);
        AddFilter("teacher", teacherNumbers);
        AddFilter("criterion", criterionNumbers);

        string where = conditions.Count == 0 ? "" : " WHERE " + string.Join(" AND ", conditions);
        return database.Query(ListReportScoresSql + where + " ORDER BY student, criterion, teacher",
            ReadReportScore, parameters.ToArray());

        void AddFilter(string column, IReadOnlyCollection<int> numbers)
        {
            if (numbers.Count == 0)
                return;

            var placeholders = new List<string>(numbers.Count);
            foreach (int number in numbers)
            {
                string name = $"{column}{placeholders.Count}";
                placeholders.Add("$" + name);
                parameters.Add((name, number));
            }
            conditions.Add($"{column} IN ({string.Join(", ", placeholders)})");
        }
    }

    private static ReportScoreRow ReadReportScore(DbDataReader row)
    {
        int student = row.GetInt32(row.GetOrdinal("student"));
        int teacher = row.GetInt32(row.GetOrdinal("teacher"));
        int criterion = row.GetInt32(row.GetOrdinal("criterion"));
        return new ReportScoreRow(student, teacher, criterion, ReadScore(row).Value);
    }

    private static EvaluationRow ReadEvaluationRow(DbDataReader row)
    {
        int teacherNumber = row.GetInt32(row.GetOrdinal("teacher"));
        string teacherName = row.GetString(row.GetOrdinal("teacher_name"));
        int criterionNumber = row.GetInt32(row.GetOrdinal("criterion"));
        string criterionName = row.GetString(row.GetOrdinal("criterion_name"));
        Score score = ReadScore(row);

        return new EvaluationRow(teacherNumber, teacherName, criterionNumber, criterionName, score);
    }

    private static Score ReadScore(DbDataReader row)
    {
        long tenths = row.GetInt64(row.GetOrdinal("score_tenths"));
        return new Score(tenths / 10m);
    }

    private static long ToTenths(Score score)
    {
        if (score.Value > long.MaxValue / 10m)
            throw new ArgumentOutOfRangeException(nameof(score), "Балл слишком велик для хранения.");

        return (long)(score.Value * 10m);
    }

    protected virtual string FindScoreSql => """
        SELECT score_tenths FROM evaluations
        WHERE student = $student AND teacher = $teacher AND criterion = $criterion
        """;

    protected virtual string InsertScoreSql => """
        INSERT INTO evaluations (student, teacher, criterion, score_tenths)
        VALUES ($student, $teacher, $criterion, $score)
        ON CONFLICT(student, teacher, criterion) DO NOTHING
        RETURNING number
        """;

    protected virtual string UpdateScoreSql => """
        UPDATE evaluations SET score_tenths = $score
        WHERE student = $student AND teacher = $teacher AND criterion = $criterion
        RETURNING number
        """;

    protected virtual string DeleteScoreSql => """
        DELETE FROM evaluations
        WHERE student = $student AND teacher = $teacher AND criterion = $criterion
        RETURNING number
        """;

    protected virtual string ListReportScoresSql => """
        SELECT student, teacher, criterion, score_tenths
        FROM evaluations
        """;

    protected virtual string ListScoresForStudentSql => """
        SELECT teacher, teachers.name AS teacher_name, criterion,
            criteria.name AS criterion_name, score_tenths
        FROM evaluations
        JOIN teachers ON teachers.number = evaluations.teacher
        JOIN criteria ON criteria.number = evaluations.criterion
        WHERE student = $student AND ($teacher IS NULL OR teacher = $teacher)
        ORDER BY evaluations.number
        """;
}
