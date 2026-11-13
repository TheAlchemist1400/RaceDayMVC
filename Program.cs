using RaceDay.Mvc.Services;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Decimal Range attributes and HTML number inputs use '.' (e.g. 0.01).
// Pin number parsing to invariant culture so Windows regional settings
// that use ',' cannot throw FormatException or reject form posts.
var invariantCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentCulture = invariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = invariantCulture;

// Add services to the container.
builder.Services.AddControllersWithViews();

// 2. Enable session storage.
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// 3. Allow services to access the current HTTP context.
builder.Services.AddHttpContextAccessor();

// 4. Register the API HttpClient.
builder.Services.AddHttpClient("RaceDayApi", client =>
{
    string baseUrl = builder.Configuration["ApiSettings:BaseUrl"]
        ?? throw new InvalidOperationException(
            "ApiSettings:BaseUrl is missing.");

    client.BaseAddress = new Uri(baseUrl);
});

// 5. Register our API communication service.
// TODO Going to add Services
builder.Services.AddScoped<RaceDayApiClient>();


var app = builder.Build();


var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(invariantCulture),
    SupportedCultures = new[] { invariantCulture },
    SupportedUICultures = new[] { invariantCulture }
};
// Ignore Accept-Language / cookies so the OS or browser cannot switch
// back to a comma decimal separator.
localizationOptions.RequestCultureProviders.Clear();
app.UseRequestLocalization(localizationOptions);


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

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


/*
 
UseRouting() identifies the requested endpoint.
UseSession() makes session data available.
UseAuthorization() handles authorization.
MapControllerRoute() maps your MVC routes.
 
 */