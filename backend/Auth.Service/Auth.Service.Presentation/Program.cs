using Auth.Service.Application.Interfaces;
using Auth.Service.Application.Mappings;
using Auth.Service.Application.Validators;
using Auth.Service.Infrastructure.Data;
using Auth.Service.Infrastructure.Repositories;
using Auth.Service.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Text;

// Bootstrap logger active before host builds (catches startup failures)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, config) => config
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    builder.Services.AddControllers();
    builder.Services.AddCors(options =>
        options.AddPolicy("AllowAngular", policy => policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()));

    // EF Core – SQL Server
    builder.Services.AddDbContext<AuthDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));

    // Onion DI: bind interfaces to implementations
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddSingleton<ITokenService, TokenService>();
    builder.Services.AddScoped<IAuthService, AuthService>();

    builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AuthMappingProfile>());

    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

    var jwtKey = builder.Configuration["Jwt:Key"]!;
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        });

    builder.Services.AddAuthorization();

    var app = builder.Build();

    // Logs all HTTP requests (method, path, status code, duration)
    app.UseSerilogRequestLogging();

    // Disabled in Production/Docker: container only listens on HTTP (port 8080)
    if (app.Environment.IsDevelopment())
        app.UseHttpsRedirection();

    app.UseCors("AllowAngular");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapGet("/", () => "Auth Service Running");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Auth Service terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
