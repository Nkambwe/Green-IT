using GreenIT.Infrastructure;
using GreenIT.Infrastructure.Data;
using GreenIT.Infrastructure.Data.Seeding;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Infrastructure and database
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure exception handling.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Development database seeding and diagnostics.
if (app.Environment.IsDevelopment()) {
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider
        .GetRequiredService<GreenITDbContext>();

    var connection = db.Database.GetDbConnection();

    Console.WriteLine("========== GREENIT DATABASE CHECK ==========");
    Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
    Console.WriteLine($"SQL Server: {connection.DataSource}");
    Console.WriteLine($"Database: {connection.Database}");
    Console.WriteLine("============================================");

    await GreenITSeeder.SeedAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
