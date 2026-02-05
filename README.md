# Unity Terminal (Editor MVP)

Unity エディタ内で macOS PTY を使ったターミナル表示を行う MVP 実装です。

## 追加された構成

- `Native/macOS/pty_bridge.c`: PTY + `/bin/zsh -l` 起動ブリッジ
- `Native/macOS/build.sh`: `libpty_bridge.dylib` ビルドスクリプト
- `Runtime/*`: PTY セッション、ANSI 最小パーサ、スクリーンバッファ、UIElements TerminalView
- `Editor/TerminalWindow.cs`: `Tools > Unity Terminal > Open Terminal` の EditorWindow

## セットアップ（macOS）

1. `Native/macOS/build.sh` を実行して `Plugins/macOS/libpty_bridge.dylib` を生成
2. Unity でパッケージを読み込む
3. `Tools > Unity Terminal > Open Terminal` を開く

## 現在の対応範囲

- PTY spawn / read / write / resize / kill / close
- 矢印キー、Enter、Space、Backspace、Ctrl+C/D/Z
- ANSI/VT 最小対応（カーソル移動、行/画面消去、スクロール、SGRは受理のみ）
- リサイズ時の `pty_resize` + `SIGWINCH`

## 既知の制約（MVP）

- 色の描画（SGR）はまだ UI 反映なし
- コピー/選択/検索/高速描画は未実装
- macOS Editor 以外は PTY 無効
