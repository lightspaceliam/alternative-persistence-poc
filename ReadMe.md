# Alternative Persistence

Overly simplified proof of concept / personal development on data tech stacks I've not used before:

- Snowflake Catalog SQL 
- Redis
- SQLite

## Snowflake

I was unable to work out how to develop against a local Snowflake instance or emulator, so I had to create an account. It's free for 30 days. Apparently you can use:

- Go/DuckDB
- LocalStack

To emulate local development.

**User Secrets:**

You will need to add user secrets. The code is expecting this schema:

```json
{
  "Snowflake": {
    "Account": "{your}-{account}",
    "Username": "{user}",
    "Password": "{password}",
    "DatabaseName": "{database-name}"
  }
}
```

**Package/s:**

```c#
<ItemGroup>
    <PackageReference Include="Snowflake.Data" Version="5.3.0" />
</ItemGroup>
```

- https://www.nuget.org/packages/Snowflake.Data/

**Official Reference:**

*How to connect to snowflake using C Sharp application with snowflake .NET Connector to perform SQL operations in windows*

- https://community.snowflake.com/s/article/How-to-connect-to-snowflake-using-C-Sharp-application-with-snowflake-NET-Connector-to-perform-SQL-operations-in-windows

## Redis

Redis requires an hosted external service. In this case, we can mitigate the hosting of the Redis service to Docker.

**Package/s:**

```c#
<ItemGroup>
    <PackageReference Include="StackExchange.Redis.Extensions.AspNetCore" Version="9.1.0" />
    <PackageReference Include="StackExchange.Redis.Extensions.System.Text.Json" Version="9.1.0" /> //  You can use whatever JSON serializer you prefer 
</ItemGroup>
```

- https://www.nuget.org/packages/StackExchange.Redis.Extensions.AspNetCore
- https://www.nuget.org/packages/StackExchange.Redis.Extensions.System.Text.Json

**Docker:**

Requires Docker for Local development. Commands for local installation:

```bash
# get the latest image
docker pull redis:latest

# run in Docker
docker run -d --name local-redis -p 6379:6379 redis:latest

# Conn: 
localhost:6379
```

## SQLite

SQLite seems to just run within the application. We just need to add the package EF Core Sqlite and provide minimal configuration in the Fluent API inheriting from DbContext. Will explore further when time permits...

**Package/s:**

```c#
<ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.11" />
</ItemGroup>
```

- https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite