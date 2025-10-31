var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.FabricLibrary_Backend>("fabriclibrary-backend");

builder.Build().Run();
