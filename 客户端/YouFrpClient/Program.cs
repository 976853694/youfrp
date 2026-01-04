using FrpClient.Forms;
using System;
using System.Windows.Forms;

namespace FrpClient
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            
            // 显示登录窗口
            using var loginForm = new LoginForm();
            var result = loginForm.ShowDialog();
            
            if (result == DialogResult.OK && loginForm.CurrentUser != null)
            {
                // 登录成功，显示主窗口
                Application.Run(new MainForm(loginForm.CurrentUser));
            }
        }
    }
}
