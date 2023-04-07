using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog.WebApplication.Models;
using Serilog.Formatting.Json;
using Serilog.Sinks.MSSqlServer;

namespace Serilog.WebApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // https://codewithmukesh.com/blog/serilog-in-aspnet-core-3-1/#:~:text=Serilog%20Sinks,-Serilog%20Sinks%20in&text=In%20the%20packages%20that%20we,%2CSQLite%2C%20SEQ%20and%20more.
            // Read configuration from Appsettings
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            // Initialize Logger
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(config).CreateLogger();

            try
            {
                string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}";
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", Events.LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.AspNetCore", Events.LogEventLevel.Warning)
                    .WriteTo.File("C:\\Logs\\Demo.txt", outputTemplate: outputTemplate, rollingInterval: RollingInterval.Day)
                    .WriteTo.Console(new JsonFormatter()) // https://code-maze.com/structured-logging-in-asp-net-core-with-serilog/
                    .WriteTo.Seq("http://localhost:5341")
                    .WriteTo.MSSqlServer("Server=LAPTOP-88RH8M43;Database=LoggingDb;User ID=sa;Password=p@ssw0rd", new MSSqlServerSinkOptions { 
                        AutoCreateSqlTable = true,
                        SchemaName = "dbo",
                        TableName = "Logs"
                    })
                    .CreateLogger();
                
                Log.Information("Application Starting");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "The application failed to start.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
            //// what is the use of ILoggerFactory
            //// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.1#clms
            ////CreateHostBuilder(args).Build().Run();
            //var host = CreateHostBuilder(args).Build();
            //var logger = host.Services.GetRequiredService<ILogger<Program>>();
            //logger.LogInformation(MyLogEvents.GetItem,"Host created {Id}", 1);

            //host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).UseSerilog()
                //.ConfigureLogging(logging =>
                //{
                //    //logging.AddFilter("System", LogLevel.Debug)
                //    //    .AddFilter<DebugLoggerProvider>("Microsoft", LogLevel.Information)
                //    //    .AddFilter<ConsoleLoggerProvider>("Microsoft", LogLevel.Trace);
                //    //logging.SetMinimumLevel(LogLevel.Error);
                //    logging.ClearProviders();
                //    logging.AddConsole();
                    
                //})
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
