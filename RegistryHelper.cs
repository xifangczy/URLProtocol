namespace URLProtocol.Helpers
{
    public static class RegistryHelper
    {
        // 提取协议名和去除协议名后的 URL
        public static (string, string) ExtractProtocolAndRemove(string url)
        {
            int protocolEndIndex = url.IndexOf(":");
            if (protocolEndIndex != -1)
            {
                string protocol = url.Substring(0, protocolEndIndex);
                string parameters = url.Substring(protocolEndIndex + 1);
                if (parameters.StartsWith("//"))
                {
                    parameters = parameters.Substring(2);
                }
                return (protocol, parameters);
            }
            return (null, url);
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
