using BookDiscovery.Data;
using BookDiscovery.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddRazorPages();

// Configure Entity Framework based on environment
if (builder.Environment.IsProduction())
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    Console.WriteLine($"DATABASE_URL present: {!string.IsNullOrEmpty(databaseUrl)}");
    
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        var databaseUri = new Uri(databaseUrl);
        var userInfo = databaseUri.UserInfo.Split(':');
        var connectionString = $"Host={databaseUri.Host};Port={databaseUri.Port};Database={databaseUri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;";
        
        Console.WriteLine($"Connecting to database: {databaseUri.Host}/{databaseUri.AbsolutePath.TrimStart('/')}");
        
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
    }
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Add HttpClient for OpenLibrary API
builder.Services.AddHttpClient<OpenLibraryService>();
builder.Services.AddHttpClient();

var app = builder.Build();

// Apply migrations at startup
if (app.Environment.IsProduction())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        try
        {
            logger.LogInformation("Checking database connection...");
            var context = services.GetRequiredService<ApplicationDbContext>();
            
            // First, try migrations
            try
            {
                logger.LogInformation("Applying migrations...");
                context.Database.Migrate();
                logger.LogInformation("Migrations applied successfully!");
            }
            catch (Exception migrationEx)
            {
                logger.LogWarning($"Migration failed: {migrationEx.Message}");
                
                // If migrations fail, try to create the database directly
                try
                {
                    logger.LogInformation("Attempting to create database schema directly...");
                    context.Database.EnsureCreated();
                    logger.LogInformation("Database schema created successfully!");
                }
                catch (Exception createEx)
                {
                    logger.LogError($"Failed to create database: {createEx.Message}");
                    // Continue anyway - the app might still work for some operations
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Database initialization failed: {ex.Message}");
            // Continue anyway
        }
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

// Test endpoint
app.MapGet("/test", () => "Application is running!");

// Database status endpoint
app.MapGet("/db-status", async (ApplicationDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        return Results.Json(new { 
            CanConnect = canConnect,
            Provider = db.Database.ProviderName
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new { Error = ex.Message });
    }
});

// Configure port for Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "3000";
Console.WriteLine($"Starting application on port: {port}");
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
