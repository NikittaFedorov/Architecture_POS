using System.Data.Common;
using DuckDB.NET.Data;

namespace POS.Infrastructure.DuckDb;

public sealed class DuckDbDatabase(string path) : RelationalDatabase(
    new DuckDBConnection(new DbConnectionStringBuilder { ["Data Source"] = path }.ToString()),
    path,
    """
    CREATE SEQUENCE IF NOT EXISTS student_numbers;
    CREATE SEQUENCE IF NOT EXISTS teacher_numbers;
    CREATE SEQUENCE IF NOT EXISTS criterion_numbers;
    CREATE SEQUENCE IF NOT EXISTS evaluation_numbers;
    CREATE TABLE IF NOT EXISTS students (
        number INTEGER PRIMARY KEY DEFAULT nextval('student_numbers'),
        id TEXT NOT NULL UNIQUE, name TEXT NOT NULL
    );
    CREATE TABLE IF NOT EXISTS teachers (
        number INTEGER PRIMARY KEY DEFAULT nextval('teacher_numbers'),
        id TEXT NOT NULL UNIQUE, name TEXT NOT NULL, weight TEXT NOT NULL
    );
    CREATE TABLE IF NOT EXISTS criteria (
        number INTEGER PRIMARY KEY DEFAULT nextval('criterion_numbers'),
        id TEXT NOT NULL UNIQUE, name TEXT NOT NULL, max_score TEXT NOT NULL, weight TEXT NOT NULL
    );
    CREATE TABLE IF NOT EXISTS evaluations (
        number INTEGER PRIMARY KEY DEFAULT nextval('evaluation_numbers'),
        student INTEGER NOT NULL REFERENCES students(number),
        teacher INTEGER NOT NULL REFERENCES teachers(number),
        criterion INTEGER NOT NULL REFERENCES criteria(number),
        score_tenths BIGINT NOT NULL CHECK (score_tenths >= 0),
        UNIQUE(student, teacher, criterion)
    );
    SELECT score_tenths FROM evaluations WHERE 1 = 0;
    """);
