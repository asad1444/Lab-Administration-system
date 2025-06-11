using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Eadministration.Areas.Identity.Data;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Identity.UI.Services;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() // If you want to use roles
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IEmailSender, NoOpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Equipment}/{action=ShowEquipment}/{id?}");


using (var scope = app.Services.CreateScope())
{
    var sevices = scope.ServiceProvider;
    await CreateRolesAndAdminUser(sevices);
}
app.Run();

static async Task CreateRolesAndAdminUser(IServiceProvider serviceProvider)
{
    var rolemanager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var usermanager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roleNames = { "Admin", "HOD", "Instructor", "TechniqalStaff" };
    IdentityResult roleResult;
    foreach(var roleName in roleNames)
    {
        var roleExist = await rolemanager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            roleResult = await rolemanager.CreateAsync(new IdentityRole(roleName));
        }
    }
    var adminUser = new ApplicationUser
    {
        UserName = "admin@admin.com",
        Email = "admin@admin.com",
        EmailConfirmed = true
    };
    string adminPassword = "Admin@123";
    var createAdminUser = await usermanager.CreateAsync(adminUser,adminPassword);
    if (createAdminUser.Succeeded)
    {
        await usermanager.AddToRoleAsync(adminUser, "Admin");
    }


}
public class NoOpEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email,string subject ,string htmlMessage)
    {
        return Task.CompletedTask;
    }
}

