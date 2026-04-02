using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using WSIST.Engine;
using WSIST.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

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
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Google:ClientId"]
            ?? throw new InvalidOperationException("Google:ClientId is missing");
        options.ClientSecret = builder.Configuration["Google:ClientSecret"]
            ?? throw new InvalidOperationException("Google:ClientSecret is missing");
    });

builder.Services.AddScoped<TestManagement>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.Use(async (context, next) =>
{
    var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;
    if (context.Request.Path == "/")
    {
        Console.WriteLine($"PATH: / | AUTH: {isAuthenticated}");
        foreach (var cookie in context.Request.Cookies)
            Console.WriteLine($"  COOKIE: {cookie.Key} = {cookie.Value[..Math.Min(20, cookie.Value.Length)]}...");
    }
    if (context.Request.Path == "/" && !isAuthenticated)
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
    ctx.Response.Cookies.Delete(".AspNetCore.Cookies", new CookieOptions 
    { 
        Path = "/",
        IsEssential = true
    });
    return Results.Redirect("/login-page");
}).AllowAnonymous();

app.Run();