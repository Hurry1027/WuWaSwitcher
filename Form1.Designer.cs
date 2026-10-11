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
        private System.Windows.Forms.Button btnSwitchOfficialVersion;
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
            this.btnLaunchWeGame = new System.Windows.Forms.Button();
            this.btnBrowseWeGame = new System.Windows.Forms.Button();
            this.txtWeGamePath = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
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
            this.btnCheckGame = new System.Windows.Forms.Button();
            this.btnBackupSwitchBilibiliVersion = new System.Windows.Forms.Button();
            this.btnSwitchBilibiliVersion = new System.Windows.Forms.Button();
            this.btnBackupSwitchWegameVersion = new System.Windows.Forms.Button();
            this.btnSwitchWegameVersion = new System.Windows.Forms.Button();
            this.btn_bakcupCurrentVersion = new System.Windows.Forms.Button();
            this.checkBox_isAutoClose = new System.Windows.Forms.CheckBox();
            this.btnBackupSwitchOfficialVersion = new System.Windows.Forms.Button();
            this.lblCurrentVersion = new System.Windows.Forms.Label();
            this.btnSwitchOfficialVersion = new System.Windows.Forms.Button();
            this.btnLaunchCurrent = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.labelTip = new System.Windows.Forms.Label();
            this.linkLabelGitee = new System.Windows.Forms.LinkLabel();
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
            this.btnCheckUpdate = new System.Windows.Forms.LinkLabel();
            this.button1 = new System.Windows.Forms.Button();
            this.btnLoadConfig = new System.Windows.Forms.Button();
            this.btnWuWaDiff = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnLaunchWeGame);
            this.groupBox2.Controls.Add(this.btnBrowseWeGame);
            this.groupBox2.Controls.Add(this.txtWeGamePath);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.btnLaunchOfficial);
            this.groupBox2.Controls.Add(this.btnLaunchBilibili);
            this.groupBox2.Controls.Add(this.btnBrowseBilibili);
            this.groupBox2.Controls.Add(this.btnBrowseOfficial);
            this.groupBox2.Controls.Add(this.txtOfficialPath);
            this.groupBox2.Controls.Add(this.txtBilibiliPath);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(18, 117);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(1014, 162);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "启动器路径设置(launcher.exe)";
            // 
            // btnLaunchWeGame
            // 
            this.btnLaunchWeGame.Location = new System.Drawing.Point(900, 117);
            this.btnLaunchWeGame.Margin = new System.Windows.Forms.Padding(4);
            this.btnLaunchWeGame.Name = "btnLaunchWeGame";
            this.btnLaunchWeGame.Size = new System.Drawing.Size(104, 34);
            this.btnLaunchWeGame.TabIndex = 11;
            this.btnLaunchWeGame.Text = "启动";
            this.btnLaunchWeGame.UseVisualStyleBackColor = true;
            this.btnLaunchWeGame.Click += new System.EventHandler(this.btnLaunchWeGame_Click);
            // 
            // btnBrowseWeGame
            // 
            this.btnBrowseWeGame.Location = new System.Drawing.Point(788, 116);
            this.btnBrowseWeGame.Margin = new System.Windows.Forms.Padding(4);
            this.btnBrowseWeGame.Name = "btnBrowseWeGame";
            this.btnBrowseWeGame.Size = new System.Drawing.Size(104, 34);
            this.btnBrowseWeGame.TabIndex = 10;
            this.btnBrowseWeGame.Text = "浏览...";
            this.btnBrowseWeGame.UseVisualStyleBackColor = true;
            this.btnBrowseWeGame.Click += new System.EventHandler(this.btnBrowseWeGame_Click);
            // 
            // txtWeGamePath
            // 
            this.txtWeGamePath.Location = new System.Drawing.Point(148, 121);
            this.txtWeGamePath.Margin = new System.Windows.Forms.Padding(4);
            this.txtWeGamePath.Name = "txtWeGamePath";
            this.txtWeGamePath.Size = new System.Drawing.Size(634, 28);
            this.txtWeGamePath.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 125);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(116, 18);
            this.label6.TabIndex = 8;
            this.label6.Text = "WeGame路径：";
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
            this.btnSaveConfig.Location = new System.Drawing.Point(920, 287);
            this.btnSaveConfig.Margin = new System.Windows.Forms.Padding(4);
            this.btnSaveConfig.Name = "btnSaveConfig";
            this.btnSaveConfig.Size = new System.Drawing.Size(102, 34);
            this.btnSaveConfig.TabIndex = 2;
            this.btnSaveConfig.Text = "保存配置";
            this.btnSaveConfig.UseVisualStyleBackColor = true;
            this.btnSaveConfig.Click += new System.EventHandler(this.btnSaveConfig_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnCheckGame);
            this.groupBox3.Controls.Add(this.btnBackupSwitchBilibiliVersion);
            this.groupBox3.Controls.Add(this.btnSwitchBilibiliVersion);
            this.groupBox3.Controls.Add(this.btnBackupSwitchWegameVersion);
            this.groupBox3.Controls.Add(this.btnSwitchWegameVersion);
            this.groupBox3.Controls.Add(this.btn_bakcupCurrentVersion);
            this.groupBox3.Controls.Add(this.checkBox_isAutoClose);
            this.groupBox3.Controls.Add(this.btnBackupSwitchOfficialVersion);
            this.groupBox3.Controls.Add(this.lblCurrentVersion);
            this.groupBox3.Controls.Add(this.btnSwitchOfficialVersion);
            this.groupBox3.Controls.Add(this.btnLaunchCurrent);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Location = new System.Drawing.Point(16, 340);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox3.Size = new System.Drawing.Size(1014, 184);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "操作";
            // 
            // btnCheckGame
            // 
            this.btnCheckGame.Location = new System.Drawing.Point(662, 24);
            this.btnCheckGame.Margin = new System.Windows.Forms.Padding(4);
            this.btnCheckGame.Name = "btnCheckGame";
            this.btnCheckGame.Size = new System.Drawing.Size(144, 34);
            this.btnCheckGame.TabIndex = 8;
            this.btnCheckGame.Text = "检查客户端版本";
            this.btnCheckGame.UseVisualStyleBackColor = true;
            this.btnCheckGame.Click += new System.EventHandler(this.btnCheckGame_Click);
            // 
            // btnBackupSwitchBilibiliVersion
            // 
            this.btnBackupSwitchBilibiliVersion.Location = new System.Drawing.Point(484, 131);
            this.btnBackupSwitchBilibiliVersion.Margin = new System.Windows.Forms.Padding(4);
            this.btnBackupSwitchBilibiliVersion.Name = "btnBackupSwitchBilibiliVersion";
            this.btnBackupSwitchBilibiliVersion.Size = new System.Drawing.Size(228, 45);
            this.btnBackupSwitchBilibiliVersion.TabIndex = 12;
            this.btnBackupSwitchBilibiliVersion.Text = "备份并切换到Bilibili端";
            this.btnBackupSwitchBilibiliVersion.UseVisualStyleBackColor = true;
            this.btnBackupSwitchBilibiliVersion.Click += new System.EventHandler(this.btnBackupSwitchBilibiliVersion_Click);
            // 
            // btnSwitchBilibiliVersion
            // 
            this.btnSwitchBilibiliVersion.Location = new System.Drawing.Point(484, 78);
            this.btnSwitchBilibiliVersion.Margin = new System.Windows.Forms.Padding(4);
            this.btnSwitchBilibiliVersion.Name = "btnSwitchBilibiliVersion";
            this.btnSwitchBilibiliVersion.Size = new System.Drawing.Size(228, 45);
            this.btnSwitchBilibiliVersion.TabIndex = 11;
            this.btnSwitchBilibiliVersion.Text = "切换到Bilibili端";
            this.btnSwitchBilibiliVersion.UseVisualStyleBackColor = true;
            this.btnSwitchBilibiliVersion.Click += new System.EventHandler(this.btnSwitchBilibiliVersion_Click);
            // 
            // btnBackupSwitchWegameVersion
            // 
            this.btnBackupSwitchWegameVersion.Location = new System.Drawing.Point(720, 131);
            this.btnBackupSwitchWegameVersion.Margin = new System.Windows.Forms.Padding(4);
            this.btnBackupSwitchWegameVersion.Name = "btnBackupSwitchWegameVersion";
            this.btnBackupSwitchWegameVersion.Size = new System.Drawing.Size(228, 45);
            this.btnBackupSwitchWegameVersion.TabIndex = 10;
            this.btnBackupSwitchWegameVersion.Text = "备份并切换到WeGame端";
            this.btnBackupSwitchWegameVersion.UseVisualStyleBackColor = true;
            this.btnBackupSwitchWegameVersion.Click += new System.EventHandler(this.btnBackupSwitchWegameVersion_Click);
            // 
            // btnSwitchWegameVersion
            // 
            this.btnSwitchWegameVersion.Location = new System.Drawing.Point(720, 78);
            this.btnSwitchWegameVersion.Margin = new System.Windows.Forms.Padding(4);
            this.btnSwitchWegameVersion.Name = "btnSwitchWegameVersion";
            this.btnSwitchWegameVersion.Size = new System.Drawing.Size(228, 45);
            this.btnSwitchWegameVersion.TabIndex = 9;
            this.btnSwitchWegameVersion.Text = "切换到WeGame端";
            this.btnSwitchWegameVersion.UseVisualStyleBackColor = true;
            this.btnSwitchWegameVersion.Click += new System.EventHandler(this.btnSwitchWegameVersion_Click);
            // 
            // btn_bakcupCurrentVersion
            // 
            this.btn_bakcupCurrentVersion.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_bakcupCurrentVersion.Location = new System.Drawing.Point(12, 131);
            this.btn_bakcupCurrentVersion.Margin = new System.Windows.Forms.Padding(4);
            this.btn_bakcupCurrentVersion.Name = "btn_bakcupCurrentVersion";
            this.btn_bakcupCurrentVersion.Size = new System.Drawing.Size(228, 45);
            this.btn_bakcupCurrentVersion.TabIndex = 6;
            this.btn_bakcupCurrentVersion.Text = "备份当前客户端";
            this.btn_bakcupCurrentVersion.UseVisualStyleBackColor = true;
            this.btn_bakcupCurrentVersion.Click += new System.EventHandler(this.btnBackup_Click);
            // 
            // checkBox_isAutoClose
            // 
            this.checkBox_isAutoClose.AutoSize = true;
            this.checkBox_isAutoClose.Location = new System.Drawing.Point(828, 31);
            this.checkBox_isAutoClose.Name = "checkBox_isAutoClose";
            this.checkBox_isAutoClose.Size = new System.Drawing.Size(178, 22);
            this.checkBox_isAutoClose.TabIndex = 5;
            this.checkBox_isAutoClose.Text = "启动后关闭切换器";
            this.checkBox_isAutoClose.UseVisualStyleBackColor = true;
            // 
            // btnBackupSwitchOfficialVersion
            // 
            this.btnBackupSwitchOfficialVersion.Location = new System.Drawing.Point(248, 131);
            this.btnBackupSwitchOfficialVersion.Margin = new System.Windows.Forms.Padding(4);
            this.btnBackupSwitchOfficialVersion.Name = "btnBackupSwitchOfficialVersion";
            this.btnBackupSwitchOfficialVersion.Size = new System.Drawing.Size(228, 45);
            this.btnBackupSwitchOfficialVersion.TabIndex = 4;
            this.btnBackupSwitchOfficialVersion.Text = "备份并切换到官方端";
            this.btnBackupSwitchOfficialVersion.UseVisualStyleBackColor = true;
            this.btnBackupSwitchOfficialVersion.Click += new System.EventHandler(this.btnBackupSwitchOfficialVersion_Click);
            // 
            // lblCurrentVersion
            // 
            this.lblCurrentVersion.AutoSize = true;
            this.lblCurrentVersion.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblCurrentVersion.ForeColor = System.Drawing.Color.Red;
            this.lblCurrentVersion.Location = new System.Drawing.Point(115, 29);
            this.lblCurrentVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCurrentVersion.Name = "lblCurrentVersion";
            this.lblCurrentVersion.Size = new System.Drawing.Size(142, 21);
            this.lblCurrentVersion.TabIndex = 3;
            this.lblCurrentVersion.Text = "未识别客户端";
            this.lblCurrentVersion.DoubleClick += new System.EventHandler(this.lblCurrentVersion_DoubleClick);
            // 
            // btnSwitchOfficialVersion
            // 
            this.btnSwitchOfficialVersion.Location = new System.Drawing.Point(248, 78);
            this.btnSwitchOfficialVersion.Margin = new System.Windows.Forms.Padding(4);
            this.btnSwitchOfficialVersion.Name = "btnSwitchOfficialVersion";
            this.btnSwitchOfficialVersion.Size = new System.Drawing.Size(228, 45);
            this.btnSwitchOfficialVersion.TabIndex = 2;
            this.btnSwitchOfficialVersion.Text = "切换到官方端";
            this.btnSwitchOfficialVersion.UseVisualStyleBackColor = true;
            this.btnSwitchOfficialVersion.Click += new System.EventHandler(this.btnSwitchOfficialVersion_Click);
            // 
            // btnLaunchCurrent
            // 
            this.btnLaunchCurrent.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnLaunchCurrent.Location = new System.Drawing.Point(12, 78);
            this.btnLaunchCurrent.Margin = new System.Windows.Forms.Padding(4);
            this.btnLaunchCurrent.Name = "btnLaunchCurrent";
            this.btnLaunchCurrent.Size = new System.Drawing.Size(228, 45);
            this.btnLaunchCurrent.TabIndex = 1;
            this.btnLaunchCurrent.Text = "启动当前客户端";
            this.btnLaunchCurrent.UseVisualStyleBackColor = true;
            this.btnLaunchCurrent.Click += new System.EventHandler(this.btnLaunchCurrent_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 32);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(98, 18);
            this.label4.TabIndex = 0;
            this.label4.Text = "当前版本：";
            // 
            // labelTip
            // 
            this.labelTip.AutoSize = true;
            this.labelTip.ForeColor = System.Drawing.Color.DimGray;
            this.labelTip.Location = new System.Drawing.Point(25, 530);
            this.labelTip.Name = "labelTip";
            this.labelTip.Size = new System.Drawing.Size(314, 18);
            this.labelTip.TabIndex = 8;
            this.labelTip.Text = "版本号变化后一定记得备份当前客户端";
            // 
            // linkLabelGitee
            // 
            this.linkLabelGitee.AutoSize = true;
            this.linkLabelGitee.Location = new System.Drawing.Point(952, 530);
            this.linkLabelGitee.Name = "linkLabelGitee";
            this.linkLabelGitee.Size = new System.Drawing.Size(80, 18);
            this.linkLabelGitee.TabIndex = 7;
            this.linkLabelGitee.TabStop = true;
            this.linkLabelGitee.Text = "项目主页";
            this.linkLabelGitee.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelGitee_LinkClicked);
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
            this.statusStrip1.Location = new System.Drawing.Point(0, 558);
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
            this.groupBox1.Size = new System.Drawing.Size(1014, 81);
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
            this.btnScreenShot.Location = new System.Drawing.Point(18, 287);
            this.btnScreenShot.Margin = new System.Windows.Forms.Padding(4);
            this.btnScreenShot.Name = "btnScreenShot";
            this.btnScreenShot.Size = new System.Drawing.Size(120, 34);
            this.btnScreenShot.TabIndex = 5;
            this.btnScreenShot.Text = "截图目录";
            this.btnScreenShot.UseVisualStyleBackColor = true;
            this.btnScreenShot.Click += new System.EventHandler(this.btnScreenShot_Click);
            // 
            // btnCheckUpdate
            // 
            this.btnCheckUpdate.AutoSize = true;
            this.btnCheckUpdate.Location = new System.Drawing.Point(866, 530);
            this.btnCheckUpdate.Name = "btnCheckUpdate";
            this.btnCheckUpdate.Size = new System.Drawing.Size(80, 18);
            this.btnCheckUpdate.TabIndex = 9;
            this.btnCheckUpdate.TabStop = true;
            this.btnCheckUpdate.Text = "检查更新";
            this.btnCheckUpdate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.btnCheckUpdate_LinkClicked);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(146, 287);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(120, 34);
            this.button1.TabIndex = 11;
            this.button1.Text = "切换器目录";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnSwitcherFolder_Click);
            // 
            // btnLoadConfig
            // 
            this.btnLoadConfig.Location = new System.Drawing.Point(808, 287);
            this.btnLoadConfig.Margin = new System.Windows.Forms.Padding(4);
            this.btnLoadConfig.Name = "btnLoadConfig";
            this.btnLoadConfig.Size = new System.Drawing.Size(102, 34);
            this.btnLoadConfig.TabIndex = 16;
            this.btnLoadConfig.Text = "重载配置";
            this.btnLoadConfig.UseVisualStyleBackColor = true;
            this.btnLoadConfig.Click += new System.EventHandler(this.btnLoadConfig_Click);
            // 
            // btnWuWaDiff
            // 
            this.btnWuWaDiff.Location = new System.Drawing.Point(274, 287);
            this.btnWuWaDiff.Margin = new System.Windows.Forms.Padding(4);
            this.btnWuWaDiff.Name = "btnWuWaDiff";
            this.btnWuWaDiff.Size = new System.Drawing.Size(149, 34);
            this.btnWuWaDiff.TabIndex = 17;
            this.btnWuWaDiff.Text = "计算客户端差异";
            this.btnWuWaDiff.UseVisualStyleBackColor = true;
            this.btnWuWaDiff.Click += new System.EventHandler(this.btnWuWaDiff_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1050, 580);
            this.Controls.Add(this.btnWuWaDiff);
            this.Controls.Add(this.btnLoadConfig);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnCheckUpdate);
            this.Controls.Add(this.btnScreenShot);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.linkLabelGitee);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.labelTip);
            this.Controls.Add(this.btnSaveConfig);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "鸣潮官服B服WeGame切换器1.3.1";
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
        private System.Windows.Forms.Button btnBackupSwitchOfficialVersion;
        private System.Windows.Forms.CheckBox checkBox_isAutoClose;
        private System.Windows.Forms.Button btnScreenShot;
        private System.Windows.Forms.Button btn_bakcupCurrentVersion;
        private System.Windows.Forms.Button btnOpenGameFolder;
        private System.Windows.Forms.Button btnLaunchBilibili;
        private System.Windows.Forms.Button btnLaunchOfficial;
        private System.Windows.Forms.LinkLabel linkLabelGitee;
        private System.Windows.Forms.Label labelTip;
        private System.Windows.Forms.Button btnBackupSwitchWegameVersion;
        private System.Windows.Forms.Button btnSwitchWegameVersion;
        private System.Windows.Forms.Button btnBackupSwitchBilibiliVersion;
        private System.Windows.Forms.Button btnSwitchBilibiliVersion;
        private System.Windows.Forms.Button btnLaunchWeGame;
        private System.Windows.Forms.Button btnBrowseWeGame;
        private System.Windows.Forms.TextBox txtWeGamePath;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnCheckGame;
        private System.Windows.Forms.LinkLabel btnCheckUpdate;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnLoadConfig;
        private System.Windows.Forms.Button btnWuWaDiff;
    }
}

