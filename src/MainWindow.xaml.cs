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
        private static bool ShowAll = false;    // 默认不显示所有协议

        public MainWindow()
        {
            InitializeComponent();
            CheckURLProtocols();
        }

        private void CheckURLProtocols()
        {
            ProtocolsDict = new Dictionary<string, Protocols> { { I18n("AllProtocols"), new Protocols("") } };  // 设置下拉菜单提示
            appPath = Process.GetCurrentProcess().MainModule.FileName;  // 获取本程序绝对路径

            // 打开HKEY_CLASSES_ROOT
            using (RegistryKey ClassesRootKey = Registry.ClassesRoot)
            {
                if (ClassesRootKey == null)
                {

                    MessageBox.Show(I18n("UnableOpeningRegistry"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                            string Target = commandKey.GetValue("Target") as string;
                            //if(!ShowAll && Target == null)
                            //{
                            //    continue;
                            //}
                            ProtocolsDict[protocolName] = new Protocols(commandValue, Target);
                        }
                    }
                }
                RefreshComboBox(0);
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
                MessageBox.Show(I18n("ProtocolNameEmpty"), I18n("Warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (RegistryHelper.CheckProtocolName(ProtocolName.Text))
            {
                MessageBox.Show(I18n("IllegalCharacters") + " \" / . \\ :\"", I18n("Warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (TargetProgram.Text == "")
            {
                MessageBox.Show(I18n("TargetEmpty"), I18n("Warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (ProtocolsDict.ContainsKey(ProtocolName.Text))
            {
                if (MessageBox.Show(I18n("ConfirmChange"), I18n("Information"), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return;
                }
            }
            // 创建或打开注册表项
            using (RegistryKey CurrentUserKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ProtocolName.Text}"))
            {
                if (CurrentUserKey == null)
                {
                    MessageBox.Show(I18n("UnableOpeningRegistry"), I18n("Warning"), MessageBoxButton.OK, MessageBoxImage.Error);
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
                        RefreshComboBox();
                        Tips.Text = I18n("Parameters") + $": {value}";
                        MessageBox.Show(I18n("AddedSuccessfully"), I18n("Information"), MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(I18n("UnableOpeningRegistry"), I18n("Warning"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = I18n("SelectExecutable"),
                Filter = I18n("AllFiles") + " (*.*)|*.*",
                InitialDirectory = Directory.GetCurrentDirectory()
            };
            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileName == appPath)
                {
                    MessageBox.Show(I18n("Oneself"), I18n("Warning"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                TargetProgram.Text = openFileDialog.FileName;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (ProtocolName.Text == "")
            {
                MessageBox.Show(I18n("ProtocolNameEmpty"), I18n("Warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            using (RegistryKey ClassesRootKey = Registry.ClassesRoot)
            {
                if (ClassesRootKey == null)
                {
                    MessageBox.Show(I18n("UnableOpeningRegistry"), I18n("Warning"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (ClassesRootKey.OpenSubKey(ProtocolName.Text) == null)
                {
                    MessageBox.Show(I18n("ProtocolNotExist"), I18n("Warning"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (MessageBox.Show(I18n("ConfirmDeletion"), I18n("ConfirmDeletion"), MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    ClassesRootKey.DeleteSubKeyTree(ProtocolName.Text);
                    ProtocolsDict.Remove(ProtocolName.Text);    // 从字典里删除
                    RefreshComboBox();
                    ProtocolName.Text = "";
                    TargetProgram.Text = "";
                    Tips.Text = "";
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
            Tips.Text = I18n("Parameters") + ": " + ProtocolsDict[AllProtocol.SelectedItem.ToString()].Value; // 提示协议执行程序 字典里查找 填写Tips
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
                TargetProgram.Text = ProtocolsDict[ProtocolName.Text].Target;
                Tips.Text = I18n("Parameters") + ": " + ProtocolsDict[ProtocolName.Text].Value;
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

        private void SetLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SetLanguage.SelectedIndex == 0)
            {
                return;
            }
            App app = (App)Application.Current;
            ComboBoxItem selectedItem = SetLanguage.SelectedItem as ComboBoxItem;
            app.ChangeLanguage(selectedItem.Tag.ToString());

            // 切换下拉菜单提示语言
            var firstItem = ProtocolsDict.First();
            Protocols currentProtocols = firstItem.Value;
            ProtocolsDict.Remove(firstItem.Key);
            ProtocolsDict.Add(I18n("AllProtocols"), currentProtocols);
            int index = AllProtocol.SelectedIndex;
            RefreshComboBox(index);
        }

        private string I18n(string key)
        {
            return (string)Application.Current.FindResource(key);
        }

        private void ShowAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ShowAll = !ShowAll;
            CheckURLProtocols();
        }
        private void RefreshComboBox(int index = -1)
        {
            string tips = ProtocolsDict.ElementAt(0).Key;
            AllProtocol.Items.Clear();
            AllProtocol.Items.Add(tips);

            for (int i = 1; i < ProtocolsDict.Count; i++)
            {
                var item = ProtocolsDict.ElementAt(i);
                if (!ShowAll && item.Value.Target == null)
                {
                    continue;
                }
                AllProtocol.Items.Add(item.Key);
            }

            if (index != -1)
            {
                AllProtocol.SelectedIndex = index;
            }
        }
    }
}