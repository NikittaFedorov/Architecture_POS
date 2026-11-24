using POS.Application;
using Microsoft.Extensions.DependencyInjection;

namespace POS.Infrastructure;

public static class RelationalStorageServices
{
    public static void AddRelationalStorage(IServiceCollection services, Func<RelationalDatabase> openDatabase)
    {
        services.AddScoped<RelationalDatabase>(_ => openDatabase());
        services.AddScoped<IStudentRepository, SqlStudentRepository>();
        services.AddScoped<ITeacherRepository, SqlTeacherRepository>();
        services.AddScoped<IRubricRepository, SqlRubricRepository>();
        services.AddScoped<IEvaluationRepository, SqlEvaluationRepository>();
    }
}
