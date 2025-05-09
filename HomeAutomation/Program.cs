﻿using DataBase;

using HomeAutomation;
using HomeAutomation.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace ScrapingService
{
    public class Program
    {
        public static async Task Main()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", false, true);
            var configuration = builder.Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging(x => {
                    x.AddConfiguration(configuration.GetSection("Logging"));
                    x.AddConsole();
                })
                .AddScoped<IConfiguration>(_ => configuration)
                .AddScoped<IAutomationTask, HueLightAutoOff>()
                .AddScoped<IAutomationTask, HealthCheck>()
                .AddDbContext<HomeServerDbContext>(optionsBuilder => {
                    optionsBuilder.UseNpgsql(configuration.GetConnectionString("Database"));
                })
                .AddScoped<Executor>()
                .BuildServiceProvider();

            var isCanceled = false;

            var cron = serviceProvider.GetService<Executor>();

            if (cron == null)
            {
                throw new ApplicationException();
            }

            await cron.StartAsync();
            Console.CancelKeyPress += (_, _) => {
                cron.Dispose();
                isCanceled = true;
            };

            // 永久ループ
            while (!isCanceled)
            {
                Console.ReadLine();
            }
        }
    }
}
