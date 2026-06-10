using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using WSIST.Engine;
using WSIST.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var connStr = builder.Configuration.GetConnectionString("DatabaseConnection")
    ?? throw new InvalidOperationException("DatabaseConnection string is missing.");
builder.Services.AddDbContext<WsistContext>(options =>
    options.UseMySql(connStr, ServerVersion.AutoDetect(connStr)));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = ".AspNetCore.Cookies";
        options.Cookie.Path = "/";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Google:ClientId"]
            ?? throw new InvalidOperationException("Google:ClientId is missing");
        options.ClientSecret = builder.Configuration["Google:ClientSecret"]
            ?? throw new InvalidOperationException("Google:ClientSecret is missing");
    });

builder.Services.AddScoped<TestManagement>();
builder.Services.AddScoped<PriorityCalculator>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseForwardedHeaders();  
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.Use(async (context, next) =>
{
    var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;
    var protectedPaths = new[] { "/", "/study", "/settings" };
    if (protectedPaths.Contains(context.Request.Path.Value) && !isAuthenticated)
    {
        context.Response.Redirect("/login-page");
        return;
    }
    await next();
});

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapGet("/login", () => Results.Challenge(
    new AuthenticationProperties { RedirectUri = "/" },
    [GoogleDefaults.AuthenticationScheme]));

app.MapGet("/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login-page");
}).AllowAnonymous();

app.MapGet("/api/grades", async (HttpContext ctx, TestManagement management) =>
{
    if (!ctx.User.Identity?.IsAuthenticated ?? true)
        return Results.Unauthorized();

    var email = ctx.User.FindFirst(ClaimTypes.Email)?.Value;
    if (email is null) return Results.Unauthorized();

    var user = management.GetOrCreateUser(
        email,
        ctx.User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown",
        ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? ""
    );

    var averages = management.GetGradeAverages(user.Id);

    return Results.Ok(new
    {
        userId = user.Id,
        subjects = averages
    });
}).RequireAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WsistContext>();
    db.Database.Migrate();
}

app.Run();