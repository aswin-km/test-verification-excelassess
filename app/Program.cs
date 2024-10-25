using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.Configuration;
using ExcelAssess.TestPlayer.ResponseVerification.Console.Services;
using ExcelAssess.TestPlayer.ResponseVerification.Console.Repository;
using ExcelAssess.TestPlayer.ResponseVerification.Console;

// Build configuration
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();


// Create the host
var host = Host.CreateDefaultBuilder(args)
    .UseSerilog() // Use Serilog for logging
    .ConfigureServices((context, services) =>
    {
        // Configurations
        services.AddSingleton<IConfiguration>(configuration);
        services.AddAwsSecretsManagerCache(configuration);
        services.AddAwsS3Storage(configuration);
        services.ConfigureDb(configuration);

        // Register repositories
        services.AddScoped<IScheduleUserFromS3Repository, ScheduleUserFromS3Repository>();
        services.AddScoped<IScheduleUserFromCacheRepository, ScheduleUserFromCacheRepository>();
        services.AddScoped<IScheduleUserRepository, ScheduleUserRepository>();
        services.AddScoped<IAssessmentRepository, AssessmentRepository>();
        // Register services
        services.AddScoped<ResponseVerificationService>();
        services.AddScoped<IResultCalculationService, ResultCalculationService>();
    })
    .Build();



// Run the application
var service = host.Services.GetRequiredService<ResponseVerificationService>();
await service.Start();