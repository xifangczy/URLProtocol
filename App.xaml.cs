using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
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
                // 有参数，弹出参数列表
                //string test = string.Join(" | ", e.Args);
                //MessageBox.Show(test);

                string encodedUrl = e.Args[0];
                string decodedUrl = HttpUtility.UrlDecode(encodedUrl);
                // 查找第一个 "://" 的位置
                int protocolEndIndex = decodedUrl.IndexOf("://");
                (string protocol, string parameters) = RegistryHelper.ExtractProtocolAndRemove(decodedUrl);

                string key = $@"HKEY_CLASSES_ROOT\{protocol}\shell\open\command";
                string program = (string)Registry.GetValue(key, "Target", null);
                if (program != null)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = program; // 程序的名称
                    startInfo.Arguments = parameters;
                    startInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    Process.Start(startInfo);
                    //MessageBox.Show(program);
                }

                //MessageBox.Show(protocol + " | " + parameters);
                // 关闭应用程序
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
