using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WuWaSwitcher {
    public readonly struct ClientType{
        private readonly string _value;
        public static ClientType Official => new ClientType("官方客户端");
        public static ClientType Bilibili => new ClientType("Bilibili客户端");
        public static ClientType WeGame => new ClientType("WeGame客户端");
        public static ClientType Null => new ClientType("未识别客户端");

        private ClientType(string value) => _value = value;
        
        public override string ToString() => _value;
        public static implicit operator string(ClientType clientType) => clientType._value;
        
        // 实现相等比较方法
        public override bool Equals(object obj) {
            if (obj is ClientType other) {
                return _value == other._value;
            }
            return false;
        }
        
        public override int GetHashCode() {
            return _value?.GetHashCode() ?? 0;
        }
        
        // 实现==和!=运算符
        public static bool operator ==(ClientType left, ClientType right) {
            return left.Equals(right);
        }
        
        public static bool operator !=(ClientType left, ClientType right) {
            return !(left == right);
        }
        
        public bool IsOfficial => this == Official;
        public bool IsBilibili => this == Bilibili;
        public bool IsWeGame => this == WeGame;
        public bool IsNull => this == Null;
    }

    public partial class Form1 : Form {
        public Config config;
        private ClientType currentClientType = ClientType.Null;
        private string currentSdkVersion = "未知";
        private string currentGameVersion = "未知"; //  始终显示当前游戏版本
        private ClientType diffCheckedCurrentClientType = ClientType.Null;
        private string diffCheckedCurrentGameVersion = "未知"; // 检查过游戏版本更新后再同步为当前版本，为未知时需先同步一次当前版本
        // private string aimVersion;
        // private string aimVersionName;

        public Form1() {
            //this.Icon = Properties.Resources.icon;
            InitializeComponent();
            config = new Config();
            this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            //SetDynamicIcon(Assembly.GetExecutingAssembly().Location);
        }

        private async void Form1_Load(object sender, EventArgs e) {
            InitTipTimer();
            await Task.Run(() => {
                LoadAndDisplayConfig();
                CheckCurrentVersion();
                CheckForUpdate();
                InitConfigFilesToBackupNetwork();
                CheckGameVersionUpdate();
            });
        }

        private void LaunchCurrentGame() {
            try {
                if (!CheckGameFolder() || (!CheckCurrentVersion())) { // 触发计算差异则取消启动
                    return;
                }
                string launchPath;
                if (currentClientType == ClientType.Bilibili) {
                    if (!CheckBilibiliLauncher()) {return;}
                    launchPath = config.Client_launch_paths.Bilibili;
                }
                else if (currentClientType == ClientType.Official) {
                    if (!CheckOfficialLauncher()) {return;}
                    launchPath = config.Client_launch_paths.Official;
                }
                else if (currentClientType == ClientType.WeGame) {
                    if (!CheckWeGameLauncher()) {return;}
                    launchPath = config.Client_launch_paths.WeGame;
                }
                else {
                    MessageBox.Show($"未知的版本: {currentClientType}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Process.Start(launchPath);
                if (checkBox_isAutoClose.Checked) {
                    toolStripStatusLabel1.Text = $"已启动 {currentClientType} 客户端, 2s后关闭切换器...";
                    statusStrip1.Refresh();
                    System.Threading.Tasks.Task.Delay(2500).ContinueWith(t => {
                        this.BeginInvoke(new Action(() => this.Close()));
                    });
                }
            }
            catch (Exception ex) {
                MessageBox.Show($"启动 {currentClientType} 失败\n" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = $"启动 {currentClientType} 失败";
                statusStrip1.Refresh();
            }
        }

        private void SwitchVersion(ClientType aimClientType, bool isBackup) {
            if (!CheckGameFolder()) { return; }
            SaveConfig(); // 保存配置后再操作
            InitConfigFilesToBackupNetwork(); // 保存完配置要初始化

            bool isCheckCurrentVersion = CheckCurrentVersion();
            if (CheckGameVersionUpdate()) {
                MessageBox.Show($"切换操作已取消, 你可以尝试再次切换", "切换操作已取消", MessageBoxButtons.OK, MessageBoxIcon.None);
                return; //触发计算差异则不切换
            } 
            if (currentClientType == aimClientType) {
                toolStripStatusLabel1.Text = $"已切换到 {aimClientType} 版本";
                statusStrip1.Refresh();
                return;
            }

            // 检查游戏进程和启动器进程是否在运行（批量优化）
            toolStripStatusLabel1.Text = "正在关闭游戏和启动器..";
            statusStrip1.Refresh();
            List<Utils.ProcessCheckInfo> processesToCheck = new List<Utils.ProcessCheckInfo>();
            
            // 添加游戏进程检查
            string gameProcessDirectory = Path.Combine(config.GameFolderPath, "Client\\Binaries\\Win64");
            processesToCheck.Add(new Utils.ProcessCheckInfo {
                ExecutableName = "Client-Win64-Shipping.exe",
                Directory = gameProcessDirectory,
                DisplayName = "游戏主进程"
            });

            // 添加启动器进程检查
            List<string> runinglauncherDir = new List<string>();
            List<string> runningLauncherFileName = new List<string>();
            if (txtBilibiliPath.Text != "") { 
                if (!CheckBilibiliLauncher()) { return; }
                processesToCheck.Add(new Utils.ProcessCheckInfo {
                    ExecutableName = "launcher_main.exe",
                    Directory = Path.GetDirectoryName(txtBilibiliPath.Text),
                    DisplayName = "Bilibili启动器"
                });
            }
            if (txtOfficialPath.Text != "") { 
                if (!CheckOfficialLauncher()) { return; }
                processesToCheck.Add(new Utils.ProcessCheckInfo {
                    ExecutableName = "launcher_main.exe",
                    Directory = Path.GetDirectoryName(txtOfficialPath.Text),
                    DisplayName = "官方启动器"
                });
            }
            if (txtWeGamePath.Text != "") { 
                if (!CheckWeGameLauncher()) { return; }
                processesToCheck.Add(new Utils.ProcessCheckInfo {
                    ExecutableName = "launcher.exe",
                    Directory = Path.GetDirectoryName(txtWeGamePath.Text),
                    DisplayName = "WeGame启动器"
                });
            }
            
            // 添加WeGame客户端检查
            processesToCheck.Add(new Utils.ProcessCheckInfo {
                ExecutableName = "wegame.exe",
                Directory = null,
                DisplayName = "WeGame客户端"
            });
            
            // 批量检查所有进程
            List<Utils.ProcessCheckInfo> runningProcesses = Utils.CheckProcessesBatch(processesToCheck);
            
            // 准备结果
            bool isGameRunning = runningProcesses.Any(p => p.ExecutableName == "Client-Win64-Shipping.exe");
            bool isLauncherRunning = runningProcesses.Any(p => p.ExecutableName != "Client-Win64-Shipping.exe");
            
            // 收集运行的启动器信息
            foreach (var process in runningProcesses) {
                if (process.ExecutableName != "Client-Win64-Shipping.exe") {
                    runningLauncherFileName.Add(process.ExecutableName);
                    runinglauncherDir.Add(process.Directory ?? "");
                }
            }
            
            if (isGameRunning || isLauncherRunning) {
                StringBuilder message = new StringBuilder("检测到游戏进程正在运行:");
                if (isGameRunning) {
                    message.AppendLine("\nClient-Win64-Shipping.exe (游戏主进程)");
                }
                if (isLauncherRunning) {
                    foreach (string launcherFileName in runningLauncherFileName) {
                        message.AppendLine($"\n{launcherFileName} (启动器进程)");
                    }
                }
                message.AppendLine("\n是否立即结束这些进程并继续？");
                
                DialogResult result = MessageBox.Show(message.ToString(), "请先关闭游戏", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == DialogResult.No) {
                    toolStripStatusLabel1.Text = "版本切换已取消";
                    statusStrip1.Refresh();
                    return;
                }
                
                // 批量终止进程，优化性能
                List<Utils.ProcessCheckInfo> processesToKill = new List<Utils.ProcessCheckInfo>();
                
                // 添加要终止的游戏进程
                if (isGameRunning) {
                    processesToKill.Add(new Utils.ProcessCheckInfo {
                        ExecutableName = "Client-Win64-Shipping.exe",
                        Directory = gameProcessDirectory,
                        DisplayName = "游戏主进程"
                    });
                }
                
                // 添加要终止的启动器进程
                if (isLauncherRunning) {
                    // 添加运行的启动器进程
                    foreach (var process in runningProcesses) {
                        if (process.ExecutableName != "Client-Win64-Shipping.exe") {
                            processesToKill.Add(process);
                        }
                    }
                }
                
                // 执行批量终止
                Utils.KillProcessesBatch(processesToKill);
                toolStripStatusLabel1.Text = "正在结束游戏进程..";
                statusStrip1.Refresh();
                System.Threading.Thread.Sleep(2500);
                toolStripStatusLabel1.Text = $"正在切换到 {aimClientType} ..";
                statusStrip1.Refresh();
            }

            // 定义变量
            string currentPath = currentClientType;
            string targetPath = aimClientType;
            string krsdkPath = Path.Combine(config.GameFolderPath, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland");
            string targetKrsdkConfigPath = Path.Combine(targetPath, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland/KRSDKRes/KRSDKConfig.json");
            
            try {
                // 备份当前版本
                // 无备份则强制备份
                if (!Directory.Exists(currentPath)) {
                    if (isCheckCurrentVersion == true) {
                        BackupCurrentVersion();
                    }
                }
                // 参数要求备份
                if (isBackup) {
                    bool? backupResult = BackupCurrentVersion();
                    if (backupResult == null) {
                        MessageBox.Show($"备份按钮操作已取消 {currentClientType}, 你可以尝试再次备份", "备份已取消", MessageBoxButtons.OK, MessageBoxIcon.None);
                        return;
                    }
                    if (backupResult == false) {
                        return; // 备份失败终止切换版本
                    }
                }

                // 切换到目标版本
                if (Directory.Exists(targetPath)) {
                    toolStripStatusLabel1.Text = $"正在恢复文件..";
                    statusStrip1.Refresh();
                    int skippedFiles = 0;
                    int skippedDirs = 0;
                    // 恢复文件
                    foreach (string fileName in config.FilesToBackup) {
                        string sourceFile = Path.Combine(targetPath, fileName);
                        string destFile = Path.Combine(config.GameFolderPath, fileName);
                        if (File.Exists(sourceFile)) {
                            Directory.CreateDirectory(Path.GetDirectoryName(destFile));
                            File.Copy(sourceFile, destFile, true);
                        }
                        else {
                            skippedFiles++;
                        }
                    }
                    
                    // 恢复目录
                    foreach (string dirName in config.DirectoriesToBackup) {
                        string sourceDir = Path.Combine(targetPath, dirName);
                        string destDir = Path.Combine(config.GameFolderPath, dirName);
                        
                        if (Directory.Exists(sourceDir)) {
                            if (Directory.Exists(destDir)) {
                                Directory.Delete(destDir, true);
                            }
                            Directory.CreateDirectory(Path.GetDirectoryName(destDir));
                            Utils.CopyDirectory(sourceDir, destDir);
                        }
                        else {
                            skippedDirs++;
                        }
                    }
                    
                    string skipInfo = "";
                    if (skippedFiles > 0 || skippedDirs > 0) {
                        skipInfo = string.Format(" (跳过 {0} 个文件, {1} 个目录)", skippedFiles, skippedDirs);
                    }
                    toolStripStatusLabel1.Text = string.Format("已切换到 {0} 版本{1}", aimClientType, skipInfo);
                    statusStrip1.Refresh();

                    // 更新当前版本信息
                    CheckCurrentVersion();
                    CheckGameVersionUpdate();
                    // 启动游戏
                    //LaunchCurrentGame();
                }
                else {
                    MessageBox.Show($"{aimClientType} 还未完成备份\n请手动启动 {aimClientType} 并修复游戏\n然后再使用此程序进行备份 {aimClientType}\n(官服->WeGame手动切换请参考项目主页使用说明)", "手动切换提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                    toolStripStatusLabel1.Text = $"切换到 {aimClientType} 失败";
                    statusStrip1.Refresh();
                }
            }
            catch (Exception ex) {
                CheckCurrentVersion();
                CheckGameVersionUpdate();
                toolStripStatusLabel1.Text = $"切换到 {aimClientType} 失败";
                statusStrip1.Refresh();
                MessageBox.Show("切换客户端失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // null表示中断操作
        private bool? BackupCurrentVersion() {
            try {
                if (!CheckGameFolder() || !CheckCurrentVersion()) { return false; }
                if (CheckGameVersionUpdate()) { return null; }  // 触发计算差异中断操作
                SaveConfig();                       // 保存配置后再操作
                InitConfigFilesToBackupNetwork();   // 初始化备份配置

                int skipFileNum = 0;
                int skipFolderNum = 0;
                // 备份目录的根目录
                if (currentClientType.IsNull) {
                    MessageBox.Show($"未识别客户端", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                toolStripStatusLabel1.Text = $"正在备份 {currentClientType}...";
                statusStrip1.Refresh();

                string rootBackupPath = currentClientType;

                // 清空旧备份，保证目录干净
                if (Directory.Exists(rootBackupPath)) {
                    Directory.Delete(rootBackupPath, true);
                }
                Directory.CreateDirectory(rootBackupPath);

                // 备份文件
                foreach (string fileName in config.FilesToBackup) {
                    string sourceFile = Path.Combine(config.GameFolderPath, fileName);
                    string destFile = Path.Combine(rootBackupPath, fileName);
                    if (File.Exists(sourceFile)) {
                        Directory.CreateDirectory(Path.GetDirectoryName(destFile));
                        File.Copy(sourceFile, destFile, true);
                    }
                    else {
                        skipFileNum++;
                    }
                }
                // 备份目录（包括KrPcSdk_Mainland）
                foreach (string dirName in config.DirectoriesToBackup) {
                    string sourceDir = Path.Combine(config.GameFolderPath, dirName);
                    string destDir = Path.Combine(rootBackupPath, dirName);

                    if (Directory.Exists(sourceDir)) {
                        Directory.CreateDirectory(Path.GetDirectoryName(destDir));
                        Utils.CopyDirectory(sourceDir, destDir);
                    }
                    else {
                        skipFolderNum++;
                    }
                }
                long backupSize = Utils.GetDirectorySize(rootBackupPath);
                string skipTipStr = "";
                var skipParts = new List<string>();
                if (skipFileNum > 0) { skipParts.Add($"{skipFileNum} 个文件"); }
                if (skipFolderNum > 0) { skipParts.Add($"{skipFolderNum} 个目录"); }
                if (skipParts.Count > 0) { skipTipStr = " " + string.Join("、", skipParts) + "不存在"; }
                toolStripStatusLabel1.Text = "已备份 " + currentClientType + " 版本 (大小: " + Utils.FormatFileSize(backupSize) + ")"+skipTipStr;
                statusStrip1.Refresh();
                return true;
            }
            catch (Exception ex) {
                toolStripStatusLabel1.Text = $"备份当前版本 {currentClientType} 失败: ";
                statusStrip1.Refresh();
                MessageBox.Show($"备份当前版本 {currentClientType} 失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }


        // 循环小提示
        private readonly string[] _tips = new[]
        {
            "[客户端更新后] 版本号变化后记得[备份当前客户端]并[计算客户端差异]",
            "[提示修复游戏] 尝试双击版本号修改为上一个小版本(3.6.1->3.6.0)然后重启客户端",
            "[切换器失效] 打开项目主页可查看[最新公告]及[使用说明]，有问题可提交issue或B站私信",
            "[游戏包体下载] 本程序不下载游戏资源，使用启动器修复游戏更加稳定",
            "[鸣潮帧率翻倍] 打开NVIDIA APP的AI插帧并在NVIDIA APP启动一次鸣潮(刚启动会卡一会)",
            "[限制游戏帧率] 在NVIDIA APP设置限制帧率(有时需要设置为目标帧率的一半)"
        };
        private int _tipIndex;
        private System.Windows.Forms.Timer _tipTimer;

        private void InitTipTimer() {
            if (_tips.Length == 0) return;

            _tipTimer = new System.Windows.Forms.Timer { Interval = 5000 };
            _tipTimer.Tick += (s, e) =>
            {
                _tipIndex = (_tipIndex + 1) % _tips.Length;
                labelTip.Text = _tips[_tipIndex];
            };

            _tipIndex = 0;
            labelTip.Text = _tips[0];
            _tipTimer.Start();

            labelTip.MouseEnter += (s, e) => _tipTimer.Stop();   // 暂停，文本保持
            labelTip.MouseLeave += (s, e) => _tipTimer.Start();  // 恢复
        }
    }
}  


//private void btnRefreshIcon_Click(object sender, EventArgs e)
//{
//    try
//    {
//        SetDynamicIcon();
//        toolStripStatusLabel1.Text = "图标已更新";
//    }
//    catch (Exception ex)
//    {
//        MessageBox.Show("更新图标失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
//    }
//}
