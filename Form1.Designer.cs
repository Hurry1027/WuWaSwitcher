namespace WuWaSwitcher
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnBrowseBilibili;
        private System.Windows.Forms.Button btnBrowseOfficial;
        private System.Windows.Forms.TextBox txtOfficialPath;
        private System.Windows.Forms.TextBox txtBilibiliPath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSaveConfig;
        private System.Windows.Forms.Button btnRefreshIcon;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lblCurrentVersion;
        private System.Windows.Forms.Button btnSwitchVersion;
        private System.Windows.Forms.Button btnLaunchCurrent;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnLaunchOfficial = new System.Windows.Forms.Button();
            this.btnLaunchBilibili = new System.Windows.Forms.Button();
            this.btnBrowseBilibili = new System.Windows.Forms.Button();
            this.btnBrowseOfficial = new System.Windows.Forms.Button();
            this.txtOfficialPath = new System.Windows.Forms.TextBox();
            this.txtBilibiliPath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSaveConfig = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.linkLabelGitee = new System.Windows.Forms.LinkLabel();
            this.btn_bakcupCurrentVersion = new System.Windows.Forms.Button();
            this.checkBox_isAutoClose = new System.Windows.Forms.CheckBox();
            this.btnBackupSwitchVersion = new System.Windows.Forms.Button();
            this.lblCurrentVersion = new System.Windows.Forms.Label();
            this.btnSwitchVersion = new System.Windows.Forms.Button();
            this.btnLaunchCurrent = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.txtGameFolderPath = new System.Windows.Forms.TextBox();
            this.btnBrowseGameFolder = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnOpenGameFolder = new System.Windows.Forms.Button();
            this.btnScreenShot = new System.Windows.Forms.Button();
            this.btnReadme = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnLaunchOfficial);
            this.groupBox2.Controls.Add(this.btnLaunchBilibili);
            this.groupBox2.Controls.Add(this.btnBrowseBilibili);
            this.groupBox2.Controls.Add(this.btnBrowseOfficial);
            this.groupBox2.Controls.Add(this.txtOfficialPath);
            this.groupBox2.Controls.Add(this.txtBilibiliPath);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(18, 134);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(1014, 138);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "启动器路径设置(launcher.exe)";
            // 
            // btnLaunchOfficial
            // 
            this.btnLaunchOfficial.Location = new System.Drawing.Point(902, 74);
            this.btnLaunchOfficial.Margin = new System.Windows.Forms.Padding(4);
            this.btnLaunchOfficial.Name = "btnLaunchOfficial";
            this.btnLaunchOfficial.Size = new System.Drawing.Size(104, 34);
            this.btnLaunchOfficial.TabIndex = 7;
            this.btnLaunchOfficial.Text = "启动";
            this.btnLaunchOfficial.UseVisualStyleBackColor = true;
            this.btnLaunchOfficial.Click += new System.EventHandler(this.btnLaunchOfficial_Click);
            // 
            // btnLaunchBilibili
            // 
            this.btnLaunchBilibili.Location = new System.Drawing.Point(902, 31);
            this.btnLaunchBilibili.Margin = new System.Windows.Forms.Padding(4);
            this.btnLaunchBilibili.Name = "btnLaunchBilibili";
            this.btnLaunchBilibili.Size = new System.Drawing.Size(104, 34);
            this.btnLaunchBilibili.TabIndex = 6;
            this.btnLaunchBilibili.Text = "启动";
            this.btnLaunchBilibili.UseVisualStyleBackColor = true;
            this.btnLaunchBilibili.Click += new System.EventHandler(this.btnLaunchBilibili_Click);
            // 
            // btnBrowseBilibili
            // 
            this.btnBrowseBilibili.Location = new System.Drawing.Point(788, 31);
            this.btnBrowseBilibili.Margin = new System.Windows.Forms.Padding(4);
            this.btnBrowseBilibili.Name = "btnBrowseBilibili";
            this.btnBrowseBilibili.Size = new System.Drawing.Size(104, 34);
            this.btnBrowseBilibili.TabIndex = 5;
            this.btnBrowseBilibili.Text = "浏览...";
            this.btnBrowseBilibili.UseVisualStyleBackColor = true;
            this.btnBrowseBilibili.Click += new System.EventHandler(this.btnBrowseBilibili_Click);
            // 
            // btnBrowseOfficial
            // 
            this.btnBrowseOfficial.Location = new System.Drawing.Point(788, 73);
            this.btnBrowseOfficial.Margin = new System.Windows.Forms.Padding(4);
            this.btnBrowseOfficial.Name = "btnBrowseOfficial";
            this.btnBrowseOfficial.Size = new System.Drawing.Size(104, 34);
            this.btnBrowseOfficial.TabIndex = 4;
            this.btnBrowseOfficial.Text = "浏览...";
            this.btnBrowseOfficial.UseVisualStyleBackColor = true;
            this.btnBrowseOfficial.Click += new System.EventHandler(this.btnBrowseOfficial_Click);
            // 
            // txtOfficialPath
            // 
            this.txtOfficialPath.Location = new System.Drawing.Point(148, 78);
            this.txtOfficialPath.Margin = new System.Windows.Forms.Padding(4);
            this.txtOfficialPath.Name = "txtOfficialPath";
            this.txtOfficialPath.Size = new System.Drawing.Size(634, 28);
            this.txtOfficialPath.TabIndex = 3;
            // 
            // txtBilibiliPath
            // 
            this.txtBilibiliPath.Location = new System.Drawing.Point(148, 34);
            this.txtBilibiliPath.Margin = new System.Windows.Forms.Padding(4);
            this.txtBilibiliPath.Name = "txtBilibiliPath";
            this.txtBilibiliPath.Size = new System.Drawing.Size(634, 28);
            this.txtBilibiliPath.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 82);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(152, 18);
            this.label3.TabIndex = 1;
            this.label3.Text = "官服启动器路径：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 39);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(143, 18);
            this.label2.TabIndex = 0;
            this.label2.Text = "B服启动器路径：";
            // 
            // btnSaveConfig
            // 
            this.btnSaveConfig.Location = new System.Drawing.Point(894, 282);
            this.btnSaveConfig.Margin = new System.Windows.Forms.Padding(4);
            this.btnSaveConfig.Name = "btnSaveConfig";
            this.btnSaveConfig.Size = new System.Drawing.Size(138, 34);
            this.btnSaveConfig.TabIndex = 2;
            this.btnSaveConfig.Text = "保存配置";
            this.btnSaveConfig.UseVisualStyleBackColor = true;
            this.btnSaveConfig.Click += new System.EventHandler(this.btnSaveConfig_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.linkLabelGitee);
            this.groupBox3.Controls.Add(this.btn_bakcupCurrentVersion);
            this.groupBox3.Controls.Add(this.checkBox_isAutoClose);
            this.groupBox3.Controls.Add(this.btnBackupSwitchVersion);
            this.groupBox3.Controls.Add(this.lblCurrentVersion);
            this.groupBox3.Controls.Add(this.btnSwitchVersion);
            this.groupBox3.Controls.Add(this.btnLaunchCurrent);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Location = new System.Drawing.Point(18, 324);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox3.Size = new System.Drawing.Size(1014, 184);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "操作";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(592, 91);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(224, 18);
            this.label5.TabIndex = 8;
            this.label5.Text = "版本号变化后务必备份版本";
            // 
            // linkLabelGitee
            // 
            this.linkLabelGitee.AutoSize = true;
            this.linkLabelGitee.Location = new System.Drawing.Point(926, 158);
            this.linkLabelGitee.Name = "linkLabelGitee";
            this.linkLabelGitee.Size = new System.Drawing.Size(80, 18);
            this.linkLabelGitee.TabIndex = 7;
            this.linkLabelGitee.TabStop = true;
            this.linkLabelGitee.Text = "项目主页";
            this.linkLabelGitee.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelGitee_LinkClicked);
            // 
            // btn_bakcupCurrentVersion
            // 
            this.btn_bakcupCurrentVersion.Location = new System.Drawing.Point(348, 78);
            this.btn_bakcupCurrentVersion.Margin = new System.Windows.Forms.Padding(4);
            this.btn_bakcupCurrentVersion.Name = "btn_bakcupCurrentVersion";
            this.btn_bakcupCurrentVersion.Size = new System.Drawing.Size(228, 45);
            this.btn_bakcupCurrentVersion.TabIndex = 6;
            this.btn_bakcupCurrentVersion.Text = "备份当前版本";
            this.btn_bakcupCurrentVersion.UseVisualStyleBackColor = true;
            this.btn_bakcupCurrentVersion.Click += new System.EventHandler(this.btnBackup_Click);
            // 
            // checkBox_isAutoClose
            // 
            this.checkBox_isAutoClose.AutoSize = true;
            this.checkBox_isAutoClose.Location = new System.Drawing.Point(595, 143);
            this.checkBox_isAutoClose.Name = "checkBox_isAutoClose";
            this.checkBox_isAutoClose.Size = new System.Drawing.Size(178, 22);
            this.checkBox_isAutoClose.TabIndex = 5;
            this.checkBox_isAutoClose.Text = "启动后关闭切换器";
            this.checkBox_isAutoClose.UseVisualStyleBackColor = true;
            // 
            // btnBackupSwitchVersion
            // 
            this.btnBackupSwitchVersion.Location = new System.Drawing.Point(348, 131);
            this.btnBackupSwitchVersion.Margin = new System.Windows.Forms.Padding(4);
            this.btnBackupSwitchVersion.Name = "btnBackupSwitchVersion";
            this.btnBackupSwitchVersion.Size = new System.Drawing.Size(228, 45);
            this.btnBackupSwitchVersion.TabIndex = 4;
            this.btnBackupSwitchVersion.Text = "备份并切换版本";
            this.btnBackupSwitchVersion.UseVisualStyleBackColor = true;
            this.btnBackupSwitchVersion.Click += new System.EventHandler(this.btnBackupSwitchVersion_Click);
            // 
            // lblCurrentVersion
            // 
            this.lblCurrentVersion.AutoSize = true;
            this.lblCurrentVersion.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblCurrentVersion.Location = new System.Drawing.Point(146, 38);
            this.lblCurrentVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCurrentVersion.Name = "lblCurrentVersion";
            this.lblCurrentVersion.Size = new System.Drawing.Size(0, 21);
            this.lblCurrentVersion.TabIndex = 3;
            // 
            // btnSwitchVersion
            // 
            this.btnSwitchVersion.Location = new System.Drawing.Point(92, 131);
            this.btnSwitchVersion.Margin = new System.Windows.Forms.Padding(4);
            this.btnSwitchVersion.Name = "btnSwitchVersion";
            this.btnSwitchVersion.Size = new System.Drawing.Size(228, 45);
            this.btnSwitchVersion.TabIndex = 2;
            this.btnSwitchVersion.Text = "切换版本";
            this.btnSwitchVersion.UseVisualStyleBackColor = true;
            this.btnSwitchVersion.Click += new System.EventHandler(this.btnSwitchVersion_Click);
            // 
            // btnLaunchCurrent
            // 
            this.btnLaunchCurrent.Location = new System.Drawing.Point(92, 78);
            this.btnLaunchCurrent.Margin = new System.Windows.Forms.Padding(4);
            this.btnLaunchCurrent.Name = "btnLaunchCurrent";
            this.btnLaunchCurrent.Size = new System.Drawing.Size(228, 45);
            this.btnLaunchCurrent.TabIndex = 1;
            this.btnLaunchCurrent.Text = "启动当前版本";
            this.btnLaunchCurrent.UseVisualStyleBackColor = true;
            this.btnLaunchCurrent.Click += new System.EventHandler(this.btnLaunchCurrent_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 40);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(98, 18);
            this.label4.TabIndex = 0;
            this.label4.Text = "当前版本：";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "可执行文件|*.exe|所有文件|*.*";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 536);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 21, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1050, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 15);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 39);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(152, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "游戏文件夹路径：";
            // 
            // txtGameFolderPath
            // 
            this.txtGameFolderPath.Location = new System.Drawing.Point(148, 34);
            this.txtGameFolderPath.Margin = new System.Windows.Forms.Padding(4);
            this.txtGameFolderPath.Name = "txtGameFolderPath";
            this.txtGameFolderPath.Size = new System.Drawing.Size(634, 28);
            this.txtGameFolderPath.TabIndex = 1;
            // 
            // btnBrowseGameFolder
            // 
            this.btnBrowseGameFolder.Location = new System.Drawing.Point(790, 29);
            this.btnBrowseGameFolder.Margin = new System.Windows.Forms.Padding(4);
            this.btnBrowseGameFolder.Name = "btnBrowseGameFolder";
            this.btnBrowseGameFolder.Size = new System.Drawing.Size(104, 34);
            this.btnBrowseGameFolder.TabIndex = 2;
            this.btnBrowseGameFolder.Text = "浏览...";
            this.btnBrowseGameFolder.UseVisualStyleBackColor = true;
            this.btnBrowseGameFolder.Click += new System.EventHandler(this.btnBrowseGameFolder_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnOpenGameFolder);
            this.groupBox1.Controls.Add(this.btnBrowseGameFolder);
            this.groupBox1.Controls.Add(this.txtGameFolderPath);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(18, 18);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(1014, 88);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "游戏文件夹设置(Wuthering Waves Game)";
            // 
            // btnOpenGameFolder
            // 
            this.btnOpenGameFolder.Location = new System.Drawing.Point(902, 31);
            this.btnOpenGameFolder.Margin = new System.Windows.Forms.Padding(4);
            this.btnOpenGameFolder.Name = "btnOpenGameFolder";
            this.btnOpenGameFolder.Size = new System.Drawing.Size(104, 34);
            this.btnOpenGameFolder.TabIndex = 3;
            this.btnOpenGameFolder.Text = "查看";
            this.btnOpenGameFolder.UseVisualStyleBackColor = true;
            this.btnOpenGameFolder.Click += new System.EventHandler(this.btnOpenGameFolder_Click);
            // 
            // btnScreenShot
            // 
            this.btnScreenShot.Location = new System.Drawing.Point(18, 282);
            this.btnScreenShot.Margin = new System.Windows.Forms.Padding(4);
            this.btnScreenShot.Name = "btnScreenShot";
            this.btnScreenShot.Size = new System.Drawing.Size(138, 34);
            this.btnScreenShot.TabIndex = 5;
            this.btnScreenShot.Text = "截图文件夹";
            this.btnScreenShot.UseVisualStyleBackColor = true;
            this.btnScreenShot.Click += new System.EventHandler(this.btnScreenShot_Click);
            // 
            // btnReadme
            // 
            this.btnReadme.Location = new System.Drawing.Point(164, 282);
            this.btnReadme.Margin = new System.Windows.Forms.Padding(4);
            this.btnReadme.Name = "btnReadme";
            this.btnReadme.Size = new System.Drawing.Size(138, 34);
            this.btnReadme.TabIndex = 7;
            this.btnReadme.Text = "使用说明";
            this.btnReadme.UseVisualStyleBackColor = true;
            this.btnReadme.Click += new System.EventHandler(this.btnReadme_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1050, 558);
            this.Controls.Add(this.btnReadme);
            this.Controls.Add(this.btnScreenShot);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.btnSaveConfig);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "鸣潮官服B服切换器";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtGameFolderPath;
        private System.Windows.Forms.Button btnBrowseGameFolder;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnBackupSwitchVersion;
        private System.Windows.Forms.CheckBox checkBox_isAutoClose;
        private System.Windows.Forms.Button btnScreenShot;
        private System.Windows.Forms.Button btnReadme;
        private System.Windows.Forms.Button btn_bakcupCurrentVersion;
        private System.Windows.Forms.Button btnOpenGameFolder;
        private System.Windows.Forms.Button btnLaunchBilibili;
        private System.Windows.Forms.Button btnLaunchOfficial;
        private System.Windows.Forms.LinkLabel linkLabelGitee;
        private System.Windows.Forms.Label label5;
    }
}

