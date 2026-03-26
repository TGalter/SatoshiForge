using SatoshiForge.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

builder.Services.AddProblemDetailsConfiguration();
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddOpenApiConfiguration();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

app.UseStatusCodePages();
app.UseHttpsRedirection();

app.MapHealthChecks("/health");
app.MapControllers();
app.MapOpenApiConfiguration();

app.Run();