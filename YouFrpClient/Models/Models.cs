namespace FrpClient.Models
{
    /// <summary>
    /// 配置信息
    /// </summary>
    public class Config
    {
        public string DBHost { get; set; } = "23.247.131.63:3306";
        public string DBUser { get; set; } = "frp";
        public string DBPassword { get; set; } = "tgx123456";
        public string DBName { get; set; } = "frp";
        public string FrpcPath { get; set; } = "frpc.exe";
    }

    /// <summary>
    /// 用户信息
    /// </summary>
    public class User
    {
        public int ID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// 节点信息
    /// </summary>
    public class Node
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Hostname { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Name} ({IP})";
        }
    }

    /// <summary>
    /// 隧道信息
    /// </summary>
    public class Proxy
    {
        public int ID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string ProxyName { get; set; } = string.Empty;
        public string ProxyType { get; set; } = string.Empty;
        public string LocalIP { get; set; } = string.Empty;
        public int LocalPort { get; set; }
        public string UseEncryption { get; set; } = string.Empty;
        public string UseCompression { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Locations { get; set; } = string.Empty;
        public string HostHeaderRewrite { get; set; } = string.Empty;
        public string RemotePort { get; set; } = string.Empty;
        public string Sk { get; set; } = string.Empty;
        public string HeaderXFromWhere { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string LastUpdate { get; set; } = string.Empty;
        public int Node { get; set; }
        public string CustomDomains { get; set; } = string.Empty;

        /// <summary>
        /// 获取显示文本
        /// </summary>
        public string DisplayText => $"{ProxyName} ({ProxyType})";

        /// <summary>
        /// 获取映射信息
        /// </summary>
        public string MappingInfo
        {
            get
            {
                if (ProxyType.ToLower() == "tcp" || ProxyType.ToLower() == "udp")
                {
                    return $"{LocalIP}:{LocalPort} -> {RemotePort}";
                }
                else if (ProxyType.ToLower() == "http" || ProxyType.ToLower() == "https")
                {
                    var domain = !string.IsNullOrEmpty(CustomDomains) ? CustomDomains : Domain;
                    return $"{LocalIP}:{LocalPort} -> {domain}";
                }
                return $"{LocalIP}:{LocalPort}";
            }
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled => Status == "0";
    }
}
