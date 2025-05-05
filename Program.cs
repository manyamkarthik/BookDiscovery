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

// Apply migrations automatically in production
if (app.Environment.IsProduction())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            Console.WriteLine("Applying database migrations...");
            context.Database.Migrate();
            Console.WriteLine("Database migrations applied successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
            // Let the app continue even if migration fails
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

// Test endpoints
app.MapGet("/test", () => "Application is running!");
app.MapGet("/test-db", async (ApplicationDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        return Results.Ok($"Database connection: {canConnect}");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// Configure port for Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "3000";
Console.WriteLine($"Starting application on port: {port}");
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
