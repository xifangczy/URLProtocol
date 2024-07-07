namespace URLProtocol.Helpers
{
    public static class RegistryHelper
    {
        // 提取协议名和去除协议名后的 URL
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
        // 检查协议名是否包含无效字符
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
