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

// Apply migrations at startup - with fallback to direct table creation
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<ApplicationDbContext>();
    
    try
    {
        logger.LogInformation("Database Migration Process Starting...");
        
        // First, check if we can connect
        if (await context.Database.CanConnectAsync())
        {
            logger.LogInformation("Database connection successful!");
            
            // Check if tables exist
            var tablesExist = false;
            try
            {
                var count = await context.Database.ExecuteSqlRawAsync("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public'");
                tablesExist = count > 0;
            }
            catch
            {
                tablesExist = false;
            }
            
            if (!tablesExist)
            {
                logger.LogInformation("No tables found. Creating database schema...");
                
                // Try migrations first
                try
                {
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully!");
                }
                catch (Exception migEx)
                {
                    logger.LogWarning($"Migration failed: {migEx.Message}. Attempting direct creation...");
                    
                    // If migrations fail, create tables directly
                    try
                    {
                        var sql = @"
                            CREATE TABLE IF NOT EXISTS ""Books"" (
                                ""Id"" SERIAL PRIMARY KEY,
                                ""OpenLibraryId"" TEXT NOT NULL,
                                ""Title"" TEXT NOT NULL,
                                ""Author"" TEXT,
                                ""Description"" TEXT,
                                ""CoverUrl"" TEXT,
                                ""PublishYear"" INTEGER,
                                ""PageCount"" INTEGER,
                                ""ISBN"" TEXT,
                                ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL
                            );

                            CREATE TABLE IF NOT EXISTS ""ReadingLists"" (
                                ""Id"" SERIAL PRIMARY KEY,
                                ""Name"" TEXT NOT NULL,
                                ""Description"" TEXT,
                                ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL
                            );

                            CREATE TABLE IF NOT EXISTS ""SearchHistories"" (
                                ""Id"" SERIAL PRIMARY KEY,
                                ""SearchQuery"" TEXT NOT NULL,
                                ""SearchedAt"" TIMESTAMP WITH TIME ZONE NOT NULL,
                                ""ResultCount"" INTEGER NOT NULL
                            );

                            CREATE TABLE IF NOT EXISTS ""ReadingListBooks"" (
                                ""ReadingListId"" INTEGER NOT NULL,
                                ""BookId"" INTEGER NOT NULL,
                                ""AddedAt"" TIMESTAMP WITH TIME ZONE NOT NULL,
                                PRIMARY KEY (""ReadingListId"", ""BookId""),
                                FOREIGN KEY (""ReadingListId"") REFERENCES ""ReadingLists"" (""Id"") ON DELETE CASCADE,
                                FOREIGN KEY (""BookId"") REFERENCES ""Books"" (""Id"") ON DELETE CASCADE
                            );

                            CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                                ""MigrationId"" TEXT NOT NULL PRIMARY KEY,
                                ""ProductVersion"" TEXT NOT NULL
                            );
                        ";
                        
                        await context.Database.ExecuteSqlRawAsync(sql);
                        logger.LogInformation("Tables created successfully!");
                        
                        // Add a migration history entry
                        await context.Database.ExecuteSqlRawAsync(
                            @"INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"") 
                              VALUES ('20240504000001_InitialCreate', '8.0.0') 
                              ON CONFLICT DO NOTHING");
                    }
                    catch (Exception createEx)
                    {
                        logger.LogError($"Failed to create tables: {createEx.Message}");
                    }
                }
            }
            else
            {
                logger.LogInformation("Tables already exist.");
            }
        }
        else
        {
            logger.LogError("Cannot connect to database!");
        }
    }
    catch (Exception ex)
    {
        logger.LogError($"Database initialization failed: {ex.Message}");
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

// Manual migration endpoint
app.MapGet("/create-tables", async (ApplicationDbContext db) =>
{
    try
    {
        var sql = @"
            CREATE TABLE IF NOT EXISTS ""Books"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""OpenLibraryId"" TEXT NOT NULL,
                ""Title"" TEXT NOT NULL,
                ""Author"" TEXT,
                ""Description"" TEXT,
                ""CoverUrl"" TEXT,
                ""PublishYear"" INTEGER,
                ""PageCount"" INTEGER,
                ""ISBN"" TEXT,
                ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ""ReadingLists"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Name"" TEXT NOT NULL,
                ""Description"" TEXT,
                ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ""SearchHistories"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""SearchQuery"" TEXT NOT NULL,
                ""SearchedAt"" TIMESTAMP WITH TIME ZONE NOT NULL,
                ""ResultCount"" INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ""ReadingListBooks"" (
                ""ReadingListId"" INTEGER NOT NULL,
                ""BookId"" INTEGER NOT NULL,
                ""AddedAt"" TIMESTAMP WITH TIME ZONE NOT NULL,
                PRIMARY KEY (""ReadingListId"", ""BookId""),
                FOREIGN KEY (""ReadingListId"") REFERENCES ""ReadingLists"" (""Id"") ON DELETE CASCADE,
                FOREIGN KEY (""BookId"") REFERENCES ""Books"" (""Id"") ON DELETE CASCADE
            );
        ";
        
        await db.Database.ExecuteSqlRawAsync(sql);
        return Results.Ok("Tables created successfully!");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// Database status endpoint
app.MapGet("/db-status", async (ApplicationDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        var tables = await db.Database.SqlQuery<string>($"SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'").ToListAsync();
        
        return Results.Json(new
        {
            CanConnect = canConnect,
            Tables = tables,
            TablesCount = tables.Count
        });
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
