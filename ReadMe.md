# Alternative Persistence

## Redis

Redis requires an hosted external service. In this case, we can mitigate the hosting of the Redis service to Docker.

Requires Docker:
```bash
# get the latest image
docker pull redis:latest

# run in Docker
docker run -d --name local-redis -p 6379:6379 redis:latest

# Conn: 
localhost:6379
```

## SqLite

SqLite seems to just run within the application. We just need to add the package EF Core Sqlite and provide minimal configuration in the Fluent API inheriting from DbContext.

```c#
<ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.11" />
</ItemGroup>
```

## Data Access

Data access can be mitigated to the following:
- Entity Framework
- SqLite-NET
- Dapper
- ADO.NET