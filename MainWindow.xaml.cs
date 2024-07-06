using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
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
        private void CheckURLProtocols()
        {
            string appPath = Process.GetCurrentProcess().MainModule.FileName;
            string protocolsPath = @"Software\Classes";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(protocolsPath))
            {
                if (key == null)
                {
                    MessageBox.Show("无法创建注册表项。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                string[] protocolNames = key.GetSubKeyNames();
                foreach (string protocolName in protocolNames)
                {
                    using (RegistryKey commandKey = key.OpenSubKey($@"{protocolName}\shell\open\command"))
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

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (TargetProgram.Text == "")
            {
                MessageBox.Show("别闹了!", "检查结果", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // 当前程序绝对路径
            string appPath = Process.GetCurrentProcess().MainModule.FileName;

            // 创建或打开注册表项
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ProtocolName.Text}"))
            {
                if (key == null)
                {
                    MessageBox.Show("无法创建注册表项。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                key.SetValue("", $"URL:{ProtocolName.Text} Protocol");
                key.SetValue("URL Protocol", "");

                // 创建 shell\open\command 子项
                using (RegistryKey commandKey = key.CreateSubKey(@"shell\open\command"))
                {
                    if (commandKey != null)
                    {
                        commandKey.SetValue("", $"\"{appPath}\" \"%1\"");
                        commandKey.SetValue("Target", TargetProgram.Text);
                    }
                }
                Cancel.IsEnabled = true;
                OK.IsEnabled = false;
                MessageBox.Show("URL 协议注册成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CheckProtocolName_Click(object sender, RoutedEventArgs e)
        {
            if (ProtocolName.Text == "")
            {
                MessageBox.Show("协议名不能为空!", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (RegistryHelper.CheckProtocolName(ProtocolName.Text))
            {
                MessageBox.Show("含有非法字符! 不允许包含 \"://\"", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (RegistryHelper.IsProtocolRegistered(ProtocolName.Text))
            {
                Cancel.IsEnabled = true;
                MessageBox.Show($"协议 '{ProtocolName.Text}' 已经被注册成 URL Protocol。", "检查结果", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else
            {
                OpenFile.IsEnabled = true;
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "选择文件",
                Filter = "所有文件 (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;
                TargetProgram.Text = selectedFilePath;
                OK.IsEnabled = true;
            }

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Classes", writable: true))
            {
                if (key == null)
                {
                    MessageBox.Show("无法创建注册表项。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // 检查协议是否存在
                if (key.OpenSubKey(ProtocolName.Text) != null)
                {
                    key.DeleteSubKeyTree(ProtocolName.Text);
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