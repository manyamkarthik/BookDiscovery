using BookDiscovery.Data;
using BookDiscovery.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configure Entity Framework based on environment
if (builder.Environment.IsProduction())
{
    // Use PostgreSQL in production (Railway)
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrEmpty(connectionString))
    {
        // Convert Railway's DATABASE_URL to .NET connection string format
        var databaseUri = new Uri(connectionString);
        var userInfo = databaseUri.UserInfo.Split(':');
        var connectionStringBuilder = $"Host={databaseUri.Host};Port={databaseUri.Port};Database={databaseUri.LocalPath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true;";

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionStringBuilder));
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

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Apply migrations automatically in production
if (app.Environment.IsProduction())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
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

app.Run();