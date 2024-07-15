using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
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

            // 多语言 获取电脑语言
            ChangeLanguage(CultureInfo.InstalledUICulture.Name);

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
                    // 去除末尾的 "/" 也许 URL protocol 以网址调用时被添加的
                    if (parameters.EndsWith("/"))
                    {
                        parameters = parameters.Substring(0, parameters.Length - 1);
                    }
                    // 存在测试参数
                    int test = parameters.IndexOf("--cat-catch-test");
                    if (test != -1)
                    {
                        parameters = parameters.Remove(test, "--cat-catch-test".Length);
                        if (MessageBox.Show($"\"{program}\" {parameters}", "Continue?", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.No)
                        {
                            Shutdown();
                            return;
                        }
                    }
                    // 展开环境变量
                    parameters = Environment.ExpandEnvironmentVariables(parameters);
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

        public void ChangeLanguage(string language)
        {
            ResourceDictionary dict = new ResourceDictionary();
            switch (language)
            {
                case "zh-CN":
                    dict.Source = new Uri("i18n/zh_CN.xaml", UriKind.Relative);
                    break;
                case "en-US":
                    dict.Source = new Uri("i18n/en_US.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("i18n/en_US.xaml", UriKind.Relative);
                    break;
            }

            // 清除现有资源字典并添加新的资源字典
            this.Resources.MergedDictionaries.Clear();
            this.Resources.MergedDictionaries.Add(dict);
        }
    }
}
