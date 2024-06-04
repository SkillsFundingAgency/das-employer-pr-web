using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Web.AppStart;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Validators;
using SFA.DAS.Employer.Shared.UI;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration.LoadConfiguration(builder.Services);

builder.Services
    .AddOptions()
    .AddLogging()
    .AddApplicationInsightsTelemetry()
    .AddHttpContextAccessor()
    .AddServiceRegistrations(configuration)
    .AddAuthenticationServices(configuration)
    .AddSession(configuration)
    .AddValidatorsFromAssembly(typeof(SelectLegalEntitySubmitModelValidator).Assembly)
    .AddHealthChecks(configuration)
    .AddMaMenuConfiguration(RouteNames.SignOut, configuration["ResourceEnvironmentName"]);

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
    builder.Services.AddDataProtection(configuration);
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
