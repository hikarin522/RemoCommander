# RemoCommander.CloudApi

Nature Remo Cloud API クライアントライブラリ (NSwag 自動生成)

## 概要

Nature Remo Cloud API (https://developer.nature.global/) を操作するための .NET クライアントライブラリです。

**NSwag** を使用して Swagger/OpenAPI 仕様から自動生成されたクライアントコードです。
生成されたコードは `Generated/` ディレクトリに配置され、カスタムコードと組み合わせて使用します。

## 自動生成の仕組み

### 生成元

- **Swagger JSON**: `swagger.json` (https://swagger.nature.global/swagger.json から取得)
- **NSwag 設定**: `nswag.json`
- **出力先**: `Generated/NatureRemoCloudClient.cs`

### NSwag 設定 (nswag.json)

```json
{
  "runtime": "Net100",
  "documentGenerator": {
    "fromDocument": {
      "json": "swagger.json"
    }
  },
  "codeGenerators": {
    "openApiToCSharpClient": {
      "className": "NatureRemoCloudClient",
      "namespace": "RemoCommander.CloudApi",
      "output": "Generated/NatureRemoCloudClient.cs",
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

# または swagger.json を更新してから再生成
curl https://swagger.nature.global/swagger.json -o swagger.json
nswag run nswag.json
```

## ファイル構成

### 自動生成されるファイル

- `Generated/NatureRemoCloudClient.cs`: NSwag で自動生成されたクライアントコード
  - `NatureRemoCloudClient` クラス (partial class)
  - 各種 DTO モデルクラス
  - API エンドポイントメソッド

### 手動で作成したカスタムファイル

- `NatureRemoCloudClient.cs`: JSON シリアライズ設定のカスタマイズ (partial class)
- `DeviceHelpers.cs`: デバイス・センサー・家電の型定義とヘルパーメソッド
- `UserIndexConverter.cs`: JSON シリアライズ用のカスタムコンバーター

## 生成されるクライアントの特徴

- **System.Text.Json** を使用
- **非同期メソッドのみ** (同期メソッドは生成しない)
- **Partial class** として生成 (カスタマイズ可能)
- **BaseUrl プロパティ** でAPIエンドポイントを変更可能
- **データアノテーション** による検証サポート

## カスタマイズ例

### JSON シリアライズ設定 (NatureRemoCloudClient.cs)

```csharp
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
```

### ヘルパークラス (DeviceHelpers.cs)

```csharp
// デバイスタイプ判定
var deviceType = DeviceHelpers.GetDeviceType("Remo-E/1.0.0");
// => DeviceType.RemoE

// センサー情報
var sensorType = SensorHelpers.GetSensorType("te");
var unit = SensorHelpers.GetSensorUnit("te"); // => "°C"

// 家電タイプ判定
var applianceType = ApplianceHelpers.GetApplianceType("AC");
```

## 使用例

```csharp
using RemoCommander.CloudApi;

var httpClient = new HttpClient();
var client = new NatureRemoCloudClient(httpClient);

// アクセストークンを設定 (Bearer認証)
client.SetAccessToken("your-access-token");

// デバイス一覧を取得
var devices = await client.GetDevicesAsync();

// 家電一覧を取得
var appliances = await client.GetAppliancesAsync();

// IR信号を送信
await client.SendSignalAsync(signalId);
```

## 対応する API エンドポイント

自動生成により、Swagger 仕様に定義されたすべてのエンドポイントがサポートされます:

- `/1/users/me` - ユーザー情報
- `/1/devices` - デバイス一覧
- `/1/appliances` - 家電一覧
- `/1/appliances/{appliance}/signals` - IR信号管理
- `/1/signals/{signal}/send` - IR信号送信
- その他多数

## リファレンス

- **Cloud API ドキュメント**: https://developer.nature.global/
- **Swagger UI**: https://swagger.nature.global/
- **Swagger JSON**: https://swagger.nature.global/swagger.json
- **NSwag**: https://github.com/RicoSuter/NSwag
- **NSwag バージョン**: 14.6.1.0

## 開発ワークフロー

1. Nature Remo の Swagger 仕様が更新されたら `swagger.json` を取得
2. `nswag run nswag.json` でクライアントコードを再生成
3. `Generated/` フォルダ内のコードを確認
4. 必要に応じてカスタムコード (partial class, ヘルパー) を追加・更新
