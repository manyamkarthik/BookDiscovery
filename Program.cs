using BookDiscovery.Data;
using BookDiscovery.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configure Entity Framework based on environment
if (builder.Environment.IsProduction())
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    Console.WriteLine($"DATABASE_URL present: {!string.IsNullOrEmpty(databaseUrl)}");
    
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        try
        {
            // Parse the DATABASE_URL
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            
            // Build connection string for Npgsql
            var connectionString = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Database = databaseUri.AbsolutePath.TrimStart('/'),
                Username = userInfo[0],
                Password = userInfo[1],
                SslMode = SslMode.Require
                // Remove TrustServerCertificate as it's obsolete
            }.ToString();
            
            Console.WriteLine($"Connecting to database: {databaseUri.Host}/{databaseUri.AbsolutePath.TrimStart('/')}");
            
            // Test the connection
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                Console.WriteLine("Successfully connected to database!");
                conn.Close();
            }
            
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database connection error: {ex.Message}");
            throw;
        }
    }
}
else
{
    // Use SQL Server in development
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Add HttpClient for OpenLibrary API
builder.Services.AddHttpClient<OpenLibraryService>();
builder.Services.AddHttpClient();

builder.Logging.AddConsole();

var app = builder.Build();

// Apply migrations automatically in production
if (app.Environment.IsProduction())
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();
            
            logger.LogInformation("Starting database migration...");
            
            var context = services.GetRequiredService<ApplicationDbContext>();
            
            // Log pending migrations
            var pendingMigrations = context.Database.GetPendingMigrations();
            logger.LogInformation($"Pending migrations: {pendingMigrations.Count()}");
            
            foreach (var migration in pendingMigrations)
            {
                logger.LogInformation($"Pending migration: {migration}");
            }
            
            // Apply migrations
            context.Database.Migrate();
            logger.LogInformation("Database migration completed successfully!");
        }
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

// Fix the test endpoint - proper way to inject services
app.MapGet("/test-db", async (ApplicationDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
        
        return Results.Json(new
        {
            CanConnect = canConnect,
            PendingMigrations = pendingMigrations.ToList(),
            DatabaseProvider = db.Database.ProviderName
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new { Error = ex.Message });
    }
});

// Manual migration endpoint
app.MapGet("/migrate", async (ApplicationDbContext db) =>
{
    try
    {
        await db.Database.MigrateAsync();
        return Results.Ok("Migrations applied successfully!");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Migration error: {ex.Message}");
    }
});

// Environment check endpoint
app.MapGet("/env", () =>
{
    return Results.Json(new
    {
        DatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL") != null ? "Present" : "Missing",
        Port = Environment.GetEnvironmentVariable("PORT"),
        Environment = app.Environment.EnvironmentName,
        AspNetCoreEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    });
});

// Configure port for Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "3000";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
