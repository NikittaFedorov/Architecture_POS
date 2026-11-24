using POS.Application.Reports;
using POS.CLI;
using POS.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

#if DUCKDB
using AssessmentDatabase = POS.Infrastructure.DuckDb.DuckDbDatabase;
const string defaultDatabasePath = "assessment.duckdb";
#else
using AssessmentDatabase = POS.Infrastructure.Sqlite.SqliteDatabase;
const string defaultDatabasePath = "assessment.db";
#endif

string databasePath = Environment.GetEnvironmentVariable("ASSESSMENT_DB_PATH") ?? defaultDatabasePath;
return AssessmentCli.Run(services =>
{
    RelationalStorageServices.AddRelationalStorage(
        services,
        () => new AssessmentDatabase(databasePath));
    services.AddSingleton<IReportExporter, JsonReportExporter>();
    services.AddSingleton<IReportExporter>(_ => new TextReportExporter(ReportTextFormatter.Write));
});
