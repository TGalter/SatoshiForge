using SatoshiForge.Api.Extensions;
using SatoshiForge.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

builder.Services.AddProblemDetailsConfiguration();
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddOpenApiConfiguration();

builder.Services.AddSwaggerConfiguration();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

app.UseStatusCodePages();
app.UseHttpsRedirection();

app.UseSwaggerConfiguration();

app.MapHealthChecks("/health");
app.MapControllers();
app.MapOpenApiConfiguration();

app.Run();