using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using JdhPro.Web;
using JdhPro.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configure base path for GitHub Pages deployment
var baseAddress = new Uri(builder.HostEnvironment.BaseAddress);
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = baseAddress });

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped<ContentService>();

// Contact provider services - Changed from Singleton to Scoped to fix DI error
builder.Services.AddScoped<MailtoContactProvider>();
builder.Services.AddScoped<MxRouteContactProvider>();
builder.Services.AddScoped<ContactProviderFactory>();

await builder.Build().RunAsync();
