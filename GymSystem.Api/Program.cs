using FluentValidation.AspNetCore;
using GymSystem.Application;
using GymSystem.Infrastructure;
using Scalar.AspNetCore;
using GymSystem.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddFluentValidationAutoValidation();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GymSystemDbContext>();
    await new DbSeeder(db).SeedAsync();
}
app.MapOpenApi();
app.MapScalarApiReference();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseMiddleware<GymSystem.Api.Middleware.ExceptionHandlingMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
