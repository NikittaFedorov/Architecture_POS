using Microsoft.Data.Sqlite;

namespace Architecture_1.Infrastructure.Sqlite;

public sealed class SqliteDatabase(string path) : RelationalDatabase(
    new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = path, ForeignKeys = true }.ToString()),
    path,
    """
    CREATE TABLE IF NOT EXISTS students (
        number INTEGER PRIMARY KEY, id TEXT NOT NULL UNIQUE, name TEXT NOT NULL
    );
    CREATE TABLE IF NOT EXISTS teachers (
        number INTEGER PRIMARY KEY, id TEXT NOT NULL UNIQUE, name TEXT NOT NULL,
        weight TEXT NOT NULL
    );
    CREATE TABLE IF NOT EXISTS criteria (
        number INTEGER PRIMARY KEY, id TEXT NOT NULL UNIQUE, name TEXT NOT NULL,
        max_score TEXT NOT NULL, weight TEXT NOT NULL
    );
    CREATE TABLE IF NOT EXISTS evaluations (
        number INTEGER PRIMARY KEY,
        student INTEGER NOT NULL REFERENCES students(number),
        teacher INTEGER NOT NULL REFERENCES teachers(number),
        criterion INTEGER NOT NULL REFERENCES criteria(number),
        score_tenths INTEGER NOT NULL CHECK (typeof(score_tenths) = 'integer' AND score_tenths >= 0),
        UNIQUE(student, teacher, criterion)
    );
    SELECT score_tenths FROM evaluations WHERE 1 = 0;
    """);
