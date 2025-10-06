using System.Net.Http.Headers;
using System.Threading;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

using RemoCommander.CloudApi;

namespace RemoCommander.Web.Pages;

public partial class Appliances: IDisposable
{
    [Inject] private IHttpClientFactory HttpClientFactory { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

    private string? token;
    private NatureRemoCloudClient? client;
    private ICollection<ApplianceResponse>? appliances;
    private ICollection<DeviceResponse>? devices;
    private bool loading = false;
    private bool sending = false;
    private string? error;
    private string? lastAction;
    private Timer? pollingTimer;
    private bool isPolling = false;

    protected override async Task OnInitializedAsync()
    {
        token = await JS.InvokeAsync<string>("localStorage.getItem", StorageKeys.NatureRemoToken);
        if (!string.IsNullOrEmpty(token)) {
            // Create HttpClient with Bearer token
            var httpClient = HttpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://api.nature.global");
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Create NSwag client with authenticated HttpClient
            client = new NatureRemoCloudClient(httpClient);

            await LoadAppliancesAsync();
            StartPolling();
        }
    }

    private void StartPolling()
    {
        pollingTimer?.Dispose();
        pollingTimer = new Timer(async _ => {
            if (isPolling) {
                return; // Prevent overlapping calls
            }

            isPolling = true;
            try {
                await InvokeAsync(async () => {
                    try {
                        await LoadAppliancesAsync();
                        StateHasChanged();
                    } catch (Exception ex) {
                        Console.Error.WriteLine($"Polling error: {ex.Message}");
                    }
                });
            } finally {
                isPolling = false;
            }
        }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    private void StopPolling()
    {
        pollingTimer?.Dispose();
        pollingTimer = null;
    }

    public void Dispose()
    {
        StopPolling();
    }

    private async Task LoadAppliancesAsync()
    {
        if (client == null) {
            return;
        }

        loading = true;
        error = null;
        lastAction = null;

        try {
            appliances = await client.AppliancesAllAsync();
            devices = await client.DevicesAllAsync();

            if (appliances == null || devices == null) {
                error = L["Appliances.LoadFailed"];
                return;
            }
        } catch (HttpRequestException ex) {
            error = $"{L["Appliances.Error"]} {L["Appliances.NetworkError"]} {ex.Message}";
            Console.Error.WriteLine($"Network error loading appliances: {ex}");
        } catch (Exception ex) {
            error = $"{L["Appliances.Error"]} {ex.Message}";
            Console.Error.WriteLine($"Unexpected error loading appliances: {ex}");
        } finally {
            loading = false;
        }
    }

    private async Task ExecuteApiCallAsync(Func<Task> apiCall)
    {
        if (client == null) {
            return;
        }

        sending = true;
        lastAction = null;
        error = null;

        try {
            await apiCall();
        } catch (HttpRequestException ex) {
            error = $"{L["Appliances.Error"]} {L["Appliances.NetworkError"]} {ex.Message}";
            Console.Error.WriteLine($"Network error executing API call: {ex}");
        } catch (Exception ex) {
            error = $"{L["Appliances.Error"]} {ex.Message}";
            Console.Error.WriteLine($"Unexpected error executing API call: {ex}");
        } finally {
            sending = false;
            StateHasChanged();
        }
    }

    private Task PressLightButtonAsync(string applianceId, string? buttonName, string? buttonLabel) =>
        ExecuteApiCallAsync(async () => {
            var body = new LightParams { Button = buttonName };
            await client!.LightAsync(applianceId, body);
            lastAction = string.Format(L["Light.ButtonPressed"].Value, buttonLabel);
        });

    private Task PressTvButtonAsync(string applianceId, string? buttonName, string? buttonLabel) =>
        ExecuteApiCallAsync(async () => {
            var body = new TVParams { Button = buttonName };
            await client!.TvAsync(applianceId, body);
            lastAction = string.Format(L["TV.ButtonPressed"].Value, buttonLabel);
        });

    private Task SendSignalAsync(string? signalId, string? signalName) =>
        ExecuteApiCallAsync(async () => {
            await client!.SendAsync(signalId, new EmptyObject());
            lastAction = $"{L["Appliances.SignalSent"]} {signalName}";
        });

    private Task ClickSesameBotAsync(string applianceId, string? nickname) =>
        ExecuteApiCallAsync(async () => {
            await client!.ClickAsync(applianceId);
            lastAction = string.Format(L["SesameBot.Clicked"].Value, nickname);
        });

    private Task AdjustTemperatureAsync(string applianceId, string? nickname, AirconSettingsResponsePtr? settings, int delta)
    {
        var currentTemp = int.TryParse(settings?.Temp, out var temp) ? temp : 26;
        var newTemp = (currentTemp + delta).ToString();
        var mode = settings?.Mode ?? "cool";
        return ControlAirConAsync(applianceId, nickname, newTemp, mode, powerOff: false);
    }

    private Task ControlAirConAsync(string applianceId, string? nickname, string temperature, string mode, bool powerOff) =>
        ExecuteApiCallAsync(async () => {
            var body = new AirConParams_ {
                Temperature = temperature,
                Operation_mode = mode,
                Air_volume = "",
                Air_direction = "",
                Button = powerOff ? "power-off" : ""
            };

            await client!.Aircon_settingsAsync(applianceId, body);

            lastAction = powerOff
                ? string.Format(L["AirCon.TurnedOff"].Value, nickname)
                : string.Format(L["AirCon.SettingsUpdated"].Value, nickname, temperature, mode);

            // 設定を反映するために再読み込み
            await LoadAppliancesAsync();
        });
}
