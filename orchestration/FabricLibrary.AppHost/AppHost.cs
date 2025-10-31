var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL database with pgAdmin for database management
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .AddDatabase("fabriclibrary");

// Add the backend API with reference to the database
builder.AddProject<Projects.FabricLibrary_Backend>("fabriclibrary-backend")
    .WaitFor(postgres)
    .WithReference(postgres);

builder.Build().Run();
