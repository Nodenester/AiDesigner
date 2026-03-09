using AiDesigner.Server.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Components.Web;
using Stripe;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;
using AiDesigner.Areas.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using AiDesigner.Server.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using WebPWrecover.Services;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
StripeConfiguration.ApiKey = "sk_live_YOUR_KEY_HERE";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();

builder.Services.AddAuthentication();

builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSingleton<DBConnection>(new DBConnection(connectionString));
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AiDesigner.Server.Data.AuthorizationMiddlewareResultHandler>();

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

var app = builder.Build();

//Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseStatusCodePagesWithReExecute("/StatusCode/{0}");

app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorPages();
app.MapControllers();
app.MapRazorComponents<AiDesigner.Server.App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(AiDesigner.Client._Imports).Assembly);

app.Run();