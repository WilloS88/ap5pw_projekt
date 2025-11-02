using AP5PW_Helpdesk.Data;
using AP5PW_Helpdesk.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Debug()
	.WriteTo.Console()
	.WriteTo.File(
		path: "logs/app.log",
		rollingInterval: RollingInterval.Day,
		retainedFileCountLimit: 7,
		shared: true)
	.CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllersWithViews();

var cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var serverVersion = ServerVersion.AutoDetect(cs);
    opt.UseMySql(cs, serverVersion);
});

builder.Services.AddScoped<
    AP5PW_Helpdesk.Data.Repositories.IUserRepository,
    AP5PW_Helpdesk.Data.Repositories.UserRepository>();
builder.Services.AddScoped<
	AP5PW_Helpdesk.Data.Repositories.IGoodRepository,
	AP5PW_Helpdesk.Data.Repositories.GoodRepository>();
builder.Services.AddScoped<
    AP5PW_Helpdesk.Data.Repositories.ICompanyRepository,
    AP5PW_Helpdesk.Data.Repositories.CompanyRepository>();
builder.Services.AddScoped<
    AP5PW_Helpdesk.Data.Repositories.IWarehouseRepository,
    AP5PW_Helpdesk.Data.Repositories.WarehouseRepository>();
builder.Services.AddScoped<
	AP5PW_Helpdesk.Data.Repositories.IOrderRepository,
	AP5PW_Helpdesk.Data.Repositories.OrderRepository>();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Cookie auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.LoginPath = "/Auth/Login";
		options.AccessDeniedPath = "/Auth/AccessDenied";
	});

// Role-based policy
builder.Services.AddAuthorization(options =>
{
	// Všechno vyžaduje pøihlášení, pokud neuvedeš [AllowAnonymous]
	options.FallbackPolicy = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.Build();

	// Jen admin
	options.AddPolicy("RequireAdmin", p =>
		p.RequireRole("admin"));

	// Admin nebo moderator
	options.AddPolicy("RequireAdminOrModerator", p =>
		p.RequireRole("admin", "moderator"));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization(); 

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

try
{
	Log.Information("Application starting...");
	app.Run();
}
catch (Exception ex)
{
	Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
	Log.CloseAndFlush();
}

app.Run();