using POS.Application;
using POS.Domain;
using System.Data.Common;

namespace POS.Infrastructure;

public class SqlStudentRepository(RelationalDatabase database) : IStudentRepository
{
    public int Add(Student student) =>
        database.Query(InsertSql, row => row.GetInt32(0), ("id", student.Id), ("name", student.Name)).Single();

    public Student? FindByNumber(int number)
    {
        var matchingStudents = database.Query(FindByNumberSql, ReadStudentRow, ("number", number));
        if (matchingStudents.Count == 0)
            return null;

        return matchingStudents.Single().Value;
    }

    public IReadOnlyList<Numbered<Student>> List() =>
        database.Query(ListSql, ReadStudentRow);

    private static Numbered<Student> ReadStudentRow(DbDataReader row)
    {
        int number = row.GetInt32(row.GetOrdinal("number"));
        Guid id = Guid.Parse(row.GetString(row.GetOrdinal("id")));
        string name = row.GetString(row.GetOrdinal("name"));

        var student = new Student(id, name);
        return new Numbered<Student>(number, student);
    }

    protected virtual string InsertSql => """
        INSERT INTO students (id, name) VALUES ($id, $name) RETURNING number;
        """;

    protected virtual string FindByNumberSql => "SELECT number, id, name FROM students WHERE number = $number";

    protected virtual string ListSql => "SELECT number, id, name FROM students ORDER BY number";
}

public class SqlTeacherRepository(RelationalDatabase database) : ITeacherRepository
{
    public int Add(Teacher teacher) =>
        database.Query(InsertSql, row => row.GetInt32(0), ("id", teacher.Id), ("name", teacher.Name),
            ("weight", teacher.Weight)).Single();

    public Teacher? FindByNumber(int number)
    {
        var matchingTeachers = database.Query(FindByNumberSql, ReadTeacherRow, ("number", number));
        if (matchingTeachers.Count == 0)
            return null;

        return matchingTeachers.Single().Value;
    }

    public IReadOnlyList<Numbered<Teacher>> List() =>
        database.Query(ListSql, ReadTeacherRow);

    private static Numbered<Teacher> ReadTeacherRow(DbDataReader row)
    {
        int number = row.GetInt32(row.GetOrdinal("number"));
        Guid id = Guid.Parse(row.GetString(row.GetOrdinal("id")));
        string name = row.GetString(row.GetOrdinal("name"));
        decimal weight = RelationalDatabase.ReadDecimal(row, row.GetOrdinal("weight"));

        var teacher = new Teacher(id, name, weight);
        return new Numbered<Teacher>(number, teacher);
    }

    protected virtual string InsertSql => """
        INSERT INTO teachers (id, name, weight) VALUES ($id, $name, $weight) RETURNING number;
        """;

    protected virtual string FindByNumberSql => "SELECT number, id, name, weight FROM teachers WHERE number = $number";

    protected virtual string ListSql => "SELECT number, id, name, weight FROM teachers ORDER BY number";
}

public class SqlRubricRepository(RelationalDatabase database) : IRubricRepository
{
    public int Add(Criterion criterion) =>
        database.Query(InsertSql, row => row.GetInt32(0), ("id", criterion.Id), ("name", criterion.Name),
            ("maximum", criterion.MaxScore), ("weight", criterion.Weight)).Single();

    public Criterion? FindByNumber(int number)
    {
        var matchingCriteria = database.Query(FindByNumberSql, ReadCriterionRow, ("number", number));
        if (matchingCriteria.Count == 0)
            return null;

        return matchingCriteria.Single().Value;
    }

    public IReadOnlyList<Numbered<Criterion>> List() =>
        database.Query(ListSql, ReadCriterionRow);

    private static Numbered<Criterion> ReadCriterionRow(DbDataReader row)
    {
        int number = row.GetInt32(row.GetOrdinal("number"));
        Guid id = Guid.Parse(row.GetString(row.GetOrdinal("id")));
        string name = row.GetString(row.GetOrdinal("name"));
        decimal maxScore = RelationalDatabase.ReadDecimal(row, row.GetOrdinal("max_score"));
        decimal weight = RelationalDatabase.ReadDecimal(row, row.GetOrdinal("weight"));

        var criterion = new Criterion(id, name, maxScore, weight);
        return new Numbered<Criterion>(number, criterion);
    }

    protected virtual string InsertSql => """
        INSERT INTO criteria (id, name, max_score, weight)
        VALUES ($id, $name, $maximum, $weight) RETURNING number;
        """;

    protected virtual string FindByNumberSql => "SELECT number, id, name, max_score, weight FROM criteria WHERE number = $number";

    protected virtual string ListSql => "SELECT number, id, name, max_score, weight FROM criteria ORDER BY number";
}
