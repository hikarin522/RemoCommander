# RemoCommander.LocalApi

Nature Remo Local API クライアントライブラリ (NSwag 自動生成)

## 概要

Nature Remo デバイスのローカルAPI (http://remo.local/) を操作するための .NET クライアントライブラリです。

**NSwag** を使用して Swagger/OpenAPI 仕様から自動生成されたクライアントコードです。
生成されたコードは `Generated/` ディレクトリに配置され、mDNSデバイス検出機能と組み合わせて使用します。

## 自動生成の仕組み

### 生成元

- **Swagger YAML**: `swagger.yml` (https://local-swagger.nature.global/swagger.yml から取得)
- **NSwag 設定**: `nswag.json`
- **出力先**: `Generated/NatureRemoLocalClient.cs`

### NSwag 設定 (nswag.json)

```json
{
  "runtime": "Net100",
  "documentGenerator": {
    "fromDocument": {
      "json": "swagger.yml"
    }
  },
  "codeGenerators": {
    "openApiToCSharpClient": {
      "className": "NatureRemoLocalClient",
      "namespace": "RemoCommander.LocalApi",
      "output": "Generated/NatureRemoLocalClient.cs",
      "jsonLibrary": "SystemTextJson"
    }
  }
}
```

### クライアント再生成コマンド

**注意**: クライアントコードは `dotnet build` 時に自動的に生成されます。

ソリューションルートの `Directory.Build.targets` に NSwag のビルドタスクが設定されており、
`nswag.json` が存在するプロジェクトでは `BeforeCompile` 時に自動的にクライアントコードが生成されます。

```xml
<!-- Directory.Build.targets の設定例 -->
<Target Name="GenerateNSwagClient" BeforeTargets="BeforeCompile"
        Condition="Exists('$(MSBuildProjectDirectory)\nswag.json')">
  <Exec Command="dotnet nswag run nswag.json"
        WorkingDirectory="$(MSBuildProjectDirectory)"
        Condition="!Exists('$(MSBuildProjectDirectory)\Generated')" />
</Target>
```

このため、通常は手動でコマンドを実行する必要はありません。

手動で再生成する場合は以下のコマンドを使用します:

```bash
# NSwag CLI を使用
nswag run nswag.json

# または swagger.yml を更新してから再生成
curl https://local-swagger.nature.global/swagger.yml -o swagger.yml
nswag run nswag.json
```

## ファイル構成

### 自動生成されるファイル

- `Generated/NatureRemoLocalClient.cs`: NSwag で自動生成されたクライアントコード
  - `NatureRemoLocalClient` クラス
  - `IRSignal` モデル
  - `Data` モデル (IR信号データ)
  - `ApiException` クラス

### 手動で作成したカスタムファイル

- `DeviceDiscoveryService.cs`: mDNS (Zeroconf) によるデバイス検出サービス
  - `DiscoveredDevice` モデル
  - `DeviceDiscoveryService` クラス

## 生成されるクライアントの特徴

- **System.Text.Json** を使用
- **非同期メソッドのみ** (同期メソッドは生成しない)
- **BaseUrl プロパティ** でデバイスのIPアドレスを指定可能 (デフォルト: `http://remo.local/`)
- **認証不要** (ローカルネットワーク内で動作)
- **データアノテーション** による検証サポート

## 生成される API メソッド

### GET /messages - IR信号受信

```csharp
Task<IRSignal> MessagesGETAsync(CancellationToken cancellationToken = default)
```

最新の受信IR信号を取得します。

### POST /messages - IR信号送信

```csharp
Task MessagesPOSTAsync(IRSignal message, CancellationToken cancellationToken = default)
```

指定されたIR信号を送信します。

## IRSignal モデル

```csharp
public partial class IRSignal
{
    [JsonPropertyName("freq")]
    [Range(30, 80)]
    public int? Freq { get; set; }  // サブキャリア周波数 (kHz)

    [JsonPropertyName("data")]
    public Data Data { get; set; }  // IR信号データ (時間間隔配列)

    [JsonPropertyName("format")]
    public string Format { get; set; }  // フォーマット ("nec", "raw" など)
}
```

## mDNS デバイス検出

### DeviceDiscoveryService

```csharp
public class DeviceDiscoveryService
{
    public async Task<List<DiscoveredDevice>> DiscoverDevicesAsync(
        int scanTimeSeconds = 5,
        CancellationToken cancellationToken = default)
}
```

ネットワーク上の Nature Remo デバイスを `_remo._tcp.local.` サービスタイプで検出します。

### DiscoveredDevice モデル

```csharp
public class DiscoveredDevice
{
    public required string Name { get; init; }       // デバイス表示名
    public required string Host { get; init; }       // mDNSホスト名
    public required string IPAddress { get; init; }  // IPアドレス
    public required int Port { get; init; }          // ポート番号
}
```

## 使用例

### デバイス検出と接続

```csharp
using RemoCommander.LocalApi;

// デバイスを検出
var discoveryService = new DeviceDiscoveryService();
var devices = await discoveryService.DiscoverDevicesAsync(scanTimeSeconds: 5);

if (devices.Count > 0)
{
    var device = devices[0];
    Console.WriteLine($"Found: {device.Name} at {device.IPAddress}");

    // クライアントを作成してデバイスに接続
    var httpClient = new HttpClient();
    var client = new NatureRemoLocalClient(httpClient);
    client.BaseUrl = $"http://{device.IPAddress}/";
}
```

### IR信号送信

```csharp
// NEC フォーマットのIR信号を送信
var signal = new IRSignal
{
    Format = "nec",
    Freq = 38,  // 38 kHz
    Data = new Data { 3500, 1700, 400, 400, 400, 1200, 400, 400 }
};

await client.MessagesPOSTAsync(signal);
```

### IR信号受信

```csharp
// リモコンから送信された信号を受信
var receivedSignal = await client.MessagesGETAsync();

Console.WriteLine($"Format: {receivedSignal.Format}");
Console.WriteLine($"Frequency: {receivedSignal.Freq} kHz");
Console.WriteLine($"Data: {string.Join(", ", receivedSignal.Data)}");
```

## IR信号フォーマット

### Data 配列

IR信号は ON/OFF の時間間隔シーケンスとして表現されます:

- **偶数インデックス**: ON時間 (マイクロ秒)
- **奇数インデックス**: OFF時間 (マイクロ秒)

例: `[3500, 1700, 400, 400, 400, 1200, 400, 400]`
- 3500μs ON → 1700μs OFF → 400μs ON → 400μs OFF → ...

### サブキャリア周波数 (Freq)

- 範囲: 30-80 kHz
- 一般的な値: 38 kHz (多くの赤外線リモコンで使用)

## 依存パッケージ

- `Zeroconf`: mDNS デバイス検出
- `Microsoft.Extensions.Logging.Abstractions`: ロギング抽象化

## リファレンス

- **Local API ドキュメント**: https://local-swagger.nature.global/
- **Swagger YAML**: https://local-swagger.nature.global/swagger.yml
- **NSwag**: https://github.com/RicoSuter/NSwag
- **NSwag バージョン**: 14.6.1.0
- **Zeroconf**: https://github.com/novotnyllc/Zeroconf

## 開発ワークフロー

1. Nature Remo の Swagger 仕様が更新されたら `swagger.yml` を取得
2. `nswag run nswag.json` でクライアントコードを再生成
3. `Generated/` フォルダ内のコードを確認
4. mDNS検出機能など、必要なカスタムコードを追加・更新

## 注意事項

- ローカルAPIは認証が不要ですが、同一ネットワーク内からのアクセスのみ可能
- デバイス検出には `_remo._tcp.local.` サービスのmDNS広告が必要
- `Freq` は 30-80 kHz の範囲で指定 (データアノテーションで検証)
- `Data` 配列の各要素はマイクロ秒単位
