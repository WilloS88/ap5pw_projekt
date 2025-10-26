using AP5PW_Helpdesk.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var serverVersion = ServerVersion.AutoDetect(cs);
    opt.UseMySql(cs, serverVersion);
});

builder.Services.AddControllersWithViews();

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


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();