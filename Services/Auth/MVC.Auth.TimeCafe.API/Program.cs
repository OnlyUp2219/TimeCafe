using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using MVC.Auth.TimeCafe.API.Areas.Identity.Pages.Account.Manage;
using MVC.Auth.TimeCafe.API.Controllers;
using MVC.Auth.TimeCafe.API.Data;
using MVC.Auth.TimeCafe.API.Models;
using MVC.Auth.TimeCafe.API.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var google = builder.Configuration.GetSection("Authentication:Google");
var microsoft = builder.Configuration.GetSection("Authentication:Microsoft");
builder.Services.AddAuthentication().
    AddGoogle(op =>
    {
        op.ClientId = google["ClientId"] ?? "";
        op.ClientSecret = google["ClientSecret"] ?? "";
        op.CallbackPath = "/signin-google";
    })
    .AddMicrosoftAccount(op =>
    {
        op.ClientId = microsoft["ClientId"] ?? "";
        op.ClientSecret = microsoft["ClientSecret"] ?? "";
        op.CallbackPath = "/signin-microsoft";
    });

// Postmark email sender registration
builder.Services.Configure<PostmarkOptions>(builder.Configuration.GetSection("Postmark"));
builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("Twilio"));
builder.Services.AddHttpClient();
builder.Services.AddTransient<IEmailSender, PostmarkEmailSender>();



builder.Services.AddTransient<PhoneVerificationModel>();
builder.Services.AddTransient<VerifyCodeModel>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
