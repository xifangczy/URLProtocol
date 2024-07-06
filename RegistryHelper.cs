using Microsoft.Win32;
using System;

namespace URLProtocol.Helpers
{
    public static class RegistryHelper
    {
        public static bool IsProtocolRegistered(string protocol)
        {
            try
            {
                using (RegistryKey CurrentUserKey = Registry.ClassesRoot.OpenSubKey(protocol))
                {
                    if (CurrentUserKey != null)
                    {
                        object urlProtocol = CurrentUserKey.GetValue("URL Protocol");
                        return urlProtocol != null;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                // 如果发生错误，可以在这里处理
                // 例如，记录日志或抛出异常
                throw new InvalidOperationException($"检查协议时发生错误: {ex.Message}", ex);
            }
        }
        public static string ExtractPathFromCommand(string command)
        {
            // 移除引号和参数，只保留路径
            string firstPart = command.Split(' ')[0];
            if (firstPart.StartsWith("\""))
            {
                return firstPart.Trim('\"');
            }
            return firstPart;

        }
        public static (string, string) ExtractProtocolAndRemove(string url)
        {
            // 查找第一个 "://" 的位置
            int protocolEndIndex = url.IndexOf("://");
            if (protocolEndIndex != -1)
            {
                // 提取协议名
                string protocol = url.Substring(0, protocolEndIndex);
                // 去除协议名和 "://"
                string urlWithoutProtocol = url.Substring(protocolEndIndex + 3);
                return (protocol, urlWithoutProtocol);
            }
            return (null, url); // 如果没有找到 "://"，返回原始字符串和 null 协议名
        }

        public static bool CheckProtocolName(string ProtocolName)
        {
            char[] invalidChars = { ':', '/', '\\', '.' };
            foreach (char c in invalidChars)
            {
                if (ProtocolName.IndexOf(c) >= 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
