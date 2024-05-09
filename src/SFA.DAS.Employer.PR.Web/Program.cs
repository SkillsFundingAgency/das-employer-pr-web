using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Web.AppStart;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.Shared.UI;

var builder = WebApplication.CreateBuilder(args);

var rootConfiguration = builder.Configuration.LoadConfiguration(builder.Services);

builder.Services
    .AddOptions()
    .AddLogging()
    .AddApplicationInsightsTelemetry()
    .AddHttpContextAccessor()
    .AddServiceRegistrations(rootConfiguration)
    .AddAuthenticationServices(rootConfiguration)
    .AddSession(rootConfiguration)
    .AddMaMenuConfiguration(RouteNames.SignOut, rootConfiguration["ResourceEnvironmentName"]);

builder.Services.AddHealthChecks();

builder.Services
     .Configure<RouteOptions>(options => { options.LowercaseUrls = false; })
     .AddMvc(options =>
     {
         options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();
     })
     .AddSessionStateTempDataProvider();

#if DEBUG
builder.Services.AddControllersWithViews().AddControllersAsServices().AddRazorRuntimeCompilation();
#endif

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddDataProtection(rootConfiguration);
}


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app
        .UseHsts()
        .UseExceptionHandler("/error")
        .UseStatusCodePagesWithRedirects("/error/{0}");
}

app
    .UseHealthChecks("/ping")
    .UseHttpsRedirection()
    .UseStaticFiles()
    .UseCookiePolicy()
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization()
    .UseSession()
    .UseMiddleware<SecurityHeadersMiddleware>()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            "default",
            "{controller=Home}/{action=Index}/{id?}");
    });

app.Run();
