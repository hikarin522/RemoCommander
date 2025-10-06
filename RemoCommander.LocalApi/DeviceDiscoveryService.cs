using Zeroconf;

namespace RemoCommander.LocalApi;

public class DiscoveredDevice
{
    public required string Name { get; init; }
    public required string Host { get; init; }
    public required string IPAddress { get; init; }
    public required int Port { get; init; }
}

public class DeviceDiscoveryService
{
    private const string ServiceType = "_remo._tcp.local.";

    public async Task<List<DiscoveredDevice>> DiscoverDevicesAsync(int scanTimeSeconds = 5, CancellationToken cancellationToken = default)
    {
        var responses = await ZeroconfResolver.ResolveAsync(
            ServiceType,
            scanTime: TimeSpan.FromSeconds(scanTimeSeconds),
            cancellationToken: cancellationToken);

        return responses
            .SelectMany(
                response => response.Services,
                (response, service) => (response, service))
            .Where(x => x.service.Value.ServiceName == ServiceType)
            .Select(x => new DiscoveredDevice {
                Name = x.response.DisplayName,
                Host = x.response.DisplayName,
                IPAddress = x.response.IPAddress,
                Port = x.service.Value.Port
            })
            .ToList();
    }
}
