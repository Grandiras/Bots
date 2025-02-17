var builder = DistributedApplication.CreateBuilder(args);

var botToken = builder.AddParameter("Token", builder.Configuration["Token"]!, secret: true);
var betaToken = builder.AddParameter("BetaToken", builder.Configuration["BetaToken"]!, secret: true);

#if DEBUG
builder.AddProject<Projects.TenBot>("tenbot")
    .WithEnvironment("Bot__DataRootPath", "Data")
    .WithEnvironment("Bot__IsBeta", "true")
    .WithEnvironment("Token", botToken)
    .WithEnvironment("BetaToken", betaToken);
#else
builder.AddContainer("tenbot", "grandiras/tenbot:latest")
    .WithEnvironment("Bot__DataRootPath", "Data")
    .WithEnvironment("Bot__IsBeta", "false")
    .WithEnvironment("Token", botToken)
    .WithEnvironment("BetaToken", betaToken)
    .WithVolume("tenbot-data");
#endif

builder.Build().Run();
