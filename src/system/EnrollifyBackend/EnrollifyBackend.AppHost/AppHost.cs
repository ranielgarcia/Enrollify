var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter("sql-password", value: "YourStrongPassword123!", secret: true);

var sql = builder.AddSqlServer("sql", port: 59480, password: sqlPassword)
  .WithDataVolume();

var db = sql.AddDatabase("appdb", databaseName: "enrollify-db");

var azurite = builder.AddContainer(
    "azurite",
    "mcr.microsoft.com/azure-storage/azurite")
  .WithEndpoint(10000, 10000, "blob")
  .WithEndpoint(10001, 10001, "queue")
  .WithEndpoint(10002, 10002, "table");

var storageConnectionString =
  "DefaultEndpointsProtocol=http;" +
  "AccountName=devstoreaccount1;" +
  "AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;" +
  "BlobEndpoint=http://localhost:10000/devstoreaccount1;" +
  "QueueEndpoint=http://localhost:10001/devstoreaccount1;" +
  "TableEndpoint=http://localhost:10002/devstoreaccount1;";

var webapi = builder.AddProject<Projects.Enrollify_WebAPI>("enrollify-webapi", "https")
  .WithReference(db).WaitForStart(db)
  .WaitForStart(azurite)
  .WithEnvironment(
    "ConnectionStrings__DefaultConnection",
    db.Resource.ConnectionStringExpression)
  .WithEnvironment(
    "Storage__Default__ConnectionString", storageConnectionString);

builder.AddNpmApp(
    "frontend",
    @"D:\Enrollment-System\src\system\enrollify-frontend",
    "dev")
  .WithReference(webapi)
  .WithEnvironment("VITE_API_URL", webapi.GetEndpoint("https"))
  .WithEnvironment("VITE_PORT ", "5173")
  .WithHttpEndpoint(
    targetPort: 5173,
    name: "http");

builder.Build().Run();


