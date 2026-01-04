using FrpClient.Models;
using FrpClient.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace FrpClient.Forms
{
    public partial class MainForm : Form
    {
        private readonly User _currentUser;
        private readonly Config _config;
        private readonly DatabaseService _dbService;
        private readonly FrpcService _frpcService;
        
        private List<Node> _nodes = new List<Node>();
        private List<Proxy> _proxies = new List<Proxy>();
        private Node? _selectedNode;
        
        private Point _mouseOffset;
        private bool _isDragging;

        public MainForm(User user)
        {
            InitializeComponent();
            
            _currentUser = user;
            _config = LoadConfig();
            _dbService = new DatabaseService(_config);
            _frpcService = new FrpcService(_config);
            
            // 订阅frpc输出事件
            _frpcService.OnOutputReceived += OnFrpcOutput;
            _frpcService.OnErrorReceived += OnFrpcError;
            
            // 加载数据
            LoadNodes();
            LoadSavedNodeSelection();
            
            // 设置初始视图
            ShowSoftwareHome();
        }

        private Config LoadConfig()
        {
            return new Config
            {
                DBHost = "138.2.24.169:3306",
                DBUser = "youfrp",
                DBPassword = "tgx123456.",
                DBName = "youfrp",
                FrpcPath = File.Exists("frpc.exe") ? Path.GetFullPath("frpc.exe") : "frpc.exe"
            };
        }

        private void LoadNodes()
        {
            try
            {
                _nodes = _dbService.GetAllNodes();
                cmbNodes.DataSource = _nodes;
                cmbNodes.DisplayMember = "Name";
                cmbNodes.ValueMember = "ID";
                
                if (_nodes.Count > 0 && cmbNodes.SelectedIndex < 0)
                {
                    cmbNodes.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                AppendLog($"加载节点失败: {ex.Message}");
                MessageBox.Show($"加载节点失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSavedNodeSelection()
        {
            try
            {
                if (File.Exists("node.txt"))
                {
                    var nodeIdStr = File.ReadAllText("node.txt").Trim();
                    if (int.TryParse(nodeIdStr, out int nodeId))
                    {
                        var node = _nodes.FirstOrDefault(n => n.ID == nodeId);
                        if (node != null)
                        {
                            cmbNodes.SelectedItem = node;
                        }
                    }
                }
            }
            catch
            {
                // 忽略错误
            }
        }

        private void SaveNodeSelection(int nodeId)
        {
            try
            {
                File.WriteAllText("node.txt", nodeId.ToString());
            }
            catch
            {
                // 忽略错误
            }
        }

        private void LoadProxies()
        {
            if (_selectedNode == null)
            {
                flowProxyList.Controls.Clear();
                return;
            }

            try
            {
                _proxies = _dbService.GetProxiesByNodeID(_currentUser.Username, _selectedNode.ID);
                DisplayProxies();
                AppendLog($"加载了 {_proxies.Count} 个隧道");
            }
            catch (Exception ex)
            {
                AppendLog($"加载隧道失败: {ex.Message}");
                MessageBox.Show($"加载隧道失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayProxies()
        {
            flowProxyList.Controls.Clear();

            foreach (var proxy in _proxies)
            {
                var panel = CreateProxyPanel(proxy);
                flowProxyList.Controls.Add(panel);
            }

            if (_proxies.Count == 0)
            {
                var lblEmpty = new Label
                {
                    Text = "该节点没有可用的隧道",
                    ForeColor = Color.FromArgb(150, 150, 170),
                    AutoSize = true,
                    Padding = new Padding(10)
                };
                flowProxyList.Controls.Add(lblEmpty);
            }
        }

        private Panel CreateProxyPanel(Proxy proxy)
        {
            var panel = new Panel
            {
                Width = 410,
                Height = 100,  // 增加高度以容纳节点IP
                BackColor = Color.FromArgb(45, 45, 60),
                Margin = new Padding(5),
                Padding = new Padding(10)
            };

            var lblName = new Label
            {
                Text = proxy.ProxyName,
                Font = new Font("Microsoft YaHei UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(10, 10)
            };

            var lblType = new Label
            {
                Text = proxy.ProxyType.ToUpper(),
                Font = new Font("Microsoft YaHei UI", 8),
                ForeColor = Color.FromArgb(100, 150, 255),
                AutoSize = true,
                Location = new Point(10, 35)
            };

            var lblMapping = new Label
            {
                Text = proxy.MappingInfo,
                Font = new Font("Consolas", 9),
                ForeColor = Color.FromArgb(180, 180, 200),
                AutoSize = true,
                Location = new Point(10, 55)
            };

            // 添加节点IP显示
            var lblNodeIP = new Label
            {
                Text = $"节点: {proxy.NodeIP}",
                Font = new Font("Microsoft YaHei UI", 8),
                ForeColor = Color.FromArgb(150, 150, 170),
                AutoSize = true,
                Location = new Point(10, 75)
            };

            // 添加节点IP复制按钮
            var btnCopyIP = new Button
            {
                Text = "📋",  // 剪贴板图标
                Font = new Font("Microsoft YaHei UI", 9),
                Size = new Size(30, 20),
                Location = new Point(150, 73),
                BackColor = Color.FromArgb(60, 60, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Tag = proxy.NodeIP
            };
            btnCopyIP.FlatAppearance.BorderSize = 0;
            btnCopyIP.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 100, 150);

            // 复制按钮点击事件
            btnCopyIP.Click += (s, e) =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(proxy.NodeIP))
                    {
                        Clipboard.SetText(proxy.NodeIP);
                        AppendLog($"已复制节点IP: {proxy.NodeIP}");
                        
                        // 临时改变按钮文字提示已复制
                        var btn = (Button)s;
                        var originalText = btn.Text;
                        btn.Text = "✓";
                        btn.BackColor = Color.FromArgb(80, 150, 80);
                        
                        // 1秒后恢复
                        var timer = new System.Windows.Forms.Timer();
                        timer.Interval = 1000;
                        timer.Tick += (sender, args) =>
                        {
                            btn.Text = originalText;
                            btn.BackColor = Color.FromArgb(60, 60, 80);
                            timer.Stop();
                            timer.Dispose();
                        };
                        timer.Start();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"复制失败: {ex.Message}", "错误", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 添加悬停提示
            var tooltipCopy = new System.Windows.Forms.ToolTip();
            tooltipCopy.SetToolTip(btnCopyIP, "点击复制节点IP");

            // 使用Label作为可点击的状态图标
            var lblStatus = new Label
            {
                Text = proxy.IsEnabled ? "●" : "○",  // ●=实心圆 ○=空心圆
                Font = new Font("Microsoft YaHei UI", 18),
                ForeColor = proxy.IsEnabled ? Color.FromArgb(80, 200, 80) : Color.FromArgb(150, 150, 150),
                AutoSize = true,
                Location = new Point(370, 35),  // 调整位置以适应新高度
                Cursor = Cursors.Hand,
                Tag = proxy  // 将proxy对象存储在Tag中
            };

            // 添加点击事件
            lblStatus.Click += (s, e) =>
            {
                try
                {
                    // 切换状态
                    _dbService.ToggleProxyStatus(proxy.ID);
                    
                    // 切换UI显示
                    proxy.Status = proxy.Status == "0" ? "1" : "0";
                    lblStatus.Text = proxy.IsEnabled ? "●" : "○";
                    lblStatus.ForeColor = proxy.IsEnabled ? Color.FromArgb(80, 200, 80) : Color.FromArgb(150, 150, 150);
                    
                    AppendLog($"隧道 {proxy.ProxyName} 已{(proxy.IsEnabled ? "启用" : "禁用")}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"切换隧道状态失败: {ex.Message}", "错误", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 添加悬停提示
            var tooltip = new System.Windows.Forms.ToolTip();
            tooltip.SetToolTip(lblStatus, "点击切换启用/禁用状态");

            panel.Controls.Add(lblName);
            panel.Controls.Add(lblType);
            panel.Controls.Add(lblMapping);
            panel.Controls.Add(lblNodeIP);  // 添加节点IP标签
            panel.Controls.Add(btnCopyIP);  // 添加复制按钮
            panel.Controls.Add(lblStatus);

            return panel;
        }

        private void AppendLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AppendLog), message);
                return;
            }

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText($"[{timestamp}] {message}\r\n");
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private void OnFrpcOutput(string output)
        {
            AppendLog(output);
        }

        private void OnFrpcError(string error)
        {
            AppendLog($"[错误] {error}");
        }

        private void UpdateButtonStates()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateButtonStates));
                return;
            }

            bool isRunning = _frpcService.IsRunning;
            btnStart.Enabled = !isRunning && _selectedNode != null;
            btnStop.Enabled = isRunning;
            lblStatus.Text = isRunning ? "运行中" : "未运行";
            lblStatus.ForeColor = isRunning ? Color.FromArgb(80, 200, 80) : Color.FromArgb(150, 150, 170);
        }

        private void ShowSoftwareHome()
        {
            lblTitle.Text = "软件首页";
            UpdateSidebarButtonColors(btnSoftwareHome);
            
            // 隐藏隧道管理控件
            gbNodeSelect.Visible = false;
            gbProxyList.Visible = false;
            gbControl.Visible = false;
            gbLog.Visible = false;
            
            // 创建或显示首页面板
            ShowHomePanel();
        }

        private void ShowRunLog()
        {
            lblTitle.Text = "运行日志";
            UpdateSidebarButtonColors(btnRunLog);
            
            // 隐藏首页面板
            if (_homePanel != null)
            {
                _homePanel.Visible = false;
            }
            
            // 隐藏隧道管理控件
            gbNodeSelect.Visible = false;
            gbProxyList.Visible = false;
            gbControl.Visible = false;
            
            // 显示运行日志（全屏）
            gbLog.Visible = true;
            gbLog.Location = new Point(30, 30);
            gbLog.Size = new Size(960, 590);
        }

        private void ShowTunnelManage()
        {
            lblTitle.Text = "隧道管理";
            UpdateSidebarButtonColors(btnTunnelManage);
            
            // 隐藏首页面板
            if (_homePanel != null)
            {
                _homePanel.Visible = false;
            }
            
            // 显示隧道管理控件
            gbNodeSelect.Visible = true;
            gbProxyList.Visible = true;
            gbControl.Visible = true;
            gbLog.Visible = true;
            
            // 恢复原始布局
            gbNodeSelect.Location = new Point(30, 30);
            gbNodeSelect.Size = new Size(450, 100);
            gbProxyList.Location = new Point(30, 150);
            gbProxyList.Size = new Size(450, 470);
            gbControl.Location = new Point(510, 30);
            gbControl.Size = new Size(480, 100);
            gbLog.Location = new Point(510, 150);
            gbLog.Size = new Size(480, 470);
        }

        private void UpdateSidebarButtonColors(Button activeButton)
        {
            btnSoftwareHome.BackColor = Color.FromArgb(30, 30, 45);
            btnRunLog.BackColor = Color.FromArgb(30, 30, 45);
            btnTunnelManage.BackColor = Color.FromArgb(30, 30, 45);
            
            activeButton.BackColor = Color.FromArgb(35, 35, 50);
        }

        private Panel? _homePanel;

        private void ShowHomePanel()
        {
            // 如果首页面板已存在，直接显示
            if (_homePanel != null)
            {
                _homePanel.Visible = true;
                _homePanel.BringToFront();
                return;
            }

            // 创建首页面板
            _homePanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(980, 610),
                BackColor = Color.FromArgb(35, 35, 50),
                AutoScroll = true
            };

            try
            {
                // 获取用户详细信息
                var userDetails = _dbService.GetUserDetails(_currentUser.Username);
                var todayTraffic = _dbService.GetTodayTraffic(_currentUser.Username);
                var nodeStats = _dbService.GetNodeStats();
                var userToken = _dbService.GetUserToken(_currentUser.Username) ?? "未知";

                // 计算流量数据
                long totalTrafficMB = userDetails.traffic;
                long usedTrafficBytes = todayTraffic;
                long usedTrafficMB = usedTrafficBytes / 1024 / 1024;
                long remainingTrafficMB = Math.Max(0, totalTrafficMB - usedTrafficMB);
                int trafficPercentage = totalTrafficMB > 0 ? (int)Math.Min(100, (usedTrafficMB * 100 / totalTrafficMB)) : 0;

                int yPos = 10;

                // 欢迎信息
                var lblWelcome = new Label
                {
                    Text = $"欢迎回来，{_currentUser.Username}！",
                    Font = new Font("Microsoft YaHei UI", 14, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = true,
                    Location = new Point(20, yPos)
                };
                _homePanel.Controls.Add(lblWelcome);
                yPos += 40;

                var lblWelcomeMsg = new Label
                {
                    Text = "欢迎使用YOU FRP 内网穿透系统，您可以在这里管理您的隧道、查看流量使用情况和获取服务器状态。",
                    Font = new Font("Microsoft YaHei UI", 9),
                    ForeColor = Color.FromArgb(180, 180, 200),
                    AutoSize = true,
                    Location = new Point(20, yPos),
                    MaximumSize = new Size(940, 0)
                };
                _homePanel.Controls.Add(lblWelcomeMsg);
                yPos += 35;

                // 统计卡片
                int cardWidth = 225;
                int cardHeight = 110;
                int cardSpacing = 15;

                // 剩余流量卡片
                var cardTraffic = CreateStatCard(
                    $"{remainingTrafficMB / 1024.0:F2} GB",
                    "剩余流量",
                    Color.FromArgb(70, 130, 180),
                    20, yPos, cardWidth, cardHeight
                );
                _homePanel.Controls.Add(cardTraffic);

                // 隧道数量卡片
                var cardProxies = CreateStatCard(
                    userDetails.proxies.ToString(),
                    "隧道数量",
                    Color.FromArgb(80, 150, 80),
                    20 + cardWidth + cardSpacing, yPos, cardWidth, cardHeight
                );
                _homePanel.Controls.Add(cardProxies);

                // 在线节点卡片
                var cardNodes = CreateStatCard(
                    $"{nodeStats.online} / {nodeStats.total}",
                    "在线节点",
                    Color.FromArgb(200, 140, 60),
                    20 + (cardWidth + cardSpacing) * 2, yPos, cardWidth, cardHeight
                );
                _homePanel.Controls.Add(cardNodes);

                // 带宽限制卡片 - 动态获取（优先级：limits表 > groups表 > 默认值）
                var bandwidth = _dbService.GetUserBandwidth(_currentUser.Username);
                var bandwidthText = $"{bandwidth.inbound} Mbps";
                var cardBandwidth = CreateStatCard(
                    bandwidthText,
                    "带宽限制",
                    Color.FromArgb(150, 80, 80),
                    20 + (cardWidth + cardSpacing) * 3, yPos, cardWidth, cardHeight
                );
                _homePanel.Controls.Add(cardBandwidth);

                yPos += cardHeight + 20;

                // 用户信息面板
                var gbUserInfo = new GroupBox
                {
                    Text = "用户信息",
                    ForeColor = Color.White,
                    Location = new Point(20, yPos),
                    Size = new Size(460, 230),
                    Font = new Font("Microsoft YaHei UI", 10, FontStyle.Bold)
                };

                int infoY = 30;
                int rowHeight = 35;

                // 用户名
                AddInfoRow(gbUserInfo, "用户名", _currentUser.Username, infoY);
                infoY += rowHeight;

                // 注册邮箱
                AddInfoRow(gbUserInfo, "注册邮箱", userDetails.email, infoY);
                infoY += rowHeight;

                // 注册时间
                var regDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(userDetails.regtime)).DateTime.ToString("yyyy-MM-dd");
                AddInfoRow(gbUserInfo, "注册时间", regDate, infoY);
                infoY += rowHeight;

                // 用户组
                AddInfoRow(gbUserInfo, "用户组", userDetails.group, infoY);
                infoY += rowHeight;

                // 访问密钥
                var lblTokenLabel = new Label
                {
                    Text = "访问密钥",
                    Font = new Font("Microsoft YaHei UI", 9),
                    ForeColor = Color.FromArgb(200, 200, 220),
                    Location = new Point(20, infoY),
                    Size = new Size(120, 25)
                };
                gbUserInfo.Controls.Add(lblTokenLabel);

                var lblTokenValue = new Label
                {
                    Text = userToken,
                    Font = new Font("Consolas", 9),
                    ForeColor = Color.FromArgb(100, 200, 255),
                    Location = new Point(140, infoY),
                    Size = new Size(200, 25)
                };
                gbUserInfo.Controls.Add(lblTokenValue);

                var btnCopyToken = new Button
                {
                    Text = "复制",
                    Font = new Font("Microsoft YaHei UI", 8),
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(70, 130, 180),
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(350, infoY - 2),
                    Size = new Size(80, 28)
                };
                btnCopyToken.FlatAppearance.BorderSize = 0;
                btnCopyToken.Click += (s, e) =>
                {
                    try
                    {
                        Clipboard.SetText(userToken);
                        MessageBox.Show("已复制到剪贴板", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch
                    {
                        MessageBox.Show("复制失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                gbUserInfo.Controls.Add(btnCopyToken);
                infoY += rowHeight + 10;

                // 流量使用情况
                var lblTrafficTitle = new Label
                {
                    Text = "流量使用情况",
                    Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(20, infoY),
                    AutoSize = true
                };
                gbUserInfo.Controls.Add(lblTrafficTitle);
                infoY += 25;

                var lblTrafficInfo = new Label
                {
                    Text = $"已用: {usedTrafficMB / 1024.0:F2} GB  /  总计: {totalTrafficMB / 1024.0:F2} GB",
                    Font = new Font("Microsoft YaHei UI", 8),
                    ForeColor = Color.FromArgb(180, 180, 200),
                    Location = new Point(20, infoY),
                    AutoSize = true
                };
                gbUserInfo.Controls.Add(lblTrafficInfo);
                infoY += 25;

                // 流量进度条
                var pnlProgressBg = new Panel
                {
                    Location = new Point(20, infoY),
                    Size = new Size(420, 12),
                    BackColor = Color.FromArgb(50, 50, 70)
                };
                gbUserInfo.Controls.Add(pnlProgressBg);

                var pnlProgress = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(Math.Max(1, pnlProgressBg.Width * trafficPercentage / 100), 12),
                    BackColor = Color.FromArgb(70, 130, 180)
                };
                pnlProgressBg.Controls.Add(pnlProgress);

                _homePanel.Controls.Add(gbUserInfo);

                // 公告面板
                var gbAnnouncement = new GroupBox
                {
                    Text = "最新公告",
                    ForeColor = Color.White,
                    Location = new Point(500, yPos),
                    Size = new Size(460, 300),
                    Font = new Font("Microsoft YaHei UI", 10, FontStyle.Bold)
                };

                var txtAnnouncement = new TextBox
                {
                    Text = "欢迎使用 YOUFRP 内网穿透系统\r\n" +
                           "\r\n" +
                           "使用提示：\r\n" +
                           "1. 点击“隧道管理”页面查看您的隧道列表\r\n" +
                           "2. 选择节点后点击“启动”开始使用\r\n" +
                           "3. 在“运行日志”页面可以查看实时运行状态\r\n" +
                           "\r\n" +
                           "祝您使用愉快！",
                    Multiline = true,
                    ReadOnly = true,
                    BorderStyle = BorderStyle.None,
                    BackColor = Color.FromArgb(45, 45, 60),
                    ForeColor = Color.FromArgb(200, 200, 220),
                    Font = new Font("Microsoft YaHei UI", 9),
                    Location = new Point(15, 30),
                    Size = new Size(430, 255)
                };
                gbAnnouncement.Controls.Add(txtAnnouncement);

                _homePanel.Controls.Add(gbAnnouncement);
            }
            catch (Exception ex)
            {
                var lblError = new Label
                {
                    Text = $"加载首页数据失败: {ex.Message}",
                    ForeColor = Color.FromArgb(255, 100, 100),
                    AutoSize = true,
                    Location = new Point(20, 20)
                };
                _homePanel.Controls.Add(lblError);
            }

            panelMain.Controls.Add(_homePanel);
            _homePanel.BringToFront();
        }

        private Panel CreateStatCard(string value, string label, Color bgColor, int x, int y, int width, int height)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = bgColor
            };

            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Microsoft YaHei UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            card.Controls.Add(lblValue);

            var lblLabel = new Label
            {
                Text = label,
                Font = new Font("Microsoft YaHei UI", 10),
                ForeColor = Color.FromArgb(230, 230, 240),
                AutoSize = true,
                Location = new Point(20, 65)
            };
            card.Controls.Add(lblLabel);

            return card;
        }

        private void AddInfoRow(GroupBox parent, string label, string value, int y)
        {
            var lblLabel = new Label
            {
                Text = label,
                Font = new Font("Microsoft YaHei UI", 9),
                ForeColor = Color.FromArgb(200, 200, 220),
                Location = new Point(20, y),
                Size = new Size(120, 25)
            };
            parent.Controls.Add(lblLabel);

            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Microsoft YaHei UI", 9),
                ForeColor = Color.White,
                Location = new Point(140, y),
                Size = new Size(300, 25)
            };
            parent.Controls.Add(lblValue);
        }

        // 事件处理

        private void CmbNodes_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _selectedNode = cmbNodes.SelectedItem as Node;
            
            if (_selectedNode != null)
            {
                SaveNodeSelection(_selectedNode.ID);
                LoadProxies();
                UpdateButtonStates();
            }
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
            if (_selectedNode == null)
            {
                MessageBox.Show("请先选择一个节点", "提示", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                AppendLog($"正在启动 frpc...");
                
                // 生成配置文件
                var enabledProxies = _proxies.Where(p => p.Status == "0").ToList();
                
                if (enabledProxies.Count == 0)
                {
                    MessageBox.Show("该节点没有可用的隧道", "提示", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 获取用户token（16位）
                var userToken = _dbService.GetUserToken(_currentUser.Username) ?? "";
                if (string.IsNullOrEmpty(userToken))
                {
                    MessageBox.Show("获取用户Token失败，请重新登录", "错误", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _frpcService.GenerateConfig(_selectedNode, enabledProxies, userToken);
                AppendLog("配置文件已生成");

                // 启动frpc
                _frpcService.StartFrpc();
                AppendLog("frpc 已启动");

                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                AppendLog($"启动失败: {ex.Message}");
                MessageBox.Show($"启动失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnStop_Click(object? sender, EventArgs e)
        {
            try
            {
                AppendLog("正在停止 frpc...");
                _frpcService.StopFrpc();
                AppendLog("frpc 已停止");
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                AppendLog($"停止失败: {ex.Message}");
                MessageBox.Show($"停止失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadNodes();
            LoadProxies();
            AppendLog("已刷新节点和隧道列表");
        }

        private void BtnSoftwareHome_Click(object? sender, EventArgs e)
        {
            ShowSoftwareHome();
        }

        private void BtnRunLog_Click(object? sender, EventArgs e)
        {
            ShowRunLog();
        }

        private void BtnTunnelManage_Click(object? sender, EventArgs e)
        {
            ShowTunnelManage();
        }

        private void BtnMinimize_Click(object? sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void BtnClose_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        private void PanelTitleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _mouseOffset = new Point(-e.X, -e.Y);
            }
        }

        private void PanelTitleBar_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(_mouseOffset.X, _mouseOffset.Y);
                Location = mousePos;
            }
        }

        private void PanelTitleBar_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = false;
            }
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // 停止frpc
            if (_frpcService.IsRunning)
            {
                var result = MessageBox.Show(
                    "frpc正在运行中，确定要退出吗？", 
                    "确认退出", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
                
                _frpcService.StopFrpc();
            }
        }

        private void BtnLogout_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "确定要退出登录吗？", 
                "确认退出", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                // 停止frpc
                if (_frpcService.IsRunning)
                {
                    _frpcService.StopFrpc();
                }
                
                // 删除保存的登录信息
                try
                {
                    if (File.Exists("login.txt"))
                    {
                        File.Delete("login.txt");
                    }
                }
                catch
                {
                    // 忽略错误
                }
                
                // 关闭当前窗口并显示登录窗口
                this.Hide();
                
                using var loginForm = new LoginForm();
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    // 重新启动主窗口
                    Application.Restart();
                }
                else
                {
                    // 用户取消登录，关闭程序
                    Application.Exit();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // 绘制窗口边框
            using var pen = new Pen(Color.FromArgb(60, 60, 80), 2);
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }
}
