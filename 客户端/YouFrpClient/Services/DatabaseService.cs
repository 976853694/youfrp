using MySql.Data.MySqlClient;
using FrpClient.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FrpClient.Services
{
    /// <summary>
    /// 数据库服务
    /// </summary>
    public class DatabaseService
    {
        private readonly Config _config;
        private string ConnectionString
        {
            get
            {
                // DBHost可能包含端口号，如 "23.247.131.63:3306"
                var hostParts = _config.DBHost.Split(':');
                var server = hostParts[0];
                var port = hostParts.Length > 1 ? hostParts[1] : "3306";
                // 添加AllowPublicKeyRetrieval和SSL设置以兼容MySQL 8.0+的认证方式
                return $"Server={server};Port={port};Database={_config.DBName};Uid={_config.DBUser};Pwd={_config.DBPassword};charset=utf8mb4;AllowPublicKeyRetrieval=True;SslMode=None;";
            }
        }

        public DatabaseService(Config config)
        {
            _config = config;
        }

        /// <summary>
        /// 用户名密码登录
        /// </summary>
        public User? LoginByPassword(string username, string password)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                // 先尝试用户名登录
                var query = "SELECT id, username, password, email, status FROM users WHERE username = @identifier";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@identifier", username);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var storedPassword = reader.GetString(2);
                    var status = reader.GetString(4);
                    
                    // 验证密码（PHP的password_hash使用BCrypt）
                    if (VerifyPassword(password, storedPassword) && status == "0")
                    {
                        return new User
                        {
                            ID = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Status = status
                        };
                    }
                }
                
                reader.Close();
                
                // 尝试邮箱登录
                query = "SELECT id, username, password, email, status FROM users WHERE email = @identifier";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@identifier", username);
                
                using var reader2 = cmd.ExecuteReader();
                if (reader2.Read())
                {
                    var storedPassword = reader2.GetString(2);
                    var status = reader2.GetString(4);
                    
                    if (VerifyPassword(password, storedPassword) && status == "0")
                    {
                        return new User
                        {
                            ID = reader2.GetInt32(0),
                            Username = reader2.GetString(1),
                            Status = status
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"登录失败: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// 验证BCrypt密码
        /// </summary>
        private bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        public bool RegisterUser(string username, string email, string password)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                // 检查用户名是否已存在
                var checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username";
                using var checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@username", username);
                var count = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (count > 0)
                {
                    throw new Exception("用户名已存在");
                }

                // 检查邮箱是否已存在
                checkQuery = "SELECT COUNT(*) FROM users WHERE email = @email";
                checkCmd.Parameters.Clear();
                checkCmd.Parameters.AddWithValue("@email", email);
                count = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (count > 0)
                {
                    throw new Exception("邮箱已被注册");
                }

                // 生成BCrypt密码哈希
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
                var token = GenerateToken(username, password);
                
                using var transaction = conn.BeginTransaction();
                
                try
                {
                    // 插入用户
                    var insertUser = @"INSERT INTO users 
                        (username, password, email, traffic, proxies, `group`, regtime, status) 
                        VALUES (@username, @password, @email, 1024, 5, 'default', @regtime, '0')";
                    
                    using var userCmd = new MySqlCommand(insertUser, conn, transaction);
                    userCmd.Parameters.AddWithValue("@username", username);
                    userCmd.Parameters.AddWithValue("@password", passwordHash);
                    userCmd.Parameters.AddWithValue("@email", email);
                    userCmd.Parameters.AddWithValue("@regtime", DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
                    userCmd.ExecuteNonQuery();

                    // 插入Token
                    var insertToken = "INSERT INTO tokens (username, token, status) VALUES (@username, @token, '0')";
                    using var tokenCmd = new MySqlCommand(insertToken, conn, transaction);
                    tokenCmd.Parameters.AddWithValue("@username", username);
                    tokenCmd.Parameters.AddWithValue("@token", token);
                    tokenCmd.ExecuteNonQuery();

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"注册失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 生成Token
        /// </summary>
        private string GenerateToken(string username, string password)
        {
            var input = $"{username}{password}{DateTime.Now.Ticks}{new Random().Next(0, 9999999)}";
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            var tokenBytes = new byte[8];
            Array.Copy(hash, tokenBytes, 8);
            return BitConverter.ToString(tokenBytes).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 获取用户的Token
        /// </summary>
        public string? GetUserToken(string username)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                var query = "SELECT token FROM tokens WHERE username = @username AND status = '0'";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);

                var result = cmd.ExecuteScalar();
                return result?.ToString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 根据Token获取用户信息
        /// </summary>
        public User? GetUserByToken(string token)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                var query = "SELECT id, username, token, status FROM tokens WHERE token = @token AND status = '0'";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@token", token);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new User
                    {
                        ID = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Token = reader.GetString(2),
                        Status = reader.GetString(3)
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"验证访问密钥失败: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// 获取所有可用节点
        /// </summary>
        public List<Node> GetAllNodes()
        {
            var nodes = new List<Node>();

            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                var query = "SELECT id, name, hostname, ip, port, token, status FROM nodes WHERE status = '200'";
                using var cmd = new MySqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    nodes.Add(new Node
                    {
                        ID = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Hostname = reader.GetString(2),
                        IP = reader.GetString(3),
                        Port = reader.GetInt32(4),
                        Token = reader.GetString(5),
                        Status = reader.GetString(6)
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取节点列表失败: {ex.Message}", ex);
            }

            return nodes;
        }

        /// <summary>
        /// 根据ID获取节点信息
        /// </summary>
        public Node? GetNodeByID(int id)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                var query = "SELECT id, name, hostname, ip, port, token, status FROM nodes WHERE id = @id AND status = '200'";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Node
                    {
                        ID = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Hostname = reader.GetString(2),
                        IP = reader.GetString(3),
                        Port = reader.GetInt32(4),
                        Token = reader.GetString(5),
                        Status = reader.GetString(6)
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取节点信息失败: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// 根据用户名获取隧道列表
        /// </summary>
        public List<Proxy> GetProxiesByUsername(string username)
        {
            var proxies = new List<Proxy>();

            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                var query = @"SELECT p.id, p.username, p.proxy_name, p.proxy_type, p.local_ip, p.local_port, 
                            p.use_encryption, p.use_compression, p.domain, p.locations, p.host_header_rewrite, 
                            p.remote_port, p.sk, p.`header_X-From-Where`, p.status, p.lastupdate, p.node, p.customdomains,
                            n.ip, n.name
                            FROM proxies p
                            LEFT JOIN nodes n ON p.node = n.id
                            WHERE p.username = @username";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    proxies.Add(new Proxy
                    {
                        ID = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        ProxyName = reader.GetString(2),
                        ProxyType = reader.GetString(3),
                        LocalIP = reader.GetString(4),
                        LocalPort = reader.GetInt32(5),
                        UseEncryption = reader.GetString(6),
                        UseCompression = reader.GetString(7),
                        Domain = reader.GetString(8),
                        Locations = reader.GetString(9),
                        HostHeaderRewrite = reader.GetString(10),
                        RemotePort = reader.GetString(11),
                        Sk = reader.GetString(12),
                        HeaderXFromWhere = reader.GetString(13),
                        Status = reader.GetString(14),
                        LastUpdate = reader.GetString(15),
                        Node = reader.GetInt32(16),
                        CustomDomains = reader.GetString(17),
                        NodeIP = reader.IsDBNull(18) ? "" : reader.GetString(18),
                        NodeName = reader.IsDBNull(19) ? "" : reader.GetString(19)
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取隧道列表失败: {ex.Message}", ex);
            }

            return proxies;
        }

        /// <summary>
        /// 切换隧道启用/禁用状态
        /// </summary>
        public bool ToggleProxyStatus(int proxyId)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                // 先获取当前状态
                var query = "SELECT status FROM proxies WHERE id = @id";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", proxyId);
                
                var currentStatus = cmd.ExecuteScalar()?.ToString();
                if (currentStatus == null)
                {
                    return false;
                }

                // 切换状态：0=启用, 1=禁用
                var newStatus = currentStatus == "0" ? "1" : "0";
                
                var updateQuery = "UPDATE proxies SET status = @status WHERE id = @id";
                using var updateCmd = new MySqlCommand(updateQuery, conn);
                updateCmd.Parameters.AddWithValue("@status", newStatus);
                updateCmd.Parameters.AddWithValue("@id", proxyId);
                updateCmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"切换隧道状态失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取用户详细信息（包含流量、遂道数等）
        /// </summary>
        public (long traffic, int proxies, string email, string group, string regtime) GetUserDetails(string username)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                var query = "SELECT traffic, proxies, email, `group`, regtime FROM users WHERE username = @username";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return (
                        reader.GetInt64(0),  // 改为GetInt64
                        reader.GetInt32(1),
                        reader.GetString(2),
                        reader.GetString(3),
                        reader.GetString(4)
                    );
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取用户详细信息失败: {ex.Message}", ex);
            }

            return (0, 0, "", "default", "0");
        }

        /// <summary>
        /// 获取用户的带宽限制（返回inbound和outbound，单位：Mbps）
        /// 优先级：limits表 > groups表 > 默认值(1024 KB/s = 8 Mbps)
        /// 数据库中存储的是KB/s，需要转换为Mbps
        /// </summary>
        public (int inbound, int outbound) GetUserBandwidth(string username)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                // 1. 先查询limits表（用户自定义限制）
                var limitsQuery = "SELECT inbound, outbound FROM limits WHERE username = @username";
                using (var cmd = new MySqlCommand(limitsQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        int inboundKBps = reader.GetInt32(0);
                        int outboundKBps = reader.GetInt32(1);
                        
                        int inboundMbps = (int)Math.Round(inboundKBps / 1024.0 * 8);
                        int outboundMbps = (int)Math.Round(outboundKBps / 1024.0 * 8);
                        
                        return (inboundMbps, outboundMbps);
                    }
                }

                // 2. 如果limits表没有，查询用户所属组
                var userQuery = "SELECT `group` FROM users WHERE username = @username";
                string groupName = "";
                using (var cmd = new MySqlCommand(userQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        groupName = result.ToString();
                    }
                }

                // 3. 根据组名查询groups表
                if (!string.IsNullOrEmpty(groupName))
                {
                    var groupQuery = "SELECT inbound, outbound FROM groups WHERE name = @groupName";
                    using (var cmd = new MySqlCommand(groupQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@groupName", groupName);
                        using var reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            int inboundKBps = reader.GetInt32(0);
                            int outboundKBps = reader.GetInt32(1);
                            
                            int inboundMbps = (int)Math.Round(inboundKBps / 1024.0 * 8);
                            int outboundMbps = (int)Math.Round(outboundKBps / 1024.0 * 8);
                            
                            return (inboundMbps, outboundMbps);
                        }
                    }
                }

                // 4. 如果都没有，返回默认值 1024 KB/s = 8 Mbps
                return (8, 8);
            }
            catch (Exception ex)
            {
                throw new Exception($"获取用户带宽限制失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取用户今日使用的流量（字节）
        /// </summary>
        public long GetTodayTraffic(string username)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                var query = "SELECT traffic FROM todaytraffic WHERE user = @username";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);

                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt64(result);
                }
            }
            catch
            {
                // 忽略错误，返回0
            }

            return 0;
        }

        /// <summary>
        /// 获取节点统计信息
        /// </summary>
        public (int total, int online) GetNodeStats()
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                // 获取总节点数
                var totalQuery = "SELECT COUNT(*) FROM nodes WHERE status != '401'";
                using var totalCmd = new MySqlCommand(totalQuery, conn);
                var total = Convert.ToInt32(totalCmd.ExecuteScalar());

                // 获取在线节点数
                var onlineQuery = "SELECT COUNT(*) FROM nodes WHERE status = '200'";
                using var onlineCmd = new MySqlCommand(onlineQuery, conn);
                var online = Convert.ToInt32(onlineCmd.ExecuteScalar());

                return (total, online);
            }
            catch
            {
                return (0, 0);
            }
        }

        /// <summary>
        /// 根据节点ID获取隧道列表
        /// </summary>
        public List<Proxy> GetProxiesByNodeID(string username, int nodeID)
        {
            var allProxies = GetProxiesByUsername(username);
            return allProxies.FindAll(p => p.Node == nodeID);
        }
    }
}
