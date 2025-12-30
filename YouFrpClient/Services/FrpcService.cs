using FrpClient.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace FrpClient.Services
{
    /// <summary>
    /// Frpc服务管理
    /// </summary>
    public class FrpcService
    {
        private Process? _frpcProcess;
        private readonly Config _config;

        public FrpcService(Config config)
        {
            _config = config;
        }

        /// <summary>
        /// 生成frpc配置文件 (TOML格式 for frp 0.60.0+)
        /// </summary>
        public bool GenerateConfig(Node node, List<Proxy> proxies, string userToken, string configPath = "frpc.toml")
        {
            try
            {
                var builder = new StringBuilder();

                // 生成common配置
                builder.AppendLine($"serverAddr = \"{node.IP}\"");
                builder.AppendLine($"serverPort = {node.Port}");
                builder.AppendLine("transport.tcpMux = true");
                builder.AppendLine("transport.protocol = \"tcp\"");
                builder.AppendLine("auth.method = \"token\"");
                builder.AppendLine($"auth.token = \"{node.Token}\"");
                builder.AppendLine($"user = \"{userToken}\"");
                builder.AppendLine("dnsServer = \"114.114.114.114\"");
                builder.AppendLine();

                // 生成每个代理的配置
                foreach (var proxy in proxies.Where(p => p.Status == "0"))
                {
                    builder.AppendLine("[[proxies]]");
                    builder.AppendLine($"name = \"{proxy.ProxyName}\"");
                    builder.AppendLine($"type = \"{proxy.ProxyType}\"");
                    builder.AppendLine($"localIP = \"{proxy.LocalIP}\"");
                    builder.AppendLine($"localPort = {proxy.LocalPort}");

                    // 根据不同的隧道类型处理特殊字段
                    switch (proxy.ProxyType.ToLower())
                    {
                        case "http":
                        case "https":
                            // 处理域名
                            if (!string.IsNullOrEmpty(proxy.CustomDomains))
                            {
                                builder.AppendLine($"customDomains = [\"{proxy.CustomDomains}\"]");
                            }
                            else if (!string.IsNullOrEmpty(proxy.Domain) && proxy.Domain != "[]")
                            {
                                var domain = ParseDomain(proxy.Domain);
                                if (!string.IsNullOrEmpty(domain))
                                {
                                    builder.AppendLine($"customDomains = [\"{domain}\"]");
                                }
                            }

                            if (!string.IsNullOrEmpty(proxy.HostHeaderRewrite))
                            {
                                builder.AppendLine($"hostHeaderRewrite = \"{proxy.HostHeaderRewrite}\"");
                            }

                            if (!string.IsNullOrEmpty(proxy.Locations))
                            {
                                builder.AppendLine($"locations = \"{proxy.Locations}\"");
                            }
                            break;

                        case "stcp":
                        case "xtcp":
                            if (!string.IsNullOrEmpty(proxy.RemotePort))
                            {
                                builder.AppendLine($"remotePort = {proxy.RemotePort}");
                            }

                            var sk = !string.IsNullOrEmpty(proxy.Sk) ? proxy.Sk : "tgx123456";
                            builder.AppendLine($"transport.authKey = \"{sk}\"");
                            break;

                        default:
                            if (!string.IsNullOrEmpty(proxy.RemotePort))
                            {
                                builder.AppendLine($"remotePort = {proxy.RemotePort}");
                            }
                            break;
                    }

                    // 加密和压缩设置
                    var useEncryption = proxy.UseEncryption == "1" || 
                                       proxy.UseEncryption.ToLower() == "true" || 
                                       proxy.UseEncryption.ToLower() == "on";
                    builder.AppendLine($"transport.useEncryption = {useEncryption.ToString().ToLower()}");

                    var useCompression = proxy.UseCompression == "1" || 
                                        proxy.UseCompression.ToLower() == "true" || 
                                        proxy.UseCompression.ToLower() == "on";
                    builder.AppendLine($"transport.useCompression = {useCompression.ToString().ToLower()}");
                    builder.AppendLine();
                }

                // 写入配置文件（使用LF换行符）
                var content = builder.ToString().Replace("\r\n", "\n");
                var utf8NoBom = new UTF8Encoding(false);
                File.WriteAllText(configPath, content, utf8NoBom);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"生成配置文件失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 解析域名字段
        /// </summary>
        private string ParseDomain(string domainField)
        {
            if (string.IsNullOrEmpty(domainField)) return string.Empty;

            // 如果是JSON数组格式
            if (domainField.StartsWith("[") && domainField.EndsWith("]"))
            {
                var content = domainField.Trim('[', ']');
                var domains = content.Split(',');
                if (domains.Length > 0)
                {
                    return domains[0].Trim('"', '\'', ' ');
                }
            }

            return domainField;
        }

        /// <summary>
        /// 启动frpc进程
        /// </summary>
        public bool StartFrpc(string configPath = "frpc.toml")
        {
            try
            {
                StopFrpc();

                if (!File.Exists(_config.FrpcPath))
                {
                    throw new FileNotFoundException($"找不到frpc可执行文件: {_config.FrpcPath}");
                }

                if (!File.Exists(configPath))
                {
                    throw new FileNotFoundException($"找不到配置文件: {configPath}");
                }

                _frpcProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _config.FrpcPath,
                        Arguments = $"-c \"{configPath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(_config.FrpcPath) ?? Environment.CurrentDirectory,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    }
                };

                _frpcProcess.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        OnOutputReceived?.Invoke(args.Data);
                    }
                };

                _frpcProcess.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        OnErrorReceived?.Invoke(args.Data);
                    }
                };

                _frpcProcess.Start();
                _frpcProcess.BeginOutputReadLine();
                _frpcProcess.BeginErrorReadLine();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"启动frpc失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 停止frpc进程
        /// </summary>
        public void StopFrpc()
        {
            try
            {
                if (_frpcProcess != null && !_frpcProcess.HasExited)
                {
                    _frpcProcess.Kill();
                    _frpcProcess.WaitForExit(3000);
                    _frpcProcess.Dispose();
                    _frpcProcess = null;
                }
            }
            catch (Exception)
            {
                // 忽略错误
            }
        }

        /// <summary>
        /// 获取frpc运行状态
        /// </summary>
        public bool IsRunning => _frpcProcess != null && !_frpcProcess.HasExited;

        /// <summary>
        /// 输出接收事件
        /// </summary>
        public event Action<string>? OnOutputReceived;

        /// <summary>
        /// 错误接收事件
        /// </summary>
        public event Action<string>? OnErrorReceived;
    }
}
