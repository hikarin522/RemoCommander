using System.Net.Http.Headers;

using ConsoleAppFramework;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RemoCommander.Cli;
using RemoCommander.CloudApi;
using RemoCommander.LocalApi;

var builder = Host.CreateApplicationBuilder(args);

// Configure Nature Remo options
builder.Services.Configure<NatureRemoOptions>(builder.Configuration.GetSection("NatureRemo"));

// Register HttpClient with Bearer token for Cloud API
builder.Services.AddHttpClient<NatureRemoCloudClient>((sp, client) => {
    var options = sp.GetRequiredService<IOptions<NatureRemoOptions>>();
    var token = options.Value.Token;

    if (string.IsNullOrWhiteSpace(token)) {
        throw new InvalidOperationException(
            "Nature Remo API token is not configured. Please set 'NatureRemo:Token' in appsettings.json");
    }

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);
});
// .AddStandardResilienceHandler(); // TODO: .NET 10 RC では未対応

// Register Local API services
builder.Services.AddHttpClient<NatureRemoLocalClient>();
// .AddStandardResilienceHandler(); // TODO: .NET 10 RC では未対応
builder.Services.AddSingleton<DeviceDiscoveryService>();

var app = builder.ToConsoleAppBuilder();
app.Add<Commands>();
app.Add<ListCommands>("list");
app.Add<LocalCommands>("local");
await app.RunAsync(args);
