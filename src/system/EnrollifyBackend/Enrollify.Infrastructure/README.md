# Migration

Install the EF Core tools globally (one-time):

`dotnet tool install --global dotnet-ef`

Or update if already installed:

`dotnet tool update --global dotnet-ef`

## How to Generate Migrations:

### Using .NET CLI:

Navigate to your project directory:

`cd D:\Enrollment-System\src\system\EnrollifyBackend\Enrollify.Infrastructure`

Create an initial migration:

`dotnet ef migrations add InitialCreate --startup-project ..\Enrollify.Web\Enrollify.Web.csproj --context AppDbContext`

Apply migration to database:

`dotnet ef database update --startup-project ..\Enrollify.Web\Enrollify.Web.csproj --context AppDbContext`


### Using Package Manager Console in Visual Studio:

Open Tools > NuGet Package Manager > Package Manager Console
Set Default project to Enrollify.Infrastructure:

```
Add-Migration InitialCreate
Update-Database
```




## Common Commands Reference:

```console
# Add migration
dotnet ef migrations add <MigrationName> --startup-project <StartupProject>

# Apply migrations
dotnet ef database update --startup-project <StartupProject>

# Remove last migration (if not applied)
dotnet ef migrations remove --startup-project <StartupProject>

# List all migrations
dotnet ef migrations list --startup-project <StartupProject>

# Generate SQL script from migrations
dotnet ef migrations script --startup-project <StartupProject>
```