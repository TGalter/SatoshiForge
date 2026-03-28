using SatoshiForge.Api.Extensions;
using SatoshiForge.Api.Identity;
using SatoshiForge.Application.Abstractions.Identity;
using SatoshiForge.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructureLogging();

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddProblemDetailsConfiguration();
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddOpenApiConfiguration();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddAuthenticationConfiguration(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseSwaggerConfiguration();

app.MapHealthChecks("/health");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapOpenApiConfiguration();

app.Run();

public partial class Program;