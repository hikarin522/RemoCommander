using ConsoleAppFramework;

using RemoCommander.LocalApi;

namespace RemoCommander.Cli;

public class LocalCommands(DeviceDiscoveryService discoveryService)
{
    // ローカルネットワーク上のNature Remoデバイスを一覧表示
    /// <summary>List Nature Remo devices on local network</summary>
    /// <param name="scanTime">-t, Scan time in seconds</param>
    [Command("list")]
    public async Task ListAsync(int scanTime = 5, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Scanning for Nature Remo devices on local network... ({scanTime} seconds)");

        var devices = await discoveryService.DiscoverDevicesAsync(scanTime, cancellationToken);

        if (devices.Count == 0) {
            Console.WriteLine("No devices found");
            return;
        }

        Console.WriteLine($"\nFound {devices.Count} device(s):\n");

        foreach (var device in devices) {
            Console.WriteLine($"  {device.Name}");
            Console.WriteLine($"     Host: {device.Host}");
            Console.WriteLine($"     IP: {device.IPAddress}");
            Console.WriteLine($"     Port: {device.Port}");
            Console.WriteLine();
        }
    }

    // IR信号を送信
    /// <summary>Send IR signal</summary>
    /// <param name="host">-h, Host name or IP address</param>
    /// <param name="data">-d, IR signal data</param>
    /// <param name="freq">-f, Frequency in kHz</param>
    [Command("send")]
    public async Task SendAsync(string host, int[] data, int freq = 38, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(host)) {
            Console.Error.WriteLine("Error: Host name not specified");
            return;
        }

        if (freq < 20 || freq > 60) {
            Console.Error.WriteLine($"Warning: Frequency {freq} kHz is outside normal range (20-60 kHz)");
        }

        if (data.Length == 0) {
            Console.Error.WriteLine("Error: Data not specified (e.g., 100 200 300)");
            return;
        }

        NatureRemoLocalClient client = new(new() {
            BaseAddress = new($"http://{host}")
        });

        await client.MessagesPOSTAsync(new IRSignal {
            Format = "us",
            Freq = freq,
            Data = [.. data]
        }, cancellationToken);
        Console.WriteLine($"IR signal sent: {host}");
    }

    // 最後に受信したIR信号を取得
    /// <summary>Get last received IR signal</summary>
    /// <param name="host">-h, Host name or IP address</param>
    [Command("get")]
    public async Task GetAsync(string host, CancellationToken cancellationToken = default)
    {
        NatureRemoLocalClient client = new(new() {
            BaseAddress = new($"http://{host}")
        });

        var message = await client.MessagesGETAsync(cancellationToken);

        if (message == null) {
            Console.Error.WriteLine("Failed to get IR signal");
            return;
        }

        Console.WriteLine("Last received IR signal:");
        Console.WriteLine($"Format: {message.Format}");
        Console.WriteLine($"Freq: {message.Freq}");
        Console.WriteLine($"Data: [{string.Join(", ", message.Data ?? [])}]");
    }
}
