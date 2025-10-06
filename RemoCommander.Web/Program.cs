using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using RemoCommander.CloudApi;
using RemoCommander.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register localization (culture is automatically detected from browser's Accept-Language header)
builder.Services.AddLocalization();

// Register Nature Remo Cloud API services
var apiBaseUrl = builder.Configuration["NatureRemo:ApiBaseUrl"]
    ?? throw new InvalidOperationException("NatureRemo:ApiBaseUrl configuration is required");

builder.Services.AddHttpClient<NatureRemoCloudClient>(client => client.BaseAddress = new(apiBaseUrl));

await builder.Build().RunAsync();
