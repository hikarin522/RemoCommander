using System.Text.Json;

using ConsoleAppFramework;

using RemoCommander.CloudApi;

namespace RemoCommander.Cli;

public class ListCommands(NatureRemoCloudClient cloudClient)
{
    private static readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

    // 全デバイスと配下の家電を表示、またはデバイスIDを指定して特定デバイスの家電を表示
    /// <summary>List all devices with appliances, or appliances for a specific device</summary>
    /// <param name="deviceId">-d, Device ID to filter appliances</param>
    /// <param name="json">Output as JSON</param>
    [Command("")]
    public async Task Root(string? deviceId = null, bool json = false, CancellationToken cancellationToken = default)
    {
        if (deviceId != null) {
            await this.showDeviceAppliancesAsync(deviceId, json, cancellationToken);
        } else {
            await this.showAllDevicesWithAppliancesAsync(json, cancellationToken);
        }
    }

    private async Task showDeviceAppliancesAsync(string deviceId, bool json, CancellationToken cancellationToken)
    {
        var appliances = await cloudClient.AppliancesAll2Async(deviceId, cancellationToken);

        if (appliances == null) {
            Console.Error.WriteLine("Failed to fetch data");
            return;
        }

        if (appliances.Count == 0) {
            Console.Error.WriteLine($"Error: No appliances found for device '{deviceId}'");
            return;
        }

        if (json) {
            Console.WriteLine(JsonSerializer.Serialize(appliances, jsonOptions));
            return;
        }

        Console.WriteLine($"\nDeviceId: {deviceId}");
        printAppliances(appliances);
    }

    private async Task showAllDevicesWithAppliancesAsync(bool json, CancellationToken cancellationToken)
    {
        var devices = await cloudClient.DevicesAllAsync(cancellationToken);

        if (devices == null || devices.Count == 0) {
            Console.WriteLine("No devices found");
            return;
        }

        if (json) {
            var result = new List<object>();
            foreach (var device in devices) {
                var appliances = await cloudClient.AppliancesAll2Async(device.Id, cancellationToken);
                result.Add(new { Device = device, Appliances = appliances });
            }
            Console.WriteLine(JsonSerializer.Serialize(result, jsonOptions));
            return;
        }

        Console.WriteLine("\nDevices and Appliances:\n");

        foreach (var device in devices) {
            printDeviceHeader(device);
            await this.printDeviceAppliancesAsync(device.Id, cancellationToken);
            Console.WriteLine();
        }
    }

    private static string getOnlineStatus(bool? online) =>
        online?.ToString().ToLower() ?? "unknown";

    private static void printDeviceHeader(DeviceResponse device)
    {
        Console.WriteLine($"[{device.Firmware_version}] {device.Name}");
        Console.WriteLine($"  DeviceId: {device.Id}");
        Console.WriteLine($"  Online: {getOnlineStatus(device.Online)}");
    }

    private async Task printDeviceAppliancesAsync(string deviceId, CancellationToken cancellationToken)
    {
        var appliances = await cloudClient.AppliancesAll2Async(deviceId, cancellationToken);
        if (appliances != null && appliances.Count > 0) {
            Console.WriteLine("  Appliances:");
            printAppliances(appliances, "    ");
        } else {
            Console.WriteLine("  Appliances: null");
        }
    }

    // 登録されているデバイスの一覧を表示
    /// <summary>List all registered devices</summary>
    /// <param name="homeId">-h, Home ID to filter devices</param>
    /// <param name="json">Output as JSON</param>
    [Command("device")]
    public async Task DevicesAsync(string? homeId = null, bool json = false, CancellationToken cancellationToken = default)
    {
        var devices = homeId != null
            ? await cloudClient.DevicesAll2Async(homeId, cancellationToken)
            : await cloudClient.DevicesAllAsync(cancellationToken);

        if (devices == null || devices.Count == 0) {
            Console.WriteLine("No devices found");
            return;
        }

        if (json) {
            Console.WriteLine(JsonSerializer.Serialize(devices, jsonOptions));
            return;
        }

        Console.WriteLine("\nDevices:\n");

        foreach (var device in devices) {
            Console.WriteLine($"  [{device.Firmware_version}] {device.Name}");
            Console.WriteLine($"    DeviceId: {device.Id}");
            Console.WriteLine($"    Online: {getOnlineStatus(device.Online)}");
            Console.WriteLine($"    MAC: {device.Mac_address}");
            Console.WriteLine();
        }
    }

    // Remoデバイスのセンサーデータを表示
    /// <summary>Display sensor data from Remo devices</summary>
    /// <param name="homeId">-h, Home ID to filter devices</param>
    /// <param name="json">Output as JSON</param>
    [Command("sensor")]
    public async Task SensorsAsync(string? homeId = null, bool json = false, CancellationToken cancellationToken = default)
    {
        var devices = homeId != null
            ? await cloudClient.DevicesAll2Async(homeId, cancellationToken)
            : await cloudClient.DevicesAllAsync(cancellationToken);

        if (json) {
            Console.WriteLine(JsonSerializer.Serialize(devices, jsonOptions));
            return;
        }

        foreach (var device in devices) {
            Console.WriteLine($"\n[{device.Firmware_version}] {device.Name} (DeviceId: {device.Id})");
            Console.WriteLine($"   Online: {getOnlineStatus(device.Online)}");

            if (device.Newest_events is not { Count: > 0 }) {
                Console.WriteLine("   No sensor data");
                continue;
            }

            Console.WriteLine("   Sensor Data:");

            foreach (var (sensorKey, sensorValue) in device.Newest_events) {
                var sensorType = SensorHelpers.GetSensorType(sensorKey);
                var sensorName = sensorType == SensorType.Unknown ? sensorKey : sensorType.ToString();
                var unit = SensorHelpers.GetSensorUnit(sensorKey);
                Console.WriteLine($"      {sensorName}: {sensorValue.Val:F1}{unit} (Updated: {sensorValue.Created_at})");
            }
        }
    }

    // 登録されているホームの一覧を表示
    /// <summary>List all registered homes</summary>
    /// <param name="json">Output as JSON</param>
    [Command("home")]
    public async Task HomesAsync(bool json = false, CancellationToken cancellationToken = default)
    {
        var homes = await cloudClient.HomesAllAsync(cancellationToken);

        if (homes == null || homes.Count == 0) {
            Console.WriteLine("No homes found");
            return;
        }

        if (json) {
            Console.WriteLine(JsonSerializer.Serialize(homes, jsonOptions));
            return;
        }

        Console.WriteLine("\nHomes:\n");

        foreach (var home in homes) {
            Console.WriteLine($"  {home.Name}");
            Console.WriteLine($"    HomeId: {home.Id}");
            Console.WriteLine();
        }
    }

    private static void printAppliances(IEnumerable<ApplianceResponse> appliances, string indent = "")
    {
        foreach (var appliance in appliances) {
            Console.WriteLine($"{indent}[{appliance.Type}] {appliance.Nickname} (ApplianceId: {appliance.Id})");

            if (appliance.Signals == null) {
                continue;
            }

            foreach (var signal in appliance.Signals) {
                Console.WriteLine($"{indent}  - {signal.Name} (SignalId: {signal.Id})");
            }
        }
    }
}
