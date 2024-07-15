# 使用方法
Windows 10 1709 或以系统下 需要 [.NET Framework 4.7.2](https://go.microsoft.com/fwlink/?LinkId=863262)

`URLProtocol.exe` 程序本身 注册后就不能更改文件位置 否则无法成功调用。

- `URLProtocol.exe` 复制到你想要调用的程序目录
- 打开`URLProtocol.exe` 填写 协议名(不包含 "://")
- 点击 选择目标程序 选择你要调用的exe文件
- 点击 添加/更新

测试是否添加完毕
- 浏览器地址栏尝试打开你自定义的协议加上 `://` (例如 test://)
- 是否正常打开目标程序

可以在 [cat-catch](https://github.com/xifangczy/cat-catch) 扩展里设置调用程序

# Usage
Windows 10 1709 or later requires [.NET Framework 4.7.2](https://go.microsoft.com/fwlink/?LinkId=863262).

Once the `URLProtocol.exe` program itself is registered, the file location cannot be changed, otherwise it will not be successfully called.

- Copy `URLProtocol.exe` to the directory of the program you want to call.
- Open `URLProtocol.exe` and fill in the protocol name (excluding "://").
- Click Select Target Program and choose the exe file you want to call.
- Click Add/Update.

Test whether the addition is successful:
- Try opening your custom protocol with `://` in the browser address bar (e.g., test://).
- Check if the target program opens normally.

You can set up the calling program in the [cat-catch](https://github.com/xifangczy/cat-catch) extension.