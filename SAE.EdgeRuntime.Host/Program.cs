using NATS.Net;
using NATS.Client.Core;
using NATS.Client.Hosting;
using SAE.EdgeRuntime.Core;
using SAE.EdgeRuntime.Persistence;
using SAE.EdgeRuntime.Sync;
using SAE.EdgeRuntime.Core.Kernel;
using SAE.EdgeRuntime.Modules.Orders.Commands;
using SAE.EdgeRuntime.Core.MultiTenancy;
using SAE.EdgeRuntime.Core.Projections;
using SAE.EdgeRuntime.Modules.Orders.Projections;
using SAE.EdgeRuntime.Modules.Catalog.Projections;
using SAE.EdgeRuntime.Modules.Caja.Projections;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Core Kernel Configuration
builder.Services.AddEdgeKernel();

// 2. Persistence Layer (Postgres)
var connectionString = builder.Configuration.GetConnectionString("EdgeDb") 
    ?? "Host=localhost;Database=sae_edge;Username=postgres;Password=postgres";
builder.Services.AddEdgePersistence(connectionString);

// 3. Fabric Layer (NATS)
var natsUrl = builder.Configuration["Nats:Url"] ?? "nats://localhost:4222";
builder.Services.AddNats(1, options => options with { Url = natsUrl });
builder.Services.AddSingleton<NatsReplicationEngine>();

// 4. Projections
builder.Services.AddSingleton<IProjection>(sp => new OrderSummaryProjection(connectionString));
builder.Services.AddSingleton<IProjection>(sp => new ProductCatalogProjection(connectionString));
builder.Services.AddSingleton<IProjection>(sp => new CajaBalanceProjection(connectionString));

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/scalar", options => options
        .WithTheme(ScalarTheme.DeepSpace)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));
}

app.UseHttpsRedirection();

// API Endpoints for testing the Kernel
app.MapPost("/commands/orders", async (StartOrderCommand cmd, CommandDispatcher dispatcher) =>
{
    // Simulate tenant from header or auth token
    var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    var tenantContext = new TenantContext(tenantId);
    
    await dispatcher.Dispatch(cmd, tenantContext);
    
    return Results.Accepted();
});

app.MapGet("/health", () => Results.Ok(new { Status = "SAE Edge Runtime Online" }));

app.Run();
