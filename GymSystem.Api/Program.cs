using FluentValidation;
using GymSystem.Api.Contracts;
<<<<<<< HEAD
using GymSystem.Api.Filters;
=======
>>>>>>> 1e24ce82b74a00cfc4f8e4cce973508ed2cbf585
using GymSystem.Api.Middleware;
using GymSystem.Api.OpenApi;
using GymSystem.Application;
using GymSystem.Application.Abstractions;
using GymSystem.Infrastructure;
using GymSystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
<<<<<<< HEAD
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())); 
=======
using GymSystem.Api.Filters;
var builder = WebApplication.CreateBuilder(args);

// ---- services ----
builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>()); 
>>>>>>> 1e24ce82b74a00cfc4f8e4cce973508ed2cbf585
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddValidatorsFromAssemblyContaining<BookClassBody>();

var jwt = builder.Configuration.GetSection("Jwt");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // keep claim names exactly as the generator wrote them

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "sub",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ---- dev only: sample data ----
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GymSystemDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHashing>();
    await new DbSeeder(db, hasher).SeedAsync();

    app.MapOpenApi();
    app.MapScalarApiReference();
}

// ---- the request pipeline, in order ----
app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();