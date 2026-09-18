# Yuki PC (Windows)

Windows 向けのリモート制御クライアント（WinForms 製）です。`yuki-core` に接続し、サーバー（または他のデバイス）から送られてきたコマンドを実行します - 画面のロック、URL を開く、音量調整など。システムトレイで動作し、ウィンドウを閉じても終了せず、非表示になるだけです。

## 必要要件

.NET 9（`net9.0-windows`）、Windows。`Yuki-PC.sln` を Visual Studio で開く（または `dotnet build`）、実行してください。

## 設定

すべて GUI 自体から設定され、実行ファイルと同じ場所の `settings.json` に保存されます。サーバーアドレス、デバイス ID、認証トークン、14個の許可コマンドのうちどれを有効にするか（デフォルトでは `shutdown`/`restart`/`sleep` を除くすべて）、選択されたサブステータス、ウィンドウの位置/テーマ。認証トークンは、`settings.json` に書き込まれる前に Windows DPAPI（`System.Security.Cryptography.ProtectedData`、ユーザー単位のスコープ）で保存時に暗号化されます - メモリ上でのみ平文に復号されます。

- **サーバー接続**: `ws://host:port` または `wss://host:port` のいずれかを入力し（`yuki-core` 側で TLS が有効になっていればそのまま動作します - [`yuki-core`](../yuki-core) の README を参照）、「接続」を押します。
- **コントロールパネルを開く** は、アドレス欄から導出したアドレスでブラウザに `yuki-webui` を開きます（`ws→http`/`wss→https`、ポート `8000→5000`）。
- **許可コマンド**: この PC にリモートから受け付けさせたくない項目のチェックを外してください - このアプリは `yuki-core` が許可する内容とは別に、これをローカルで強制します。

## コマンド

`open_browser`/`open_url`（`http`/`https` の URL のみ受け付け）、`shutdown`、`restart`、`sleep`、`lock`、`volume_up`/`volume_down`/`volume_mute`/`volume_unmute`/`set_volume`（Core Audio）、`open_folder`/`open_explorer`（実在するディレクトリのみ検証の上で受け付け）、`open_notepad`、`open_calculator`。

## プロトコル

`libs/yuki-protocol/csharp/` に同梱されたコピーを通じて Yuki Protocol `yuki/1.0` を使用します - 詳しくは [`yuki-protocol`](../yuki-protocol) を参照してください。同じ機能セットを持つ Linux 版は [`yuki-device-pc-linux`](../yuki-device-pc-linux) にあります。
