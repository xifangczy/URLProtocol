# Custom URL Protocol ?

[中文](README.md) · [Español](README.es.md)

The system allows developers to create custom URL protocols to launch specific applications or environments. The general format is `{protocol_name}:{launch_parameters}`.

For example:

- PotPlayer: `potplayer:`
- WeChat: `weixin:`
- Microsoft VS Code: `vscode:`

And so on. Opening these protocols in a browser will launch the corresponding applications.

However, some programs do not provide custom URL protocols for security reasons, such as VLC Player. In these cases, this project becomes necessary.

The principle of this project is to act as an intermediary. It registers itself as any protocol name, then selects the target application. When a protocol is opened, it looks for the corresponding target application, processes and corrects the parameters, passes the parameters to the target application, and then launches it.

# Important Notes

- `URLProtocol.exe` must be kept in a fixed location and should not be deleted or moved, as the protocol forwarding functionality depends on the program's location
- If the program is moved or deleted, the registered custom protocols will not work properly

# System requirements

Windows 10 1709 or later requires [.NET Framework 4.7.2](https://go.microsoft.com/fwlink/?LinkId=863262).

# Usage

- Open `URLProtocol.exe` and fill in the protocol name (excluding ":").
- Click Select Target Program and choose the exe file you want to call.
- Click Add/Update.

Test whether the addition is successful:

- Try opening your custom protocol with `:` in the browser address bar (e.g., test:).
- Check if the target program opens normally.

You can set up the calling program in the [cat-catch](https://github.com/xifangczy/cat-catch) extension.

# Test

When calling the protocol, pass in `--cat-catch-test` to display the final call string and choose whether to continue the call.
