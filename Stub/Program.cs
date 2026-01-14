// See https://aka.ms/new-console-template for more information

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Snowflake.Data.Client;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.System.Text.Json;
using Stub;
using Stub.Models;
using System.Data;
using System.Data.Common;
using Stub.Services;

var host = Host.CreateDefaultBuilder(args)
	.ConfigureAppConfiguration((context, config) =>
	{		
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
		
		services.AddTransient<SnowflakeService>();
		
	})
	.Build();

var configuration = host.Services.GetRequiredService<IConfiguration>();

#region Snowflake

var snowflakeService = host.Services.GetRequiredService<SnowflakeService>();

var snowflakePatients = new List<SnowflakePatient>();
var dataReader = snowflakeService.Read("SELECT ID, GIVEN, FAMILY FROM PATIENTS LIMIT 10");

//  Contextually aware shaping of the data reader.
while (dataReader.Read())
{
	snowflakePatients.Add(new SnowflakePatient
	{
		Id = dataReader.GetString(dataReader.GetOrdinal("ID")),
		Given = dataReader.GetString(dataReader.GetOrdinal("GIVEN")),
		Family = dataReader.GetString(dataReader.GetOrdinal("FAMILY")),
	});
}

if (!snowflakePatients.Any())
{
	Console.WriteLine("Patients not found");
	return;
}
else {  
	snowflakePatients.ForEach(p => {

		Console.WriteLine($"Id: {p.Id}, Given: {p.Given}, Family: {p.Family}");
	});
}

return;

#endregion Snowflake

#region SQLite

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

// return;

#endregion SQLite

#region Redis

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
	Console.WriteLine("\nCached Patients found in Redis\n");
	foreach (var patient in cached)
	{
		Console.WriteLine($"Given: {patient.Given}, Family: {patient.Family}, BirthDate: {patient.BirthDate}");
	}
}
else
{
	Console.WriteLine("\nCreating cached Patients in Redis\n");
	await redisDatabase.AddAsync("patients", redisSeedPatients);
}

#endregion Redis

Console.WriteLine("Hello, alternative persistence!");
Console.ReadLine();