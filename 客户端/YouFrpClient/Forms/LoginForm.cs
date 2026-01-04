using FrpClient.Models;
using FrpClient.Services;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FrpClient.Forms
{
    public partial class LoginForm : Form
    {
        private readonly Config _config;
        private readonly DatabaseService _dbService;
        private Point _mouseOffset;
        private bool _isDragging;

        public User? CurrentUser { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
            _config = LoadConfig();
            _dbService = new DatabaseService(_config);
            
            // 添加窗口拖动支持
            this.MouseDown += LoginForm_MouseDown;
            this.MouseMove += LoginForm_MouseMove;
            this.MouseUp += LoginForm_MouseUp;
            panelLeft.MouseDown += LoginForm_MouseDown;
            panelLeft.MouseMove += LoginForm_MouseMove;
            panelLeft.MouseUp += LoginForm_MouseUp;
            
            // 添加关闭按钮效果
            this.KeyPreview = true;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
            
            // 设置默认显示用户名密码登录
            ShowPasswordLogin();
            
            // 加载保存的Token
            LoadSavedToken();
            
            // 设置圆角效果
            ApplyRoundedCorners();
        }

        private void ShowPasswordLogin()
        {
            txtUsername.Visible = true;
            txtPassword.Visible = true;
            chkRememberMe.Visible = true;
            txtToken.Visible = false;
            
            btnLogin.BackColor = Color.FromArgb(100, 100, 200);
            btnTokenLogin.BackColor = Color.FromArgb(60, 60, 80);
        }

        private void ShowTokenLogin()
        {
            txtUsername.Visible = false;
            txtPassword.Visible = false;
            chkRememberMe.Visible = false;
            txtToken.Visible = true;
            
            btnLogin.BackColor = Color.FromArgb(60, 60, 80);
            btnTokenLogin.BackColor = Color.FromArgb(100, 100, 200);
        }

        private void ApplyRoundedCorners()
        {
            // 为文本框添加圆角效果
            txtToken.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            };
            txtPassword.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            };
        }

        private void LoginForm_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _mouseOffset = new Point(-e.X, -e.Y);
            }
        }

        private void LoginForm_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(_mouseOffset.X, _mouseOffset.Y);
                Location = mousePos;
            }
        }

        private void LoginForm_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = false;
            }
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

        private void LoadSavedToken()
        {
            try
            {
                // 加载保存的登录信息
                if (File.Exists("login.txt"))
                {
                    var lines = File.ReadAllLines("login.txt");
                    if (lines.Length >= 3)
                    {
                        var username = lines[0].Trim();
                        var password = lines[1].Trim();
                        var rememberMe = lines[2].Trim().ToLower() == "true";
                        
                        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                        {
                            txtUsername.Text = username;
                            txtPassword.Text = password;
                            chkRememberMe.Checked = rememberMe;
                            
                            // 如果勾选了自动登录，则自动执行登录
                            if (rememberMe)
                            {
                                // 延迟执行登录，确保界面已完全加载
                                this.Load += (s, e) => 
                                {
                                    System.Threading.Tasks.Task.Delay(100).ContinueWith(_ => 
                                    {
                                        this.Invoke(new Action(() => BtnLogin_Click(null, EventArgs.Empty)));
                                    });
                                };
                            }
                        }
                    }
                }
                
                // 加载Token（用于Token登录）
                if (File.Exists("token.txt"))
                {
                    var token = File.ReadAllText("token.txt").Trim();
                    if (!string.IsNullOrEmpty(token))
                    {
                        txtToken.Text = token;
                    }
                }
            }
            catch
            {
                // 忽略错误
            }
        }

        private void SaveLoginInfo(string username, string password, bool rememberMe)
        {
            try
            {
                if (rememberMe)
                {
                    File.WriteAllLines("login.txt", new[] { username, password, rememberMe.ToString() });
                }
                else
                {
                    if (File.Exists("login.txt"))
                    {
                        File.Delete("login.txt");
                    }
                }
            }
            catch
            {
                // 忽略错误
            }
        }

        private void SaveToken(string token)
        {
            try
            {
                if (chkRememberMe.Checked)
                {
                    File.WriteAllText("token.txt", token);
                }
                else
                {
                    if (File.Exists("token.txt"))
                    {
                        File.Delete("token.txt");
                    }
                }
            }
            catch
            {
                // 忽略错误
            }
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            // 切换到用户名密码登录模式
            if (!txtUsername.Visible)
            {
                ShowPasswordLogin();
                return;
            }

            var username = txtUsername.Text.Trim();
            var password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("请输入用户名和密码", "提示", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 禁用按钮
                btnTokenLogin.Enabled = false;
                btnLogin.Enabled = false;
                btnLogin.Text = "登录中...";
                this.Cursor = Cursors.WaitCursor;

                // 验证用户名密码
                var user = _dbService.LoginByPassword(username, password);

                if (user == null)
                {
                    MessageBox.Show("用户名或密码错误", "登录失败", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 保存登录信息
                SaveLoginInfo(username, password, chkRememberMe.Checked);

                // 设置当前用户
                CurrentUser = user;

                // 登录成功
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                // 显示详细的错误信息，包括内部异常
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\n详细信息: {ex.InnerException.Message}";
                }
                MessageBox.Show($"登录失败: {errorMessage}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 恢复按钮
                btnTokenLogin.Enabled = true;
                btnLogin.Enabled = true;
                btnLogin.Text = "立即登录";
                this.Cursor = Cursors.Default;
            }
        }

        private string? GetUserToken(string username)
        {
            try
            {
                return _dbService.GetUserToken(username);
            }
            catch
            {
                return null;
            }
        }

        private void BtnTokenLogin_Click(object? sender, EventArgs e)
        {
            // 切换到Token登录模式
            if (!txtToken.Visible)
            {
                ShowTokenLogin();
                return;
            }

            var token = txtToken.Text.Trim();

            if (string.IsNullOrEmpty(token))
            {
                MessageBox.Show("请输入访问密钥(Token)", "提示", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 禁用按钮
                btnTokenLogin.Enabled = false;
                btnLogin.Enabled = false;
                btnTokenLogin.Text = "登录中...";
                this.Cursor = Cursors.WaitCursor;

                // 验证Token
                var user = _dbService.GetUserByToken(token);

                if (user == null)
                {
                    MessageBox.Show("访问密钥无效或已过期", "登录失败", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 保存Token
                SaveToken(token);

                // 设置当前用户
                CurrentUser = user;

                // 登录成功
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"登录失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 恢复按钮
                btnTokenLogin.Enabled = true;
                btnLogin.Enabled = true;
                btnTokenLogin.Text = "Token登录";
                this.Cursor = Cursors.Default;
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
