var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Enrollify_WebAPI>("enrollify-webapi");

builder.Build().Run();
