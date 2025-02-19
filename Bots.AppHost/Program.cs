var builder = DistributedApplication.CreateBuilder(args);

var botToken = builder.AddParameter("Token", builder.Configuration["Token"]!, secret: true);
var betaToken = builder.AddParameter("BetaToken", builder.Configuration["BetaToken"]!, secret: true);

var quotes = builder.AddMilvus("quotes")
    .WithDataVolume()
    .WithAttu()
    .WithLifetime(ContainerLifetime.Persistent);

var quotesDB = quotes.AddDatabase("quotes-db", "quotes_db");

#if DEBUG
builder.AddProject<Projects.TenBot>("tenbot")
    .WithReference(quotesDB)
    .WaitFor(quotesDB)
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
