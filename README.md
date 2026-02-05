# Unity Terminal (Editor Only)

このリポジトリは Unity 向けの **エディタ拡張専用** ライブラリの初期構成です。

## 構成

- `package.json`: UPM パッケージ定義
- `Editor/UnityTerminal.Editor.asmdef`: Editor 限定の Assembly Definition
- `Editor/UnityTerminalMenu.cs`: 初期動作確認用メニュー

## 使い方

1. Unity プロジェクトの `Packages/manifest.json` からローカル参照する
2. Unity エディタ上で `Tools > Unity Terminal > About` を選択
3. ダイアログが表示されれば初期化完了
