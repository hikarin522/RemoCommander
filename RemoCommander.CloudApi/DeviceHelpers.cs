namespace RemoCommander.CloudApi;

/// <summary>
/// Nature Remo デバイスの種類
/// </summary>
public enum DeviceType
{
    /// <summary>不明なデバイス</summary>
    Unknown,
    /// <summary>Remo (第1世代)</summary>
    Remo,
    /// <summary>Remo mini</summary>
    RemoMini,
    /// <summary>Remo 3</summary>
    Remo3,
    /// <summary>Remo E (スマート電源タップ)</summary>
    RemoE
}

/// <summary>
/// センサーの種類
/// </summary>
public enum SensorType
{
    /// <summary>不明なセンサー</summary>
    Unknown,
    /// <summary>温度センサー</summary>
    Temperature,
    /// <summary>湿度センサー</summary>
    Humidity,
    /// <summary>照度センサー</summary>
    Illumination,
    /// <summary>モーションセンサー</summary>
    Motion
}

/// <summary>
/// 家電の種類
/// </summary>
public enum ApplianceType
{
    /// <summary>不明な家電</summary>
    Unknown,
    /// <summary>エアコン</summary>
    AC,
    /// <summary>テレビ</summary>
    TV,
    /// <summary>照明</summary>
    Light,
    /// <summary>赤外線リモコン</summary>
    IR,
    /// <summary>SESAME BOT</summary>
    SesameBot
}

/// <summary>
/// Nature Remo デバイス関連のヘルパーメソッド
/// </summary>
public static class DeviceHelpers
{
    /// <summary>
    /// ファームウェアバージョン文字列からデバイスタイプを判定
    /// </summary>
    /// <param name="firmwareVersion">ファームウェアバージョン (例: "Remo-E/1.0.0")</param>
    /// <returns>検出された <see cref="DeviceType"/></returns>
    public static DeviceType GetDeviceType(string? firmwareVersion) =>
        firmwareVersion switch {
            null => DeviceType.Unknown,
            var v when v.Contains("Remo-E") => DeviceType.RemoE,
            var v when v.Contains("Remo-mini") => DeviceType.RemoMini,
            var v when v.Contains("Remo-3") => DeviceType.Remo3,
            var v when v.Contains("Remo") => DeviceType.Remo,
            _ => DeviceType.Unknown
        };
}

/// <summary>
/// センサー関連のヘルパーメソッド
/// </summary>
public static class SensorHelpers
{
    /// <summary>
    /// センサーキー文字列からセンサータイプを判定
    /// </summary>
    /// <param name="sensorKey">センサーキー ("te", "hu", "il", "mo")</param>
    /// <returns>センサータイプ</returns>
    public static SensorType GetSensorType(string sensorKey) =>
        sensorKey switch {
            "te" => SensorType.Temperature,
            "hu" => SensorType.Humidity,
            "il" => SensorType.Illumination,
            "mo" => SensorType.Motion,
            _ => SensorType.Unknown
        };

    /// <summary>
    /// センサータイプの単位を取得
    /// </summary>
    public static string GetSensorUnit(SensorType sensorType) =>
        sensorType switch {
            SensorType.Temperature => "°C",
            SensorType.Humidity => "%",
            SensorType.Illumination => "lux",
            _ => ""
        };

    /// <summary>
    /// センサーキーから単位を取得（便利なオーバーロード）
    /// </summary>
    public static string GetSensorUnit(string sensorKey) =>
        GetSensorUnit(GetSensorType(sensorKey));
}

/// <summary>
/// 家電関連のヘルパーメソッド
/// </summary>
public static class ApplianceHelpers
{
    /// <summary>
    /// 家電タイプ文字列から <see cref="ApplianceType"/> を判定
    /// </summary>
    /// <param name="type">家電タイプ ("AC", "TV", "LIGHT", "IR", "BLE_SESAME_BOT")</param>
    /// <returns>家電タイプ</returns>
    public static ApplianceType GetApplianceType(string? type) =>
        type switch {
            "AC" => ApplianceType.AC,
            "TV" => ApplianceType.TV,
            "LIGHT" => ApplianceType.Light,
            "IR" => ApplianceType.IR,
            "BLE_SESAME_BOT" => ApplianceType.SesameBot,
            _ => ApplianceType.Unknown
        };
}
