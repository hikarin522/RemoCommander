using ConsoleAppFramework;

using RemoCommander.CloudApi;

namespace RemoCommander.Cli;

public class Commands(NatureRemoCloudClient cloudClient)
{
    // SESAME BOTを操作
    /// <summary>Control SESAME BOT</summary>
    /// <param name="applianceId">-a, Appliance ID</param>
    [Command("sesame-bot")]
    public async Task SesameBotAsync(string applianceId, CancellationToken cancellationToken = default)
    {
        await cloudClient.ClickAsync(applianceId, cancellationToken);
        Console.WriteLine($"SESAME BOT clicked: {applianceId}");
    }

    // IR信号を送信
    /// <summary>Send IR signal</summary>
    /// <param name="signalId">-s, Signal ID</param>
    [Command("ir")]
    public async Task IrAsync(string signalId, CancellationToken cancellationToken = default)
    {
        await cloudClient.SendAsync(signalId, new EmptyObject(), cancellationToken);
        Console.WriteLine($"Signal sent: {signalId}");
    }

    // エアコンの設定を変更
    /// <summary>Change air conditioner settings</summary>
    /// <param name="applianceId">-a, Appliance ID</param>
    /// <param name="temperature">-t, Temperature</param>
    /// <param name="mode">-m, Operation mode</param>
    /// <param name="volume">-v, Air volume</param>
    /// <param name="direction">-d, Air direction</param>
    [Command("aircon")]
    public async Task AirConAsync(
        string applianceId,
        string? temperature = null,
        string? mode = null,
        string? volume = null,
        string? direction = null,
        CancellationToken cancellationToken = default)
    {
        var appliances = await cloudClient.AppliancesAllAsync(cancellationToken);
        var targetAppliance = appliances.FirstOrDefault(a => a.Id == applianceId);

        if (targetAppliance?.Settings == null) {
            Console.Error.WriteLine($"エラー: エアコン '{applianceId}' が見つからないか、設定情報を取得できませんでした");
            return;
        }

        AirConParams_ parameters = new() {
            Temperature = temperature ?? targetAppliance.Settings.Temp ?? "",
            Operation_mode = mode ?? targetAppliance.Settings.Mode ?? "",
            Air_volume = volume ?? targetAppliance.Settings.Vol ?? "",
            Air_direction = direction ?? targetAppliance.Settings.Dir ?? "",
            Air_direction_h = targetAppliance.Settings.Dirh ?? "",
            Button = targetAppliance.Settings.Button ?? ""
        };

        await cloudClient.Aircon_settingsAsync(applianceId, parameters, cancellationToken);

        Console.WriteLine($"Air conditioner settings updated: {applianceId}");
        Console.WriteLine($"   Temperature: {parameters.Temperature}°C");
        Console.WriteLine($"   Mode: {parameters.Operation_mode}");
        Console.WriteLine($"   Air volume: {parameters.Air_volume}");
        Console.WriteLine($"   Air direction: {parameters.Air_direction}");
    }

    // ライトのボタンを送信
    /// <summary>Send light button</summary>
    /// <param name="applianceId">-a, Appliance ID</param>
    /// <param name="button">-b, Button name</param>
    [Command("light")]
    public async Task LightAsync(string applianceId, string button, CancellationToken cancellationToken = default)
    {
        await cloudClient.LightAsync(applianceId, new LightParams {
            Button = button
        }, cancellationToken);
        Console.WriteLine($"Light button '{button}' sent: {applianceId}");
    }

    // TVのボタンを送信
    /// <summary>Send TV button</summary>
    /// <param name="applianceId">-a, Appliance ID</param>
    /// <param name="button">-b, Button name</param>
    [Command("tv")]
    public async Task TVAsync(string applianceId, string button, CancellationToken cancellationToken = default)
    {
        await cloudClient.TvAsync(applianceId, new TVParams {
            Button = button
        }, cancellationToken);
        Console.WriteLine($"TV button '{button}' sent: {applianceId}");
    }

}
