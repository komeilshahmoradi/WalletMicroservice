using KO.BuildingBlocks.Infrastructure.DependencyInjection;
using System.Text.Json.Serialization;
using Wallet.Api.ExceptionHandler;
using Wallet.Application.DependencyInjection;
using Wallet.Infrastructure;
using Wallet.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var configuration = builder.Configuration;

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddApplication().AddInfrastructure(configuration);
builder.Services.AddControllers().AddJsonOptions(option =>
{
  option.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();

builder.Host.UseBuildingBlocksSerilog();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.UseSwaggerUI(options =>
  {
    options.SwaggerEndpoint("/openapi/v1.json", AppDomain.CurrentDomain.FriendlyName);
  });
}

app.UseExceptionHandler();
app.MigrateDatabase<ApplicationDbContext>();
app.UseHttpsRedirection();
app.MapControllers();

await app.RunAsync();