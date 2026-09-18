# Yuki PC (Windows)

WinForms-клиент удалённого управления для Windows, который подключается к `yuki-core` и
выполняет команды, присылаемые сервером (или другими устройствами) - блокировка экрана, открытие
URL, регулировка громкости и так далее. Работает в системном трее; закрытие окна скрывает
приложение, а не завершает его работу.

## Требования

.NET 9 (`net9.0-windows`), Windows. Откройте `Yuki-PC.sln` в Visual Studio (или выполните
`dotnet build`) и запустите.

## Конфигурация

Всё настраивается прямо в графическом интерфейсе и сохраняется в `settings.json` рядом с
исполняемым файлом: адрес сервера, идентификатор устройства, токен аутентификации, какие из 14
команд разрешены (по умолчанию - все, кроме `shutdown`/`restart`/`sleep`), выбранный substatus, а
также положение и тема окна. Токен аутентификации шифруется при хранении с помощью Windows DPAPI
(`System.Security.Cryptography.ProtectedData`, область действия - текущий пользователь) перед
записью в `settings.json` - расшифровывается обратно в открытый вид только в памяти.

- **Server Connection**: введите `ws://host:port` или `wss://host:port` (TLS работает «из
  коробки», если он включён на стороне `yuki-core` - см. README [`yuki-core`](../yuki-core)) и
  нажмите Connect.
- **Open Control Panel** открывает `yuki-webui` в браузере, адрес для которого вычисляется из
  поля адреса (`ws→http`/`wss→https`, порт `8000→5000`).
- **Capabilities**: снимите флажок с команд, которые этот ПК не должен принимать удалённо, - тем
  самым вы формируете список разрешённых команд, который приложение проверяет локально в
  дополнение к тому, что разрешает сам `yuki-core`.

## Команды

`open_browser`/`open_url` (принимает только URL со схемой `http`/`https`), `shutdown`, `restart`,
`sleep`, `lock`, `volume_up`/`volume_down`/`volume_mute`/`volume_unmute`/`set_volume` (Core
Audio), `open_folder`/`open_explorer` (только существующие проверенные каталоги), `open_notepad`,
`open_calculator`.

## Протокол

Использует Yuki Protocol `yuki/1.0` через встроенную копию в `libs/yuki-protocol/csharp/` - см.
[`yuki-protocol`](../yuki-protocol). Аналог для Linux с тем же набором возможностей находится в
[`yuki-device-pc-linux`](../yuki-device-pc-linux).
