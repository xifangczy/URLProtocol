using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Web;
using System.Windows;
using URLProtocol.Helpers;

namespace URLProtocol
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 检查命令行参数
            if (e.Args.Length > 0)
            {
                string decodedUrl = HttpUtility.UrlDecode(e.Args[0]);

                // 解析出协议名 和 参数
                (string protocol, string parameters) = RegistryHelper.ExtractProtocolAndRemove(decodedUrl);

                // 根据协议名查找到 目标程序 加上参数运行
                string program = (string)Registry.GetValue($@"HKEY_CLASSES_ROOT\{protocol}\shell\open\command", "Target", null);
                if (program != null)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = program, // 程序
                        Arguments = parameters, // 参数
                        WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory    // 调整工作目录
                    };
                    Process.Start(startInfo);
                }
                // 关闭本应用程序
                Shutdown();
            }
            else
            {
                // 没有参数，启动主窗口
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }
    }
}
