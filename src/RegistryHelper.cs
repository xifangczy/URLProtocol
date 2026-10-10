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
        // 解析命令行字符串，返回 (可执行程序路径, 附加参数)
        public static (string, string) ParseProgramAndArgs(string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine))
            {
                return (commandLine, string.Empty);
            }

            commandLine = commandLine.Trim();

            string program;
            string args = string.Empty;

            if (commandLine.StartsWith("\""))
            {
                int endQuote = commandLine.IndexOf('"', 1);
                if (endQuote == -1)
                {
                    program = commandLine.Substring(1);
                }
                else
                {
                    program = commandLine.Substring(1, endQuote - 1);
                    args = commandLine.Substring(endQuote + 1).Trim();
                }
            }
            else
            {
                int firstSpace = commandLine.IndexOf(' ');
                if (firstSpace == -1)
                {
                    program = commandLine;
                }
                else
                {
                    program = commandLine.Substring(0, firstSpace);
                    args = commandLine.Substring(firstSpace + 1).Trim();
                }
            }

            return (program, args);
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
