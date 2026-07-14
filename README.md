# 自定义协议 URL Protocol ?

[English](README.en.md) · [Español](README.es.md)

系统允许开发者自定义 URL 协议，以便在特定应用程序或环境中启动自己。一般格式为 `{协议名}:{启动参数}`

例如:

- PotPlayer `potplayer:`
- 微信 `weixin:`
- Micerosoft VS Code `vscode:`

等等，在浏览器打开以上协议，就能启动他们。

但也有一些程序为了某些安全原因，没有开发自定义 URL 协议，例如 VLC 播放器。这时候就需要本项目。

本项目原理为中转，本程序能注册为任意自定义协议，通过协议打开本程序，本程序查表找到真正需要打开的程序，传递参数并启动它。

# 重要说明

- `URLProtocol.exe` 需要固定在一个位置，不能删除或更改，因为协议中转功能依赖该程序的位置
- 如果移动或删除该程序，已注册的自定义协议将无法正常工作

# 系统要求

Windows 10 1709 或以系统下 需要 [.NET Framework 4.7.2](https://go.microsoft.com/fwlink/?LinkId=863262)

# 使用方法

- `URLProtocol.exe` 放入一个固定位，调用协议需要它
- 打开 `URLProtocol.exe` 填写 协议名(不包含 ":")
- 点击 选择目标程序 选择你要调用的 exe 文件
- 点击 添加/更新

测试是否添加完毕

- 浏览器地址栏尝试打开你自定义的协议加上 `:` (例如 test:)
- 是否正常打开目标程序

可以在 [cat-catch](https://github.com/xifangczy/cat-catch) 扩展里设置调用程序

# 测试

在调用协议时传入 `--cat-catch-test` 打开协议时会提示最终调用的字符串, 以及选择是否继续调用.
