using FrpClient.Services;
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FrpClient.Forms
{
    public partial class RegisterForm : Form
    {
        private readonly DatabaseService _dbService;

        public RegisterForm(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            var username = txtUsername.Text.Trim();
            var email = txtEmail.Text.Trim();
            var password = txtPassword.Text;
            var confirmPassword = txtConfirmPassword.Text;

            // 验证输入
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || 
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("请填写完整信息", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 验证用户名格式
            if (!Regex.IsMatch(username, @"^[A-Za-z0-9_-]{3,32}$"))
            {
                MessageBox.Show("用户名格式不正确\n必须为3-32位字母、数字、下划线或连字符", "提示", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 验证邮箱格式
            if (!Regex.IsMatch(email, @"^\w[-\w.+]*@([A-Za-z0-9][-A-Za-z0-9]+\.)+[A-Za-z]{2,48}$"))
            {
                MessageBox.Show("邮箱格式不正确", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 验证密码
            if (password.Length < 6)
            {
                MessageBox.Show("密码长度不能少于6位", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("两次输入的密码不一致", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnRegister.Enabled = false;
                btnRegister.Text = "注册中...";

                // 执行注册
                var success = _dbService.RegisterUser(username, email, password);

                if (success)
                {
                    MessageBox.Show($"注册成功！\n\n用户名: {username}\n邮箱: {email}\n\n请使用您的账号登录", 
                        "注册成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("注册失败，请重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "注册失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRegister.Enabled = true;
                btnRegister.Text = "注册";
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
