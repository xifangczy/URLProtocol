using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using URLProtocol.Helpers;

namespace URLProtocol
{
    public class Protocols
    {
        public string Value { get; set; }
        public string Target { get; set; }
        public Protocols(string value, string target = null)
        {
            Value = value;
            Target = target;
        }
    }

    public partial class MainWindow : Window
    {
        private bool isUpdatingProtocol = false;    // 防止下拉菜单触发 ProtocolName_TextChanged
        private string appPath; // 本程序绝对路径
        private static Dictionary<string, Protocols> ProtocolsDict; // 储存所有已注册的协议

        public MainWindow()
        {
            InitializeComponent();
            CheckURLProtocols();
        }

        private void CheckURLProtocols()
        {
            ProtocolsDict = new Dictionary<string, Protocols> { { "所有已注册协议...", new Protocols("") } };  // 设置下拉菜单提示
            appPath = Process.GetCurrentProcess().MainModule.FileName;  // 获取本程序绝对路径

            // 打开HKEY_CLASSES_ROOT
            using (RegistryKey ClassesRootKey = Registry.ClassesRoot)
            {
                if (ClassesRootKey == null)
                {

                    MessageBox.Show("无法打开注册表项。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                string[] protocolNames = ClassesRootKey.GetSubKeyNames();
                foreach (string protocolName in protocolNames)
                {
                    if (protocolName.Contains("."))
                    {
                        continue;
                    }
                    using (RegistryKey protocolKey = ClassesRootKey.OpenSubKey(protocolName))
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
                            ProtocolsDict[protocolName] = new Protocols(commandValue, commandKey.GetValue("Target") as string);
                        }
                    }
                }

                AllProtocol.ItemsSource = ProtocolsDict.Keys.ToList();  // 绑定下拉菜单
                AllProtocol.SelectedIndex = 0;  // 默认选中提示
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
            if (TargetProgram.Text == "")
            {
                MessageBox.Show("目标程序为空!", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (ProtocolsDict.ContainsKey(ProtocolName.Text))
            {
                if (MessageBox.Show($"确认更改？", "确认更新", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return;
                }
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
                        string value = $"\"{appPath}\" \"%1\"";
                        commandKey.SetValue("", value);
                        commandKey.SetValue("Target", TargetProgram.Text);
                        ProtocolsDict[ProtocolName.Text] = new Protocols(value, commandKey.GetValue("Target") as string); // 添加到字典
                        AllProtocol.ItemsSource = ProtocolsDict.Keys.ToList();  // 刷新下拉菜单
                        Tips.Text = $"调用参数: {value}";
                        MessageBox.Show("URL 协议添加成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("无法创建注册表项。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
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
                if (openFileDialog.FileName == appPath)
                {
                    MessageBox.Show("不能选择自己", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                TargetProgram.Text = openFileDialog.FileName;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (ProtocolName.Text == "")
            {
                MessageBox.Show("协议名不能为空!", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            using (RegistryKey ClassesRootKey = Registry.ClassesRoot)
            {
                if (ClassesRootKey == null)
                {
                    MessageBox.Show("无法打开注册表项。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (ClassesRootKey.OpenSubKey(ProtocolName.Text) == null)
                {
                    MessageBox.Show("协议不存在。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (MessageBox.Show("确认删除?", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    ClassesRootKey.DeleteSubKeyTree(ProtocolName.Text);
                    ProtocolsDict.Remove(ProtocolName.Text);    // 从字典里删除
                    AllProtocol.ItemsSource = ProtocolsDict.Keys.ToList();  //刷新下拉菜单
                    ProtocolName.Text = "";
                    TargetProgram.Text = "";
                    Tips.Text = "";
                    //MessageBox.Show("URL 协议删除成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void AllProtocol_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (isUpdatingProtocol)
            {
                return;
            }
            if (AllProtocol.SelectedIndex <= 0)
            {
                ProtocolName.Text = "";
                TargetProgram.Text = "";
                Tips.Text = "";
                return;
            }
            isUpdatingProtocol = true;  // 防止触发 ProtocolName_TextChanged
            ProtocolName.Text = AllProtocol.SelectedItem as string; // 填写ProtocolName
            isUpdatingProtocol = false;
            TargetProgram.Text = ProtocolsDict[AllProtocol.SelectedItem.ToString()].Target; // 从字典里查找 填写TargetProgram
            Tips.Text = "调用参数: " + ProtocolsDict[AllProtocol.SelectedItem.ToString()].Value; // 提示协议执行程序 字典里查找 填写Tips
        }

        private void ProtocolName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isUpdatingProtocol)
            {
                return;
            }
            // 查找ProtocolsDict 中是否存在 ProtocolName.Text
            if (ProtocolsDict.ContainsKey(ProtocolName.Text))
            {
                AllProtocol.SelectedItem = ProtocolName.Text;
                TargetProgram.Text = ProtocolsDict[AllProtocol.SelectedItem.ToString()].Target;
                Tips.Text = "调用参数: " + ProtocolsDict[AllProtocol.SelectedItem.ToString()].Value;
                return;
            }
            Tips.Text = "";
            TargetProgram.Text = "";
            isUpdatingProtocol = true;
            AllProtocol.SelectedIndex = 0;
            isUpdatingProtocol = false;
            return;
        }
        private void Logo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("喵~", "噗噗", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}