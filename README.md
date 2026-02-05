# Unity Terminal (Editor MVP)

Unity エディタ内で macOS PTY を使ったターミナル表示を行う MVP 実装です。

## 追加された構成

- `Native/macOS/pty_bridge.c`: PTY + `/bin/zsh -l` 起動ブリッジ
- `Native/macOS/build.sh`: `Plugins/macOS/libpty_bridge.dylib` をユニバーサル（arm64 + x86_64）でビルドするスクリプト
- `.github/workflows/macos-native-build-pr.yml`: PR 作成時に macOS 向けネイティブライブラリをビルドし、PR ブランチへ自動反映
- `Editor/*`: PTY セッション、ANSI 最小パーサ、スクリーンバッファ、UIElements TerminalView、EditorWindow

## セットアップ（macOS）

1. Unity でパッケージを読み込む
2. `Tools > Unity Terminal > Open Terminal` を開く

> `Plugins/macOS/libpty_bridge.dylib` は GitHub Actions でビルドして PR に含める運用です。
> 通常は手元で追加ビルド不要で、そのままインポートして利用できます。

## ローカルで再ビルドしたい場合

```bash
chmod +x Native/macOS/build.sh
Native/macOS/build.sh
```

## 現在の対応範囲

- PTY spawn / read / write / resize / kill / close
- 矢印キー、Enter、Space、Backspace、Ctrl+C/D/Z
- ANSI/VT 最小対応（カーソル移動、行/画面消去、スクロール、SGRは受理のみ）
- リサイズ時の `pty_resize` + `SIGWINCH`

## 既知の制約（MVP）

- 色の描画（SGR）はまだ UI 反映なし
- コピー/選択/検索/高速描画は未実装
- macOS Editor 以外は PTY 無効
