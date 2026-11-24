using POS.Application;
using Microsoft.Extensions.DependencyInjection;

namespace POS.CLI;

public static class AssessmentCli
{
    public static int Run(Action<IServiceCollection> configureServices)
    {
        var services = new ServiceCollection();
        configureServices(services);
        services.AddScoped<AssessmentSession>();
        services.AddScoped<ConsoleMode>();
        services.AddScoped<ConsoleLoop>();
        services.AddScoped<CommandDispatcher>();
        services.AddScoped<HelpCommand>();
        services.AddScoped<ExitCommand>();

        services.AddScoped<ConsoleCommand, AddStudentCommand>();
        services.AddScoped<ConsoleCommand, ListStudentsCommand>();
        services.AddScoped<ConsoleCommand, AddTeacherCommand>();
        services.AddScoped<ConsoleCommand, ListTeachersCommand>();
        services.AddScoped<ConsoleCommand, AddCriterionCommand>();
        services.AddScoped<ConsoleCommand, ListCriteriaCommand>();
        services.AddScoped<ConsoleCommand, SetScoreConsoleCommand>();
        services.AddScoped<ConsoleCommand, CorrectScoreConsoleCommand>();
        services.AddScoped<ConsoleCommand, CancelScoreConsoleCommand>();
        services.AddScoped<ConsoleCommand, ListScoresCommand>();
        services.AddScoped<ConsoleCommand, ShowReportCommand>();
        services.AddScoped<ConsoleCommand, ExportReportCommand>();
        services.AddScoped<ConsoleCommand, EnterTeacherCommand>();
        services.AddScoped<ConsoleCommand, EnterStudentCommand>();
        services.AddScoped<ConsoleCommand, BackCommand>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        using var sessionScope = provider.CreateScope();

        ConsoleLoop loop;
        try
        {
            loop = sessionScope.ServiceProvider.GetRequiredService<ConsoleLoop>();
        }
        catch (InvalidOperationException error)
        {
            Console.Error.WriteLine(error.Message);
            return 1;
        }

        loop.Run(Console.In, Console.Out);
        return 0;
    }
}
