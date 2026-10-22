using FrameRent.Api.Middleware;
using FrameRent.Infrastructure;
using FrameRent.Infrastructure.Persistence;
using FrameRent.Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Register Infrastructure (Persistence, Identity, JWT, Repositories, Services)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Controllers and Route configuration
builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Configure CORS for client applications
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClientApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure Swagger / OpenAPI with JWT Bearer authorization
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FrameRent API",
        Version = "v1",
        Description = "Equipment Rental Management System REST API"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token. Example: 'Bearer {token}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            securityScheme,
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Global Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FrameRent API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowClientApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Execute database migration and data seeding if connection is available
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = services.GetRequiredService<FrameRentDbContext>();
        if (await db.Database.CanConnectAsync())
        {
            logger.LogInformation("Database connection established. Applying migrations...");
            await db.Database.MigrateAsync();

            logger.LogInformation("Seeding default roles and staff user...");
            await DataSeeder.SeedAsync(services);
            logger.LogInformation("Database migrations and seeding completed successfully.");
        }
        else
        {
            logger.LogWarning("Database is unreachable at startup. Skipping automatic migrations and seeding.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database migration or seeding at startup.");
    }
}

app.Run();
