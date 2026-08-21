using ConcertTicket.Infrastructure;
using ConcertTicket.Infrastructure.Persistence;
using ConcertTicket.Infrastructure.Persistence.Seed;
using ConcertTicket.Application;
using System.Text;
using ConcertTicket.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ConcertTicket.Application.Common.Interfaces;
using DotNetEnv;
using ConcertTicket.Api.Middlewares;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ConcertTicket API", Version = "v1" });

    // Add JWT Authentication to Swagger
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token **_only_**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

builder.Services.Configure<JwtOptions>(options =>
{
    builder.Configuration.GetSection("Jwt").Bind(options);
    
    var envSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
    if (!string.IsNullOrEmpty(envSecret)) options.Secret = envSecret;
    
    var envIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
    if (!string.IsNullOrEmpty(envIssuer)) options.Issuer = envIssuer;
    
    var envAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
    if (!string.IsNullOrEmpty(envAudience)) options.Audience = envAudience;
});

var jwtOptions = new JwtOptions();
builder.Configuration.GetSection("Jwt").Bind(jwtOptions);

var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
if (!string.IsNullOrEmpty(secret)) jwtOptions.Secret = secret;

var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
if (!string.IsNullOrEmpty(issuer)) jwtOptions.Issuer = issuer;

var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
if (!string.IsNullOrEmpty(audience)) jwtOptions.Audience = audience;

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    jwtOptions.Issuer,

                ValidAudience =
                    jwtOptions.Audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtOptions.Secret))
            };
    });

builder.Services.AddAuthorization();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

    var passwordHasher =
        scope.ServiceProvider
            .GetRequiredService<IPasswordHasher>();

    await DatabaseSeeder.SeedAsync(
        dbContext,
        passwordHasher);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
