using System.Text.Json;
using System.Text.Json.Serialization;

namespace RemoCommander.CloudApi;

public partial class NatureRemoCloudClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings)
    {
        settings.PropertyNameCaseInsensitive = true;
        settings.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        settings.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        settings.Converters.Add(new UserIndexConverter());
    }
}
