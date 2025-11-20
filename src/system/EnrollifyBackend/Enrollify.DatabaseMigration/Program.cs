using Enrollify.DatabaseMigration;
using Enrollify.DatabaseMigration.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;
using System.Reflection;

var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Environment.ContentRootPath = Directory.GetCurrentDirectory();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true);

var registrar = new TypeRegistrar(builder.Services);

// Create a new command app with the registrar
// and run it with the provided arguments.
var app = new CommandApp<MigrationCommand>(registrar);
return app.Run(args);