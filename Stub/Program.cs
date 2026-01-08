// See https://aka.ms/new-console-template for more information

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.System.Text.Json;
using Stub;
using Stub.Models;

/*
 * docker pull redis:latest
 * docker run -d --name local-redis -p 6379:6379 redis:latest
   
 */
var host = Host.CreateDefaultBuilder(args)
	.ConfigureAppConfiguration((context, config) =>
	{
		// I can read both, environment variables and user secrets without enabling these two:
		// config.AddEnvironmentVariables();
		// config.AddUserSecrets<Program>();
		
		var environment = context.HostingEnvironment;
		Console.WriteLine($"Environment: {environment.EnvironmentName}");
		if (environment.IsDevelopment())
		{
			Console.WriteLine("For when you run from the IDE - Development environment");
			var projectPath = Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName).FullName;
			config.SetBasePath(projectPath);
		}
		else
		{
			Console.WriteLine("For when you run from the CLI - Not development environment");
			config.SetBasePath(Directory.GetCurrentDirectory());
		}
		
		//  Pick up configuration settings.
		config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
	})
	.ConfigureLogging(logging =>
	{
		logging.AddConsole();
	})
	.ConfigureServices((hostContext, services) =>
	{
		services.AddLogging();
		
		services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(new RedisConfiguration
		{
			ConnectionString = hostContext.Configuration.GetConnectionString("Redis") ?? throw new NullReferenceException("Redis conn not set"),
		});

		//  We don't need to register Redis.
	})
	.Build();

var dbName = "sqlite.db";

await using var context = new SqLiteDbContext();
await context.Database.EnsureCreatedAsync();

if (!context.Patients.Any())
{
	await context.Patients.AddRangeAsync(new List<Patient>
	{
		new Patient { Id = "4", Given = "Poe", Family = "Dameron", BirthDate = new DateOnly(1960, 02, 14) },
		new Patient { Id = "5", Given = "Boba", Family = "Fett", BirthDate = new DateOnly(1974, 02, 14) },
		new Patient { Id = "6", Given = "Kylo", Family = "Ren", BirthDate = new DateOnly(2020, 09, 22) },
	});

	await context.SaveChangesAsync();
}

var kylo = await context.Patients
	.FirstOrDefaultAsync(p => p.Id == "6");

if (kylo == null)
{
	Console.WriteLine("Patient not found");
}
else
{
	Console.WriteLine($"Patient found: {kylo.Given} {kylo.Family} {kylo.BirthDate}");	
}

// Console.WriteLine("Hello SqLite!");
// return;

var redisDatabase = host.Services.GetRequiredService<IRedisDatabase>();

var redisSeedPatients = new List<Patient>
{
	new Patient { Id = "1", Given = "Luke", Family = "Skywalker", BirthDate = new DateOnly(1974, 02, 14) },
	new Patient { Id = "2", Given = "Leia", Family = "Organa", BirthDate = new DateOnly(1974, 02, 14) },
	new Patient { Id = "3", Given = "Han", Family = "Solo", BirthDate = new DateOnly(1971, 09, 22) },
};

var cached = await redisDatabase.GetAsync<List<Patient>>("patients");


if (cached != null)
{
	Console.WriteLine("\nCached Patients found\n");
	foreach (var patient in cached)
	{
		Console.WriteLine($"Given: {patient.Given}, Family: {patient.Family}, BirthDate: {patient.BirthDate}");
	}
}
else
{
	Console.WriteLine("\nCreating cached Patients\n");
	await redisDatabase.AddAsync("patients", redisSeedPatients);
}
Console.WriteLine("Hello, alternative persistence!");
Console.ReadLine();