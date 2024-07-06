using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using URLProtocol.Helpers;

namespace URLProtocol
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CheckURLProtocols();
        }

        string appPath; // 本程序绝对路径

        private void CheckURLProtocols()
        {
            appPath = Process.GetCurrentProcess().MainModule.FileName;
            using (RegistryKey CurrentUserKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes"))
            {
                if (CurrentUserKey == null)
                {
                    MessageBox.Show("无法打开注册表项。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                string[] protocolNames = CurrentUserKey.GetSubKeyNames();
                foreach (string protocolName in protocolNames)
                {
                    if (protocolName.Contains("."))
                    {
                        continue;
                    }
                    using (RegistryKey protocolKey = CurrentUserKey.OpenSubKey(protocolName))
                    {
                        if (protocolKey == null || protocolKey.GetValue("URL Protocol") == null)
                        {
                            continue;
                        }
                        using (RegistryKey commandKey = protocolKey.OpenSubKey(@"shell\open\command"))
                        {
                            if (commandKey == null)
                            {
                                continue;
                            }
                            string commandValue = commandKey.GetValue("") as string;
                            if (string.IsNullOrEmpty(commandValue))
                            {
                                continue;
                            }
                            string commandPath = RegistryHelper.ExtractPathFromCommand(commandValue);
                            if (string.Equals(commandPath, appPath, StringComparison.OrdinalIgnoreCase))
                            {
                                ProtocolName.Text = protocolName;
                                TargetProgram.Text = commandKey.GetValue("Target") as string;
                                Cancel.IsEnabled = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            // 验证协议名
            if (ProtocolName.Text.EndsWith("://"))
            {
                ProtocolName.Text = ProtocolName.Text.Substring(0, ProtocolName.Text.Length - 3);
            }
            if (ProtocolName.Text == "")
            {
                MessageBox.Show("协议名不能为空!", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (RegistryHelper.CheckProtocolName(ProtocolName.Text))
            {
                MessageBox.Show("含有非法字符! 不允许包含 \" / . \\ :\"", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (RegistryHelper.IsProtocolRegistered(ProtocolName.Text))
            {
                MessageBox.Show($"'{ProtocolName.Text}' 已被占用。", "检查结果", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (TargetProgram.Text == "")
            {
                MessageBox.Show("目标程序为空!", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 创建或打开注册表项
            using (RegistryKey CurrentUserKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ProtocolName.Text}"))
            {
                if (CurrentUserKey == null)
                {
                    MessageBox.Show("无法创建注册表项。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                CurrentUserKey.SetValue("", $"URL:{ProtocolName.Text} Protocol");
                CurrentUserKey.SetValue("URL Protocol", "");

                // 创建 shell\open\command 子项
                using (RegistryKey commandKey = CurrentUserKey.CreateSubKey(@"shell\open\command"))
                {
                    if (commandKey != null)
                    {
                        commandKey.SetValue("", $"\"{appPath}\" \"%1\"");
                        commandKey.SetValue("Target", TargetProgram.Text);
                    }
                }
                Cancel.IsEnabled = true;
                MessageBox.Show("URL 协议添加成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "选择目标可执行程序",
                Filter = "所有文件 (*.*)|*.*",
                InitialDirectory = Directory.GetCurrentDirectory()
            };
            if (openFileDialog.ShowDialog() == true)
            {
                if(openFileDialog.FileName == appPath)
                {
                    MessageBox.Show("不能选择自己", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                TargetProgram.Text = openFileDialog.FileName;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            using (RegistryKey CurrentUserKey = Registry.CurrentUser.OpenSubKey("Software\\Classes", writable: true))
            {
                if (CurrentUserKey == null)
                {
                    MessageBox.Show("无法打开注册表项。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // 检查协议是否存在
                if (CurrentUserKey.OpenSubKey(ProtocolName.Text) != null)
                {
                    CurrentUserKey.DeleteSubKeyTree(ProtocolName.Text);
                    ProtocolName.Text = "";
                    TargetProgram.Text = "";
                    MessageBox.Show("URL 协议删除成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("协议不存在。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Cancel.IsEnabled = false;
            }
        }
    }
}