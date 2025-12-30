namespace FrpClient.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelSidebar = new System.Windows.Forms.Panel();
            this.lblAppTitle = new System.Windows.Forms.Label();
            this.btnSoftwareHome = new System.Windows.Forms.Button();
            this.btnRunLog = new System.Windows.Forms.Button();
            this.btnTunnelManage = new System.Windows.Forms.Button();
            this.btnLogout = new System.Windows.Forms.Button();
            this.panelTitleBar = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnMinimize = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.panelMain = new System.Windows.Forms.Panel();
            this.gbProxyList = new System.Windows.Forms.GroupBox();
            this.flowProxyList = new System.Windows.Forms.FlowLayoutPanel();
            this.gbNodeSelect = new System.Windows.Forms.GroupBox();
            this.cmbNodes = new System.Windows.Forms.ComboBox();
            this.lblNodeHint = new System.Windows.Forms.Label();
            this.gbControl = new System.Windows.Forms.GroupBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.gbLog = new System.Windows.Forms.GroupBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.panelSidebar.SuspendLayout();
            this.panelTitleBar.SuspendLayout();
            this.panelMain.SuspendLayout();
            this.gbProxyList.SuspendLayout();
            this.gbNodeSelect.SuspendLayout();
            this.gbControl.SuspendLayout();
            this.gbLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelSidebar
            // 
            this.panelSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(30)))));
            this.panelSidebar.Controls.Add(this.lblAppTitle);
            this.panelSidebar.Controls.Add(this.btnSoftwareHome);
            this.panelSidebar.Controls.Add(this.btnRunLog);
            this.panelSidebar.Controls.Add(this.btnTunnelManage);
            this.panelSidebar.Controls.Add(this.btnLogout);
            this.panelSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelSidebar.Location = new System.Drawing.Point(0, 0);
            this.panelSidebar.Name = "panelSidebar";
            this.panelSidebar.Size = new System.Drawing.Size(180, 700);
            this.panelSidebar.TabIndex = 0;
            // 
            // lblAppTitle
            // 
            this.lblAppTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblAppTitle.ForeColor = System.Drawing.Color.White;
            this.lblAppTitle.Location = new System.Drawing.Point(15, 20);
            this.lblAppTitle.Name = "lblAppTitle";
            this.lblAppTitle.Size = new System.Drawing.Size(150, 40);
            this.lblAppTitle.TabIndex = 0;
            this.lblAppTitle.Text = "YouFRP";
            this.lblAppTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnSoftwareHome
            // 
            this.btnSoftwareHome.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(50)))));
            this.btnSoftwareHome.FlatAppearance.BorderSize = 0;
            this.btnSoftwareHome.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSoftwareHome.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnSoftwareHome.ForeColor = System.Drawing.Color.White;
            this.btnSoftwareHome.Location = new System.Drawing.Point(15, 100);
            this.btnSoftwareHome.Name = "btnSoftwareHome";
            this.btnSoftwareHome.Size = new System.Drawing.Size(150, 45);
            this.btnSoftwareHome.TabIndex = 1;
            this.btnSoftwareHome.Text = "软件首页";
            this.btnSoftwareHome.UseVisualStyleBackColor = false;
            this.btnSoftwareHome.Click += new System.EventHandler(this.BtnSoftwareHome_Click);
            // 
            // btnRunLog
            // 
            this.btnRunLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(45)))));
            this.btnRunLog.FlatAppearance.BorderSize = 0;
            this.btnRunLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRunLog.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnRunLog.ForeColor = System.Drawing.Color.White;
            this.btnRunLog.Location = new System.Drawing.Point(15, 155);
            this.btnRunLog.Name = "btnRunLog";
            this.btnRunLog.Size = new System.Drawing.Size(150, 45);
            this.btnRunLog.TabIndex = 2;
            this.btnRunLog.Text = "运行日志";
            this.btnRunLog.UseVisualStyleBackColor = false;
            this.btnRunLog.Click += new System.EventHandler(this.BtnRunLog_Click);
            // 
            // btnTunnelManage
            // 
            this.btnTunnelManage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(45)))));
            this.btnTunnelManage.FlatAppearance.BorderSize = 0;
            this.btnTunnelManage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTunnelManage.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnTunnelManage.ForeColor = System.Drawing.Color.White;
            this.btnTunnelManage.Location = new System.Drawing.Point(15, 210);
            this.btnTunnelManage.Name = "btnTunnelManage";
            this.btnTunnelManage.Size = new System.Drawing.Size(150, 45);
            this.btnTunnelManage.TabIndex = 3;
            this.btnTunnelManage.Text = "隧道管理";
            this.btnTunnelManage.UseVisualStyleBackColor = false;
            this.btnTunnelManage.Click += new System.EventHandler(this.BtnTunnelManage_Click);
            // 
            // btnLogout
            // 
            this.btnLogout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnLogout.FlatAppearance.BorderSize = 0;
            this.btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogout.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnLogout.ForeColor = System.Drawing.Color.White;
            this.btnLogout.Location = new System.Drawing.Point(15, 640);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(150, 45);
            this.btnLogout.TabIndex = 4;
            this.btnLogout.Text = "退出登录";
            this.btnLogout.UseVisualStyleBackColor = false;
            this.btnLogout.Click += new System.EventHandler(this.BtnLogout_Click);
            // 
            // panelTitleBar
            // 
            this.panelTitleBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(45)))));
            this.panelTitleBar.Controls.Add(this.lblTitle);
            this.panelTitleBar.Controls.Add(this.btnMinimize);
            this.panelTitleBar.Controls.Add(this.btnClose);
            this.panelTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTitleBar.Location = new System.Drawing.Point(180, 0);
            this.panelTitleBar.Name = "panelTitleBar";
            this.panelTitleBar.Size = new System.Drawing.Size(1020, 50);
            this.panelTitleBar.TabIndex = 1;
            this.panelTitleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanelTitleBar_MouseDown);
            this.panelTitleBar.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PanelTitleBar_MouseMove);
            this.panelTitleBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PanelTitleBar_MouseUp);
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(20, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(300, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "软件首页";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnMinimize
            // 
            this.btnMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinimize.FlatAppearance.BorderSize = 0;
            this.btnMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMinimize.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnMinimize.ForeColor = System.Drawing.Color.White;
            this.btnMinimize.Location = new System.Drawing.Point(920, 0);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(50, 50);
            this.btnMinimize.TabIndex = 1;
            this.btnMinimize.Text = "─";
            this.btnMinimize.UseVisualStyleBackColor = true;
            this.btnMinimize.Click += new System.EventHandler(this.BtnMinimize_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(970, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(50, 50);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "✕";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // panelMain
            // 
            this.panelMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(50)))));
            this.panelMain.Controls.Add(this.gbLog);
            this.panelMain.Controls.Add(this.gbControl);
            this.panelMain.Controls.Add(this.gbProxyList);
            this.panelMain.Controls.Add(this.gbNodeSelect);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(180, 50);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(20);
            this.panelMain.Size = new System.Drawing.Size(1020, 650);
            this.panelMain.TabIndex = 2;
            // 
            // gbProxyList
            // 
            this.gbProxyList.Controls.Add(this.flowProxyList);
            this.gbProxyList.ForeColor = System.Drawing.Color.White;
            this.gbProxyList.Location = new System.Drawing.Point(30, 150);
            this.gbProxyList.Name = "gbProxyList";
            this.gbProxyList.Size = new System.Drawing.Size(450, 300);
            this.gbProxyList.TabIndex = 1;
            this.gbProxyList.TabStop = false;
            this.gbProxyList.Text = "隧道列表";
            // 
            // flowProxyList
            // 
            this.flowProxyList.AutoScroll = true;
            this.flowProxyList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowProxyList.Location = new System.Drawing.Point(3, 19);
            this.flowProxyList.Name = "flowProxyList";
            this.flowProxyList.Padding = new System.Windows.Forms.Padding(10);
            this.flowProxyList.Size = new System.Drawing.Size(444, 278);
            this.flowProxyList.TabIndex = 0;
            // 
            // gbNodeSelect
            // 
            this.gbNodeSelect.Controls.Add(this.cmbNodes);
            this.gbNodeSelect.Controls.Add(this.lblNodeHint);
            this.gbNodeSelect.ForeColor = System.Drawing.Color.White;
            this.gbNodeSelect.Location = new System.Drawing.Point(30, 30);
            this.gbNodeSelect.Name = "gbNodeSelect";
            this.gbNodeSelect.Size = new System.Drawing.Size(450, 100);
            this.gbNodeSelect.TabIndex = 0;
            this.gbNodeSelect.TabStop = false;
            this.gbNodeSelect.Text = "节点选择";
            // 
            // cmbNodes
            // 
            this.cmbNodes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(70)))));
            this.cmbNodes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNodes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbNodes.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cmbNodes.ForeColor = System.Drawing.Color.White;
            this.cmbNodes.FormattingEnabled = true;
            this.cmbNodes.Location = new System.Drawing.Point(20, 35);
            this.cmbNodes.Name = "cmbNodes";
            this.cmbNodes.Size = new System.Drawing.Size(410, 27);
            this.cmbNodes.TabIndex = 0;
            this.cmbNodes.SelectedIndexChanged += new System.EventHandler(this.CmbNodes_SelectedIndexChanged);
            // 
            // lblNodeHint
            // 
            this.lblNodeHint.AutoSize = true;
            this.lblNodeHint.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblNodeHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(170)))));
            this.lblNodeHint.Location = new System.Drawing.Point(20, 70);
            this.lblNodeHint.Name = "lblNodeHint";
            this.lblNodeHint.Size = new System.Drawing.Size(200, 16);
            this.lblNodeHint.TabIndex = 1;
            this.lblNodeHint.Text = "请选择一个节点以查看隧道列表";
            // 
            // gbControl
            // 
            this.gbControl.Controls.Add(this.lblStatus);
            this.gbControl.Controls.Add(this.btnRefresh);
            this.gbControl.Controls.Add(this.btnStop);
            this.gbControl.Controls.Add(this.btnStart);
            this.gbControl.ForeColor = System.Drawing.Color.White;
            this.gbControl.Location = new System.Drawing.Point(510, 30);
            this.gbControl.Name = "gbControl";
            this.gbControl.Size = new System.Drawing.Size(480, 100);
            this.gbControl.TabIndex = 2;
            this.gbControl.TabStop = false;
            this.gbControl.Text = "控制面板";
            // 
            // btnStart
            // 
            this.btnStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(150)))), ((int)(((byte)(80)))));
            this.btnStart.FlatAppearance.BorderSize = 0;
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnStart.ForeColor = System.Drawing.Color.White;
            this.btnStart.Location = new System.Drawing.Point(20, 35);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(100, 40);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "启动";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.btnStop.Enabled = false;
            this.btnStop.FlatAppearance.BorderSize = 0;
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnStop.ForeColor = System.Drawing.Color.White;
            this.btnStop.Location = new System.Drawing.Point(140, 35);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(100, 40);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "停止";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.BtnStop_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(100)))), ((int)(((byte)(150)))));
            this.btnRefresh.FlatAppearance.BorderSize = 0;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnRefresh.ForeColor = System.Drawing.Color.White;
            this.btnRefresh.Location = new System.Drawing.Point(260, 35);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(100, 40);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "刷新";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(170)))));
            this.lblStatus.Location = new System.Drawing.Point(380, 35);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(90, 40);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "未运行";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // gbLog
            // 
            this.gbLog.Controls.Add(this.txtLog);
            this.gbLog.ForeColor = System.Drawing.Color.White;
            this.gbLog.Location = new System.Drawing.Point(510, 150);
            this.gbLog.Name = "gbLog";
            this.gbLog.Size = new System.Drawing.Size(480, 470);
            this.gbLog.TabIndex = 3;
            this.gbLog.TabStop = false;
            this.gbLog.Text = "运行日志";
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(35)))));
            this.txtLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(200)))));
            this.txtLog.Location = new System.Drawing.Point(3, 19);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(474, 448);
            this.txtLog.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(35)))));
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.panelTitleBar);
            this.Controls.Add(this.panelSidebar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "YouFRP";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.panelSidebar.ResumeLayout(false);
            this.panelTitleBar.ResumeLayout(false);
            this.panelMain.ResumeLayout(false);
            this.gbProxyList.ResumeLayout(false);
            this.gbNodeSelect.ResumeLayout(false);
            this.gbNodeSelect.PerformLayout();
            this.gbControl.ResumeLayout(false);
            this.gbLog.ResumeLayout(false);
            this.gbLog.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel panelSidebar;
        private System.Windows.Forms.Label lblAppTitle;
        private System.Windows.Forms.Button btnSoftwareHome;
        private System.Windows.Forms.Button btnRunLog;
        private System.Windows.Forms.Button btnTunnelManage;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Panel panelTitleBar;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnMinimize;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.GroupBox gbProxyList;
        private System.Windows.Forms.FlowLayoutPanel flowProxyList;
        private System.Windows.Forms.GroupBox gbNodeSelect;
        private System.Windows.Forms.ComboBox cmbNodes;
        private System.Windows.Forms.Label lblNodeHint;
        private System.Windows.Forms.GroupBox gbControl;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.GroupBox gbLog;
        private System.Windows.Forms.TextBox txtLog;
    }
}
