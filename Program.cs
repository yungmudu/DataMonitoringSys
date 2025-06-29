using DataMonitoringSys.Components;
using DataMonitoringSys.Data;
using DataMonitoringSys.Models;
using DataMonitoringSys.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ??
                     "Data Source=DataMonitoring.db"));

// Add Identity services
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // SignIn settings
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultUI();

// Add application services
builder.Services.AddScoped<IDataPointService, DataPointService>();
builder.Services.AddScoped<IEngineeringUnitService, EngineeringUnitService>();
builder.Services.AddScoped<DataSeedingService>();

// Add controllers
builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure SignalR for Blazor Server
builder.Services.AddSignalR(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.HandshakeTimeout = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

// Add authorization
builder.Services.AddAuthorization();

// Add authentication state provider
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider,
    Microsoft.AspNetCore.Components.Server.ServerAuthenticationStateProvider>();

var app = builder.Build();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    await context.Database.EnsureDeletedAsync();
    await context.Database.EnsureCreatedAsync();

    // Create default admin user if no users exist
    Console.WriteLine($"Checking for existing users... Count: {context.Users.Count()}");
    if (!context.Users.Any())
    {
        Console.WriteLine("No users found, creating admin user...");
        var adminUser = new ApplicationUser
        {
            UserName = "admin@engineering.com",
            Email = "admin@engineering.com",
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "User",
            Department = "Engineering",
            JobTitle = "System Administrator",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            EngineeringUnitId = 1 // Process Engineering Unit A
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            Console.WriteLine("Default admin user created:");
            Console.WriteLine("Email: admin@engineering.com");
            Console.WriteLine("Password: Admin123!");
        }
        else
        {
            Console.WriteLine("Failed to create admin user:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"- {error.Description}");
            }
        }
    }
    else
    {
        Console.WriteLine("Users already exist in database.");
    }

    // Seed mock data for charts and analytics
    try
    {
        var dataSeedingService = scope.ServiceProvider.GetRequiredService<DataSeedingService>();
        await dataSeedingService.SeedMockDataAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding mock data: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map controllers
app.MapControllers();

// Map Identity UI
app.MapRazorPages();

app.Run();
