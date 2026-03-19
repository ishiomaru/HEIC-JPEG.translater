# HEIC to JPEG Converter

指定フォルダ内の HEIC 画像を **高速・一括で JPEG に変換** する Windows 向け CLI ツールです。  
フォルダに `.exe` を置いてダブルクリックするだけで、元ファイルを残したまま JPEG 変換が完了します。

---

## ✨ 特徴

| 機能 | 説明 |
|---|---|
| ⚡ **高速並列変換** | CPU の論理コア数に応じた `Parallel.ForEachAsync` による並列処理 |
| 🖥️ **WIC ネイティブ処理** | Windows Imaging Component (WIC) を直接呼び出し、OS レベルの GPU デコードを透過的に利用 |
| 📸 **Exif メタデータ保持** | 撮影日時・GPS 情報などの Exif データを JPEG にそのまま引き継ぎ |
| 📦 **単体 exe** | Native AOT コンパイルにより .NET ランタイム不要の自己完結型バイナリ |
| 🛡️ **堅牢なエラー処理** | 破損ファイルはスキップし、全体の処理を止めずに続行 |
| 🔁 **重複スキップ** | 変換済みの同名 JPEG が存在する場合は自動でスキップ |

---

## 📋 前提条件

- **OS**: Windows 10 (build 17763) 以降
- **必須拡張機能** (Microsoft Store からインストール):
  - [HEIF 画像拡張機能](https://www.microsoft.com/store/productId/9PMMSR1CGPWG)
  - [HEVC ビデオ拡張機能](https://www.microsoft.com/store/productId/9NMZLZ57R3T7)

---

## 🚀 使い方

### 1. exe を配置する

変換したい `.heic` ファイルが入っているフォルダに `HeicToJpegConverter.exe` を配置します。

### 2. ダブルクリックで実行

exe をダブルクリックすると、自動的に同一フォルダ内の HEIC ファイルを検出し、変換処理を開始します。

### 3. 結果を確認

変換後の JPEG ファイルは、同階層に作成される **`{フォルダ名}_output`** フォルダに保存されます。

```
Photos/
├── IMG_0001.heic
├── IMG_0002.heic
├── HeicToJpegConverter.exe
└── Photos_output/          ← 自動生成
    ├── IMG_0001.jpg
    └── IMG_0002.jpg
```

### コンソール出力例

```text
HEIC to JPEG Converter
Scanning directory: C:\Users\Owner\Pictures\Photos
Found 150 HEIC files to convert.
Skipping 10 files (already exist).

Progress: 140/140 (Success: 139, Error: 1)

Conversion Finished!
Total Success: 139
Total Errors: 1
Total Skipped: 10

Press Enter to exit...
```

---

## 🔧 変換仕様

| 項目 | 値 |
|---|---|
| 入力形式 | `.heic`, `.HEIC` |
| 出力形式 | `.jpg` |
| JPEG 品質 | **95** (0.95) |
| スキャン範囲 | フォルダ直下のみ (サブフォルダは対象外) |
| 隠しファイル / システムファイル | スキップ |

---

## 🏗️ ビルド

### 必要な環境

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10 以降

### 通常ビルド

```powershell
cd src\HeicToJpegConverter
dotnet build -c Release
```

### Native AOT パブリッシュ (単体 exe 生成)

```powershell
cd src\HeicToJpegConverter
dotnet publish -c Release -r win-x64
```

生成された実行ファイルは `bin\Release\net8.0-windows10.0.17763.0\win-x64\publish\` に出力されます。

---

## 📁 プロジェクト構成

```
HEIC-JPEG.translater/
├── README.md
├── docs/
│   ├── Requirements.md        # 要件定義書
│   └── Specification.md       # 仕様書
└── src/
    └── HeicToJpegConverter/
        ├── HeicToJpegConverter.csproj   # プロジェクト設定 (.NET 8 / Native AOT)
        ├── Program.cs                   # エントリポイント・並列処理制御
        ├── Converter.cs                 # WIC によるデコード・エンコード・メタデータ転送
        ├── Scanner.cs                   # ファイル走査・スキップ判定・出力先管理
        └── NativeMethods.txt            # CsWin32 ネイティブ API 定義
```

---

## 🛠️ 技術スタック

- **C# / .NET 8** — メインの開発言語・フレームワーク
- **Native AOT** — ランタイム不要の単体 exe を生成
- **WIC (Windows Imaging Component)** — HEIC デコード・JPEG エンコードのネイティブ処理
- **TerraFX.Interop.Windows** — WIC / COM の P/Invoke バインディング
- **Parallel.ForEachAsync** — CPU コア数に応じた効率的なタスク並列処理

---

## 📄 ライセンス

このプロジェクトのライセンスは未定義です。
