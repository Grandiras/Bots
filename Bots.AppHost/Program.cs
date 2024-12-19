var builder = DistributedApplication.CreateBuilder(args);

var botToken = builder.AddParameter("Token", builder.Configuration["Token"]!, secret: true);
var betaToken = builder.AddParameter("BetaToken", builder.Configuration["BetaToken"]!, secret: true);

builder.AddProject<Projects.TenBot>("tenbot")
    .WithOtlpExporter()
    .WithEnvironment("Bot__DataRootPath", "Data")
    .WithEnvironment("Bot__IsBeta", "true")
    .WithEnvironment("Token", botToken)
    .WithEnvironment("BetaToken", betaToken);

builder.Build().Run();
