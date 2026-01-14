using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Snowflake.Data.Client;

namespace Stub.Services;

public class SnowflakeService(
	IConfiguration configuration,
	ILogger<SnowflakeService> logger)
{
	public IDataReader Read(string query, string warehouse = "USE WAREHOUSE COMPUTE_WH")
	{
		try
		{
			using IDbConnection snowflakeDbConnection = new SnowflakeDbConnection();
			snowflakeDbConnection.ConnectionString = $"account={configuration["Snowflake:Account"]};user={configuration["Snowflake:Username"]};password={configuration["Snowflake:Password"]};ROLE=ACCOUNTADMIN;db={configuration["Snowflake:DatabaseName"]};schema=PUBLIC";
			snowflakeDbConnection.Open();

			logger.LogInformation($"Snowflake Database {snowflakeDbConnection.Database} is successefully opened", snowflakeDbConnection.Database);

			using IDbCommand cmd = snowflakeDbConnection.CreateCommand();
			cmd.CommandText = warehouse;
			cmd.ExecuteNonQuery();
			cmd.CommandText = query;
			IDataReader reader = cmd.ExecuteReader();
			snowflakeDbConnection.Close();
			
			return reader;
		}
		catch(DbException ex)
		{
			logger.LogInformation($"Snowflake request exception: {ex.Message}");
			throw;
		}
	}
}