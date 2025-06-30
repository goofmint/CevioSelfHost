# CeVIO Web API Server

CeVIO AI をローカルまたは LAN 内から HTTP API 経由で操作できる Windows 向け自己ホスト型 Web サーバーです。  
文章を読み上げ、音声ファイル（WAV）を返却します。

## 🔧 必要環境

- Windows 10 / 11
- .NET Framework 4.7.2
- [CeVIO AI](https://cevio.jp/)（インストール済み）
- `CeVIO.Talk.RemoteService2.dll` にアクセス可能であること

## 🚀 起動方法

1. Visual Studio またはビルド済みの `CevioSelfHost.exe` を実行
2. 起動後、以下のようなメッセージが表示されます:

```
CeVIO Web API running at http://192.168.0.2:5000/
```

3. 上記のIPアドレスを使ってリクエストできます。

## 🔁 APIエンドポイント

### `POST /speak`

CeVIOで音声合成を行い、WAVファイルを返却します。

#### リクエスト

- `Content-Type`: `application/json`
- ボディ:

```json
{
"text": "こんにちは、テストです。",
"language": "ja",
"volume": 80,
"speed": 60,
"tone": 50,
"alpha": 30,
"toneScale": 70
}
````

#### パラメータ一覧

| パラメータ       | 型      | 説明                     | 省略時の動作         |
| ----------- | ------ | ---------------------- | -------------- |
| `text`      | string | 読み上げたい文章（必須）           | ―              |
| `language`  | string | `ja` または `en`          | `ja`           |
| `cast`      | string | キャスト名（例: `"弦巻マキ (日)"`） | 言語に応じて自動設定     |
| `volume`    | uint   | 音量（0〜100）              | デフォルト（CeVIO依存） |
| `speed`     | uint   | 話す速さ（0〜100）            | デフォルト          |
| `tone`      | uint   | 音の高さ（0〜100）            | デフォルト          |
| `alpha`     | uint   | 声質（0〜100）              | デフォルト          |
| `toneScale` | uint   | 抑揚（0〜100）              | デフォルト          |

#### レスポンス

* 成功時：WAVファイル（`audio/wav`）
* 失敗時：`400` または `500` ステータスとエラーメッセージ

## 🧪 実行例（Windowsのcmd）

```cmd
curl -X POST http://192.168.1.19:5000/speak ^
  -H "Content-Type: application/json" ^
  -d "{\"text\":\"こんにちは、テストです。\",\"language\":\"ja\"}" ^
  --output voice.wav
```

## 🔐 ポート5000を開放する

LAN 内アクセスを許可するには、以下の手順を実行してください：

1. 管理者として以下を実行：

```cmd
netsh http add urlacl url=http://+:5000/ user=%USERNAME%
```

2. Windows ファイアウォールで TCP 5000 ポートを許可：

   * 「Windows Defender ファイアウォール」 → 「受信の規則」 → 「新しい規則」
   * 「ポート」→ TCP 5000 → 許可 → 名前は `CeVIO API`

または PowerShell を管理者で起動して、以下を実行。

```
New-NetFirewallRule -DisplayName "CeVIO Web API" -Direction Inbound -Protocol TCP -LocalPort 5000 -Action Allow
```

## 📦 ビルド方法

* Visual Studio で `.csproj` を開き、`Release` ビルド
* 実行ファイル `CevioSelfHost.exe` が出力されます

---

## 🔄 自動起動させる方法（オプション）

* `shell:startup` にショートカットを配置するか、タスクスケジューラで `CevioSelfHost.exe` を登録

---

## 📝 ライセンス

コードはMIT Licenseです。このツールはCeVIO製品の利用規約に従ってご使用ください。
