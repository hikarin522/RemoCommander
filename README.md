# RemoCommander 🎮

Nature Remo の家電をブラウザから操作できる Blazor WebAssembly アプリケーションです。

## 機能

- 🌐 **Blazor WebAssembly**: 完全にブラウザ内で動作、サーバー不要
- 🔐 **セキュア**: アクセストークンはブラウザの localStorage に保存
- 🏠 **家電操作**: すべての Nature Remo 家電を Web から操作可能
- 🌍 **多言語対応**: 日本語・英語に対応（ブラウザ言語で自動切替）
- 📱 **レスポンシブ**: モバイル・デスクトップ両対応
- 🚀 **GitHub Pages**: 静的ホスティングで簡単デプロイ

## プロジェクト構成

```
RemoCommander/
├── RemoCommander.Web/        # Blazor WebAssembly フロントエンド
├── RemoCommander.CloudApi/   # Nature Remo Cloud API クライアント
├── RemoCommander.LocalApi/   # Nature Remo Local API クライアント
└── RemoCommander.Cli/        # CLI ツール（開発・テスト用）
```

## セットアップ

### 必要な環境

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Nature Remo デバイス
- Nature Remo アクセストークン

### アクセストークンの取得

1. [Nature Remo Console](https://home.nature.global/) にアクセス
2. 「Generate Access Token」をクリック
3. 生成されたトークンをコピー

## 起動方法

### ローカル実行

```bash
# 初回のみ: .NET ツールをインストール
dotnet tool restore

# Blazor WebAssembly アプリを起動
dotnet run --project RemoCommander.Web
```

アクセス: http://localhost:5105

### ビルド

```bash
# 初回のみ: .NET ツールをインストール
dotnet tool restore

# ソリューション全体をビルド
dotnet build RemoCommander.slnx --configuration Release
```

ビルド成果物: `artifacts/bin/RemoCommander.Web/release/`

## 使い方

### 1. アクセストークンの設定

1. アプリケーションを開く
2. 初回アクセス時にトークン入力画面が表示されます
3. 「🔗 Nature Remo のトークンを取得」リンクから [Nature Remo Console](https://home.nature.global/) でトークンを取得
4. 取得したトークンを入力して「保存」をクリック

### 2. 家電の操作

1. ナビゲーションメニューから「家電」をクリック
2. デバイスごとにグループ化された家電が表示されます
3. 各信号ボタンをクリックして操作

### 3. トークンの管理

- トークンはブラウザの localStorage に保存されます
- 「トークンをクリア」ボタンで削除可能
- デバイス/ブラウザごとに個別に管理されます

## GitHub Pages へのデプロイ

このリポジトリには GitHub Actions ワークフローが設定されています。

### 自動デプロイ

1. GitHub リポジトリの Settings → Pages で Source を "GitHub Actions" に設定
2. `master` ブランチにプッシュすると自動的にデプロイされます

### 手動デプロイ

```bash
# ビルド
dotnet publish RemoCommander.Web/RemoCommander.Web.csproj -c Release -o publish

# publish/wwwroot を GitHub Pages にデプロイ
```

## 技術スタック

- .NET 10.0
- C# 13/14 Preview
- Blazor WebAssembly
- ConsoleAppFramework (CLI フレームワーク)
- NSwag (API クライアント生成)
- Microsoft.Extensions.Localization (多言語対応)
- Bootstrap 5
- Nature Remo Cloud API / Local API
- LibMan (クライアントサイドライブラリ管理)
- GitHub Actions (CI/CD)

## 開発

### プロジェクト構成

- **RemoCommander.Web**: Blazor WebAssembly UI
- **RemoCommander.CloudApi**: Nature Remo Cloud API クライアント
- **RemoCommander.LocalApi**: Nature Remo Local API クライアント
- **RemoCommander.Cli**: コマンドラインツール（開発・テスト用）

### ビルド設定

- `Directory.Build.props`: 共通プロジェクト設定（`LangVersion=preview` で C# 13/14 機能を有効化）
- `Directory.Packages.props`: 集中パッケージ管理 (CPM)
- `global.json`: .NET SDK バージョン指定
- `.config/dotnet-tools.json`: .NET ツール管理（NSwag）
- `RemoCommander.Web/libman.json`: クライアントサイドライブラリ管理

### GitHub Actions

- `.github/workflows/build.yml`: 全ブランチでビルドチェック
- `.github/workflows/deploy.yml`: master ブランチで GitHub Pages へ自動デプロイ

## CLI ツール

コマンドラインから Nature Remo を操作できます。

### セットアップ

```bash
# RemoCommander.Cli ディレクトリに移動
cd RemoCommander.Cli

# アクセストークンをユーザーシークレットに保存
dotnet user-secrets set "NatureRemo:Token" "YOUR_TOKEN_HERE"
```

### 使い方

```bash
# ヘルプを表示
dotnet run -- --help

# デバイスと家電の一覧を表示
dotnet run -- list

# 特定のデバイスの家電を表示
dotnet run -- list -d <DEVICE_ID>

# 登録されているデバイス一覧を表示
dotnet run -- list device
dotnet run -- list device -h <HOME_ID>

# 登録されているホーム一覧を表示
dotnet run -- list home

# センサーデータを表示
dotnet run -- list sensor
dotnet run -- list sensor -h <HOME_ID>

# IR 信号を送信
dotnet run -- ir -s <SIGNAL_ID>

# エアコンの設定を変更
dotnet run -- aircon -a <APPLIANCE_ID> -t 25 -m cool

# ライトのボタンを送信
dotnet run -- light -a <APPLIANCE_ID> -b on

# TV のボタンを送信
dotnet run -- tv -a <APPLIANCE_ID> -b power

# SESAME BOT を操作
dotnet run -- sesame-bot -a <APPLIANCE_ID>

# ローカル API: ネットワーク上のデバイスを検出
dotnet run -- local list -t 5

# ローカル API: IR 信号を送信
dotnet run -- local send -h 192.168.1.100 -d 100 200 300 -f 38

# ローカル API: 最後に受信した IR 信号を取得
dotnet run -- local get -h 192.168.1.100
```

## トラブルシューティング

**Q: ビルドエラーが出る**
- .NET 10 SDK がインストールされているか確認: `dotnet --version`
- .NET ツールをインストール: `dotnet tool restore`
- クリーンビルド: `dotnet clean && dotnet build`

**Q: 家電が表示されない**
- アクセストークンが正しいか確認
- ブラウザの開発者ツール (F12) でエラーを確認
- Nature Remo API が利用可能か確認

## ライセンス

MIT
