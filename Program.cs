
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SiteLocationMigration.Db;
using SiteLocationMigration.Models;
using SiteLocationMigration.Services;

Console.WriteLine("Migration app has started");

var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfigurationRoot configuration = builder.Build();

var services = new ServiceCollection();

var databaseSettingsConfiguration = configuration.GetSection("DatabaseSettings");
var appSettings = databaseSettingsConfiguration.Get<AppSettings>();

//
// Adding services
services.AddSingleton(appSettings);

services.AddDbContext<LegacyAtmbContext>(options =>
{
    options.UseSqlServer(appSettings.LegacyAtmbString);
});

services.AddDbContext<ModernAtmbContext>(options =>
{
    options.UseSqlServer(appSettings.ModernAtmbString);
});


services.AddTransient<MigrationService>();
////////////////////////////

using var serviceProvider = services.BuildServiceProvider();

var worker = serviceProvider.GetRequiredService<MigrationService>();


// Main program:
await worker.RunMigration();
///////////////////////////////

Console.WriteLine("Migration application finished.");