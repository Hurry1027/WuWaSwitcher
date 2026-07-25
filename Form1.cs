using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
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
        private Config config;
        private string configPath = "config.json";
        private ClientType currentClientType = ClientType.Null;
        private string currentSdkVersion = "未知";
        private string currentGameVersion = "未知";
        // private string aimVersion;
        // private string aimVersionName;
        

        public Form1() {
            //this.Icon = Properties.Resources.icon;
            InitializeComponent();
            config = new Config();
            SetDynamicIcon(Assembly.GetExecutingAssembly().Location);
        }

        private async void Form1_Load(object sender, EventArgs e) {
            await Task.Run(() => {
                LoadConfig();
                CheckCurrentVersion();
                CheckForUpdate();
            });
        }

        
        private void LoadConfig() {
            if (File.Exists(configPath)) {
                try {
                    string jsonContent = File.ReadAllText(configPath, Encoding.UTF8);
                    System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    var data = serializer.Deserialize<Dictionary<string, object>>(jsonContent);

                    config = new Config();
                    config.gameFolderPath = data["gameFolderPath"]?.ToString() ?? "";

                    var clientPathsData = data["client_launch_paths"] as Dictionary<string, object>;
                    config.client_launch_paths = new ClientPaths();
                    config.client_launch_paths.Bilibili = clientPathsData != null && clientPathsData.ContainsKey("Bilibili") ? clientPathsData["Bilibili"]?.ToString() ?? "" : "";
                    config.client_launch_paths.Official = clientPathsData != null && clientPathsData.ContainsKey("Official") ? clientPathsData["Official"]?.ToString() ?? "" : "";
                    config.client_launch_paths.WeGame = clientPathsData != null && clientPathsData.ContainsKey("WeGame") ? clientPathsData["WeGame"]?.ToString() ?? "" : "";
                    if (data.ContainsKey("isAutoCloseSwitcher") && data["isAutoCloseSwitcher"] != null) {
                        config.isAutoCloseSwitcher = (bool)data["isAutoCloseSwitcher"];
                    }

                    // 加载备份目录配置
                    if (data.ContainsKey("directoriesToBackup")) {
                        var dirsData = data["directoriesToBackup"] as System.Collections.ArrayList;
                        if (dirsData != null) {
                            config.directoriesToBackup = dirsData.Cast<string>().ToArray();
                        }
                    }

                    // 加载备份文件配置
                    if (data.ContainsKey("filesToBackup")) {
                        var filesData = data["filesToBackup"] as System.Collections.ArrayList;
                        if (filesData != null) {
                            config.filesToBackup = filesData.Cast<string>().ToArray();
                        }
                    }

                    // 加载配置版本
                    if (data.ContainsKey("configVersion") && data["configVersion"] != null) {
                        config.configVersion = data["configVersion"].ToString();
                    }
                    else { 
                        config.configVersion = "1.0.0"; 
                    }

                    // 加载忽略版本号配置
                    if (data.ContainsKey("ignoredVersion") && data["ignoredVersion"] != null) {
                        config.ignoredVersion = data["ignoredVersion"].ToString();
                    }

                    txtGameFolderPath.Text = config.gameFolderPath;
                    txtBilibiliPath.Text = config.client_launch_paths.Bilibili;
                    txtOfficialPath.Text = config.client_launch_paths.Official;
                    txtWeGamePath.Text = config.client_launch_paths.WeGame;
                    checkBox_isAutoClose.Checked = config.isAutoCloseSwitcher;
                    toolStripStatusLabel1.Text = "配置已加载";

                    // 检查并更新配置
                    UpdateConfigToLatestVersion(config);

                    // 检查游戏文件夹有效
                    CheckGameFolder();
                }
                catch (Exception ex) {
                    MessageBox.Show("加载配置文件失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    toolStripStatusLabel1.Text = "加载配置失败";

                    // 使用默认配置
                    config = new Config();
                    config.gameFolderPath = "";
                    config.client_launch_paths.Bilibili = "";
                    config.client_launch_paths.Official = "";
                    config.client_launch_paths.WeGame = "";
                    config.isAutoCloseSwitcher = false;
                    txtGameFolderPath.Text = config.gameFolderPath;
                    txtBilibiliPath.Text = config.client_launch_paths.Bilibili;
                    txtOfficialPath.Text = config.client_launch_paths.Official;
                    txtWeGamePath.Text = config.client_launch_paths.WeGame;
                    checkBox_isAutoClose.Checked = config.isAutoCloseSwitcher;
                }
            } else {
                // 如果配置文件不存在，使用默认配置
                config = new Config();
                config.gameFolderPath = "";
                config.client_launch_paths.Bilibili = "";
                config.client_launch_paths.Official = "";
                config.client_launch_paths.WeGame = "";
                config.isAutoCloseSwitcher = false;
                txtGameFolderPath.Text = config.gameFolderPath;
                txtBilibiliPath.Text = config.client_launch_paths.Bilibili;
                txtOfficialPath.Text = config.client_launch_paths.Official;
                txtWeGamePath.Text = config.client_launch_paths.WeGame;
                checkBox_isAutoClose.Checked = config.isAutoCloseSwitcher;
            }
        }

        private bool SaveConfig() {
            try {
                if (!CheckGameFolder()) { return false; }
                if (txtBilibiliPath.Text != "" && !CheckBilibiliLauncher()) { return false; }
                if (txtOfficialPath.Text != "" && !CheckOfficialLauncher()) { return false; }
                if (txtWeGamePath.Text != "" && !CheckWeGameLauncher()) { return false; }
                _ = Task.Run(async () => await CooldownButton(btnSaveConfig, 3));
                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var data = new Dictionary<string, object> {
                    ["configVersion"] = config.configVersion,
                    ["gameFolderPath"] = config.gameFolderPath,
                    ["isAutoCloseSwitcher"] = config.isAutoCloseSwitcher,
                    ["ignoredVersion"] = config.ignoredVersion,
                    ["client_launch_paths"] = new Dictionary<string, object> {
                        ["Bilibili"] = config.client_launch_paths.Bilibili,
                        ["Official"] = config.client_launch_paths.Official,
                        ["WeGame"] = config.client_launch_paths.WeGame
                    },
                    ["directoriesToBackup"] = config.directoriesToBackup,
                    ["filesToBackup"] = config.filesToBackup
                };

                string json = serializer.Serialize(data);
                File.WriteAllText(configPath, FormatJson(json,4), new UTF8Encoding(false));
                toolStripStatusLabel1.Text = "当前配置已保存到 config.json";
                // 保存配置后重新检查当前版本
                CheckCurrentVersion();
                return true;
            }
            catch (Exception ex) {
                MessageBox.Show("保存配置文件失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = "保存配置失败";
                return false;
            }
        }

        private bool CheckGameFolder() {
            _ = Task.Run(async () => await CooldownButton(btnCheckGame, 3));
            if (txtGameFolderPath.Text == "") {
                MessageBox.Show("请设置游戏文件夹路径\n(WeGame端为Wuthering Waves(2002137))", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                return false;
            }
            
            if (
                !Directory.Exists(txtGameFolderPath.Text) ||
                !File.Exists(Path.Combine(txtGameFolderPath.Text, "Wuthering Waves.exe"))
            ) {
                MessageBox.Show("游戏文件夹不存在: 请选择游戏目录的Wuthering Waves Game文件夹\n(WeGame端为Wuthering Waves(2002137))", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblCurrentVersion.Text = "未设置游戏文件夹路径";
                lblCurrentVersion.ForeColor = Color.Red;
                return false;
            }

            // KrPcSdk符号链接检测
            string krsdkPath = Path.Combine(txtGameFolderPath.Text, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland");
            if (IsSymlink(krsdkPath)) {
                linkMklinkState.Text = "KrPcSdk目录已链接";
                linkMklinkState.LinkColor = Color.Green;

                // 获取符号链接的目标路径
                string linkTarget = GetSymlinkTarget(krsdkPath);
                if (string.IsNullOrEmpty(linkTarget)) {
                    // 无法获取链接目标，可能链接损坏
                    MessageBox.Show("KrPcSdk_Mainland目录无法读取符号链接路径，如果版本显示与预期不一致，建议手动打开目标客户端修复游戏", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    linkMklinkState.Text = "KrPcSdk链接未知";
                    linkMklinkState.LinkColor = Color.Red;
                }
                else if (linkTarget == "Failed Open Dir") {
                    // 目标文件夹不可用，解除链接
                    if (ExecuteCommand($"rd \"{krsdkPath}\"")) {
                        linkMklinkState.Text = "KrPcSdk目录链接解除";
                        linkMklinkState.LinkColor = Color.Firebrick;
                        MessageBox.Show("KrPcSdk_Mainland目录符号链接指向的版本备份不存在，已自动解除链接\n然后你可能需要恢复备份或修复游戏(大约500MB)");
                        return false;
                    }
                    else {
                        linkMklinkState.Text = "解除链接失败";
                        linkMklinkState.LinkColor = Color.Red;
                        MessageBox.Show("KrPcSdk_Mainland目录指向的版本备份不存在，尝试解除链接失败。请手动删除游戏文件夹下的KrPcSdk_Mainland链接目录(图标左下角有角标)\n然后你可能需要恢复备份或修复游戏(大约500MB)");
                        return false;
                    }
                } else {
                    // 符号链接可用，检查链接路径与实际版本是否一致
                    string linkClientType = GetSymlinkClinetType(krsdkPath);
                    if (linkClientType == ClientType.Null) {
                        linkMklinkState.Text = "KrPcSdk目录链接路径错误";
                        linkMklinkState.LinkColor = Color.Red;
                        MessageBox.Show("错误的KrPcSdk目录链接路径, 请手动删除链接: " + linkTarget + " 并恢复KrPcSdk_Mainland目录。\n如果你看到了此错误提示, 请联系开发者反馈此BUG");
                        return false;
                    }
                }
            } else {
                linkMklinkState.Text = "版本文件夹未链接";
                linkMklinkState.LinkColor = Color.Firebrick;
            }
            
            if (!File.Exists(Path.Combine(txtGameFolderPath.Text, "Client\\Binaries\\Win64\\Client-Win64-Shipping.exe"))) {
                MessageBox.Show("游戏文件夹识别错误, 未找到Client-Win64-Shipping.exe", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            config.gameFolderPath = txtGameFolderPath.Text;
            config.isAutoCloseSwitcher = checkBox_isAutoClose.Checked;
            return true;
        }

        private bool CheckBilibiliLauncher() {
            if (txtBilibiliPath.Text == "") {
                MessageBox.Show("请设置B服启动器路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                return false;
            }
            if (!txtBilibiliPath.Text.EndsWith("launcher.exe")) {
                MessageBox.Show("B服启动器路径错误: 请选择客户端安装目录下的launcher.exe", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!File.Exists(txtBilibiliPath.Text)) {
                MessageBox.Show("B服启动器路径错误: 找不到launcher.exe", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            config.client_launch_paths.Bilibili = txtBilibiliPath.Text;
            return true;
        }

        private bool CheckOfficialLauncher() {
            if (txtOfficialPath.Text == "") {
                MessageBox.Show("请设置官服启动器路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                return false;
            }
            if (!txtOfficialPath.Text.EndsWith("launcher.exe")) {
                MessageBox.Show("官服启动器路径错误: 请选择客户端安装目录下的launcher.exe", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!File.Exists(txtOfficialPath.Text)) {
                MessageBox.Show("官服启动器路径错误: 找不到launcher.exe", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            config.client_launch_paths.Official = txtOfficialPath.Text;
            return true;
        }

        private bool CheckWeGameLauncher() {
            if (txtWeGamePath.Text == "") {
                MessageBox.Show("请设置WeGame启动器路径\n(WeGame端在/WeGameApps/rail_apps/Wuthering Waves(2002137)/WeGameLauncher下)", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                return false;
            }
            if (!txtWeGamePath.Text.EndsWith("launcher.exe")) {
                MessageBox.Show("WeGame启动器路径错误: 请选择客户端安装目录下的launcher.exe\n(WeGame端在/WeGameApps/rail_apps/Wuthering Waves(2002137)/WeGameLauncher下)", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!File.Exists(txtWeGamePath.Text)) {
                MessageBox.Show("WeGame启动器路径错误: 找不到launcher.exe\n(WeGame端在/WeGameApps/rail_apps/Wuthering Waves(2002137)/WeGameLauncher下)", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            config.client_launch_paths.WeGame = txtWeGamePath.Text;
            return true;
        }

        private bool CheckCurrentVersion() {
            try {
                lblCurrentVersion.Text = "未识别客户端(SDK版本识别失败)";
                lblCurrentVersion.ForeColor = Color.Red;

                string krsdkPath = Path.Combine(txtGameFolderPath.Text, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland");
                string krsdkConfigPath = Path.Combine(krsdkPath, "KRSDKRes/KRSDKConfig.json");
                string launcherDownloadConfigPath = Path.Combine(txtGameFolderPath.Text, "launcherDownloadConfig.json");
                if (!File.Exists(krsdkConfigPath)) {
                    lblCurrentVersion.Text = "未识别客户端(SDK版本识别失败)";
                    lblCurrentVersion.ForeColor = Color.Red;
                    //SetDynamicIcon(Assembly.GetExecutingAssembly().Location);
                    return false;
                }
                if (File.Exists(launcherDownloadConfigPath)) {
                    string launcherJson = File.ReadAllText(launcherDownloadConfigPath, Encoding.UTF8);
                    var launcherConfig = ParseLauncherConfig(launcherJson);
                    currentGameVersion = launcherConfig.version;
                }else{
                    currentGameVersion = "未知";
                }
                string json = File.ReadAllText(krsdkConfigPath, Encoding.UTF8);
                var krsdkConfig = ParseKRSDKConfig(json);
                if (krsdkConfig.KR_ChannelId == "46") {
                    currentClientType = ClientType.Bilibili;
                    //SetDynamicIcon(txtBilibiliPath.Text);
                }
                else if (krsdkConfig.KR_ChannelId == "19") {
                    currentClientType = ClientType.Official;
                    //SetDynamicIcon(txtOfficialPath.Text);
                }
                else if (krsdkConfig.KR_ChannelId == "167") {
                    currentClientType = ClientType.WeGame;
                    //SetDynamicIcon(txtWeGamePath.Text);
                }
                else {
                    currentClientType = ClientType.Null;
                    lblCurrentVersion.Text = "未能识别渠道ID: " + krsdkConfig.KR_ChannelId.ToString();
                    lblCurrentVersion.ForeColor = Color.Red;
                    //SetDynamicIcon(Assembly.GetExecutingAssembly().Location);
                    return false;
                }
                currentSdkVersion = krsdkConfig.KR_GameVersion;
                lblCurrentVersion.Text = $"{currentClientType} {currentGameVersion} (SDK:{currentSdkVersion})";
                lblCurrentVersion.ForeColor = Color.Green;

                // 检查链接路径是否与实际版本一致
                if (IsSymlink(krsdkPath)) {
                    ClientType linkClientType = GetSymlinkClinetType(krsdkPath);
                    if (currentClientType != ClientType.Null && linkClientType != currentClientType) {
                        // 根据链接路径最终修正意图客户端类型
                        currentClientType = linkClientType;
                        lblCurrentVersion.Text = $"{currentClientType} SDK缺失 | 使用启动器修复文件即可";
                        lblCurrentVersion.ForeColor = Color.Red;
                    }
                }
                return true;
            }
            catch (Exception ex) {
                MessageBox.Show("读取KRSDK或launcherDownloadConfig配置文件失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblCurrentVersion.Text = "读取版本失败";
                lblCurrentVersion.ForeColor = Color.Red;
                return false;
            }
        }



        private KRSDKConfig ParseKRSDKConfig(string json) {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var data = serializer.Deserialize<Dictionary<string, object>>(json);

            return new KRSDKConfig {
                KR_PackageName = data["KR_PackageName"]?.ToString() ?? "",
                KR_GameVersion = data["KR_GameVersion"]?.ToString() ?? "",
                KR_ChannelId = data["KR_ChannelId"]?.ToString() ?? ""
            };
        }

        private LauncherConfig ParseLauncherConfig(string json) {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var data = serializer.Deserialize<Dictionary<string, object>>(json);

            return new LauncherConfig {
                version = data["version"]?.ToString() ?? "",
                reUseVersion = data["reUseVersion"]?.ToString() ?? "",
                state = data["state"]?.ToString() ?? "",
                isPreDownload = data.ContainsKey("isPreDownload") && data["isPreDownload"] is bool ? (bool)data["isPreDownload"] : false,
                appId = data["appId"]?.ToString() ?? ""
            };
        }

        private bool CheckAndCloseProcesses() {
            toolStripStatusLabel1.Text = "正在关闭游戏和启动器..";
            List<ProcessCheckInfo> processesToCheck = new List<ProcessCheckInfo>();
            
            // 添加游戏进程检查
            string gameProcessDirectory = Path.Combine(txtGameFolderPath.Text, "Client\\Binaries\\Win64");
            processesToCheck.Add(new ProcessCheckInfo {
                ExecutableName = "Client-Win64-Shipping.exe",
                Directory = gameProcessDirectory,
                DisplayName = "游戏主进程"
            });

            // 添加启动器进程检查
            List<string> runinglauncherDir = new List<string>();
            List<string> runningLauncherFileName = new List<string>();
            if (txtBilibiliPath.Text != "") { 
                if (!CheckBilibiliLauncher()) { return false; }
                processesToCheck.Add(new ProcessCheckInfo {
                    ExecutableName = "launcher_main.exe",
                    Directory = Path.GetDirectoryName(txtBilibiliPath.Text),
                    DisplayName = "Bilibili启动器"
                });
            }
            if (txtOfficialPath.Text != "") { 
                if (!CheckOfficialLauncher()) { return false; }
                processesToCheck.Add(new ProcessCheckInfo {
                    ExecutableName = "launcher_main.exe",
                    Directory = Path.GetDirectoryName(txtOfficialPath.Text),
                    DisplayName = "官方启动器"
                });
            }
            if (txtWeGamePath.Text != "") { 
                if (!CheckWeGameLauncher()) { return false; }
                processesToCheck.Add(new ProcessCheckInfo {
                    ExecutableName = "launcher.exe",
                    Directory = Path.GetDirectoryName(txtWeGamePath.Text),
                    DisplayName = "WeGame启动器"
                });
            }
            
            // 添加WeGame客户端检查
            processesToCheck.Add(new ProcessCheckInfo {
                ExecutableName = "wegame.exe",
                Directory = null,
                DisplayName = "WeGame客户端"
            });
            
            // 批量检查所有进程
            List<ProcessCheckInfo> runningProcesses = CheckProcessesBatch(processesToCheck);
            
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
                    return false;
                }
                
                // 批量终止进程，优化性能
                List<ProcessCheckInfo> processesToKill = new List<ProcessCheckInfo>();
                
                // 添加要终止的游戏进程
                if (isGameRunning) {
                    processesToKill.Add(new ProcessCheckInfo {
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
                KillProcessesBatch(processesToKill);
                toolStripStatusLabel1.Text = "正在结束游戏进程..";
                System.Threading.Thread.Sleep(2500);
            }
            return true;
        }

        private void LaunchCurrentGame() {
            try {
                if (!CheckGameFolder() || !CheckCurrentVersion()) {
                    return;
                }
                string launchPath;
                if (currentClientType == ClientType.Bilibili) {
                    if (!CheckBilibiliLauncher()) {return;}
                    launchPath = config.client_launch_paths.Bilibili;
                }
                else if (currentClientType == ClientType.Official) {
                    if (!CheckOfficialLauncher()) {return;}
                    launchPath = config.client_launch_paths.Official;
                }
                else if (currentClientType == ClientType.WeGame) {
                    if (!CheckWeGameLauncher()) {return;}
                    launchPath = config.client_launch_paths.WeGame;
                }
                else {
                    MessageBox.Show($"未知的版本: {currentClientType}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Process.Start(launchPath);
                if (checkBox_isAutoClose.Checked) {
                    toolStripStatusLabel1.Text = $"已启动 {currentClientType} 客户端, 2s后关闭切换器...";
                    System.Threading.Tasks.Task.Delay(2500).ContinueWith(t => {
                        this.BeginInvoke(new Action(() => this.Close()));
                    });
                }
            }
            catch (Exception ex) {
                MessageBox.Show($"启动 {currentClientType} 失败\n" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = $"启动 {currentClientType} 失败";
            }
        }

        private void SwitchVersion(ClientType aimClientType, bool isBackup) {
            if (!CheckGameFolder()) { return; }
            if (currentClientType == aimClientType) {
                toolStripStatusLabel1.Text = $"已切换到 {aimClientType} 版本";
                return;
            }

            // 检查并关闭游戏和启动器进程
            if (!CheckAndCloseProcesses()) {
                return;
            }

            try {
                // 切换到目标版本
                toolStripStatusLabel1.Text = $"正在切换版本..";
                
                // 定义路径
                string krsdkPath = Path.Combine(txtGameFolderPath.Text, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland");
                string thirdPartyPath = Path.Combine(txtGameFolderPath.Text, "Client/Binaries/Win64/ThirdParty");
                string currentBuckupKrsdkDir = Path.Combine(currentClientType, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland");
                string targetBuckupKrsdkDir = Path.Combine(aimClientType, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland");

                // 1. 检查当前版本的备份是否存在，若不存在则尝试备份
                if (!Directory.Exists(currentBuckupKrsdkDir) || isBackup) {
                    BackupCurrentVersion();
                }
                
                // 2. 检查目标版本的备份是否存在，若不存在则复制一份当前版本备份
                if (!Directory.Exists(targetBuckupKrsdkDir)) {
                    if (Directory.Exists(currentBuckupKrsdkDir)) {
                        CopyDirectory(currentBuckupKrsdkDir, targetBuckupKrsdkDir);
                    }
                    if (!Directory.Exists(targetBuckupKrsdkDir)) {
                        throw new Exception($"{aimClientType} 备份不存在");
                    }
                }
                
                // 3. 删除当前版本符号链接或实目录
                if (IsSymlink(krsdkPath)) {
                    if (!ExecuteCommand($"rd \"{krsdkPath}\"")) {
                        throw new Exception("删除符号链接失败");
                    }
                } else if (Directory.Exists(krsdkPath)) {
                    // 存在实目录krsdkPath，则重备份并删除实目录
                    if (!BackupCurrentVersion()) {
                        return;
                    }

                    // 备份后会创建指向currentBuckupKrsdkDir的符号链接，需要删除它
                    if (IsSymlink(krsdkPath)) {
                        if (!ExecuteCommand($"rd \"{krsdkPath}\"")) {
                            throw new Exception("删除符号链接失败");
                        }
                    }
                }
                
                // 4. 创建目标版本的符号链接（使用相对路径，在thirdPartyPath目录下执行）
                if (!ExecuteCommand($"mklink /J \"{krsdkPath}\" \"{targetBuckupKrsdkDir}\"")) {
                    throw new Exception("创建符号链接失败");
                }
                
                toolStripStatusLabel1.Text = $"已切换到 {aimClientType} 版本";

                // 更新当前版本信息
                CheckGameFolder();
                CheckCurrentVersion();

                // 启动游戏
                //LaunchCurrentGame();
            
                // MessageBox.Show($"未找到 {aimClientType} 的备份\n请手动启动 {aimClientType} 并修复游戏\n然后再使用此程序备份 {aimClientType}\n(官服->WeGame请参考Readme.md使用说明)", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                // toolStripStatusLabel1.Text = $"切换到 {aimClientType} 失败";
                
            }
            catch (Exception ex) {
                CheckCurrentVersion();
                MessageBox.Show("切换客户端失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = $"切换到 {aimClientType} 失败";
            }
        }

        private bool BackupCurrentVersion() {
            try {
                if (!CheckGameFolder() || !CheckCurrentVersion()) { return false; }
                if (currentClientType.IsNull) {
                    MessageBox.Show($"未识别客户端", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // 定义路径及检查存在
                string krsdkPath = Path.Combine(txtGameFolderPath.Text, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland");
                string thirdPartyPath = Path.Combine(txtGameFolderPath.Text, "Client/Binaries/Win64/ThirdParty");
                string rootBackupPath = currentClientType; // 备份到该目录（当前版本名为目录名）
                string currentBuckupKrsdkDir = Path.Combine(rootBackupPath, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland");
                if (!Directory.Exists(krsdkPath)) {
                    throw new Exception($"未找到 {krsdkPath} 目录");
                }
                if (!Directory.Exists(rootBackupPath)) {
                    Directory.CreateDirectory(rootBackupPath);
                }

                // 备份并建立krsdkPath的链接
                if (!IsSymlink(krsdkPath)) {
                    // 备份所有指定文件和目录
                    foreach (string fileName in config.filesToBackup) {
                        string sourceFile = Path.Combine(txtGameFolderPath.Text, fileName);
                        string destFile = Path.Combine(rootBackupPath, fileName);
                        if (File.Exists(sourceFile)) {
                            File.Copy(sourceFile, destFile, true);
                        }
                    }
                    // 备份目录（包括KrPcSdk_Mainland）
                    foreach (string dirName in config.directoriesToBackup) {
                        string sourceDir = Path.Combine(txtGameFolderPath.Text, dirName);
                        string destDir = Path.Combine(rootBackupPath, dirName);
                        
                        if (Directory.Exists(sourceDir)) {
                            if (Directory.Exists(destDir)) {
                                Directory.Delete(destDir, true);
                            }
                            Directory.CreateDirectory(Path.GetDirectoryName(destDir));
                            CopyDirectory(sourceDir, destDir);
                        }
                    }

                    // 重命名krpcsdk目录
                    string old_krsdkPath = Path.Combine(krsdkPath, $"{krsdkPath}-使用三端切换器前的备份");
                    if (Directory.Exists(old_krsdkPath)) {
                        Directory.Delete(old_krsdkPath, true);
                    }
                    Directory.Move(krsdkPath, old_krsdkPath);
                    
                    // 建立krsdkPath链接（使用相对路径，在thirdPartyPath目录下执行）
                    if (!ExecuteCommand($"mklink /J \"{krsdkPath}\" \"{currentBuckupKrsdkDir}\"")) {
                        throw new Exception("创建符号链接失败");
                    }
                }
                toolStripStatusLabel1.Text = $"已备份 {currentClientType} 版本";
                return true;
            }
            catch (Exception ex) {
                toolStripStatusLabel1.Text = $"备份当前版本 {currentClientType} 失败: ";
                MessageBox.Show($"备份当前版本 {currentClientType} 失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }

    [DataContract]
    public class Config {
        [DataMember]
        public string configVersion { get; set; }
        [DataMember]
        public string gameFolderPath { get; set; }
        [DataMember]
        public ClientPaths client_launch_paths { get; set; }
        [DataMember]
        public bool isAutoCloseSwitcher { get; set; }
        [DataMember]
        public string[] directoriesToBackup { get; set; }
        [DataMember]
        public string[] filesToBackup { get; set; }
        [DataMember]
        public string ignoredVersion { get; set; }
        public Config() {
            configVersion = "1.2.0";
            ignoredVersion = "";
            client_launch_paths = new ClientPaths();
            directoriesToBackup = new string[] { 
                "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland",
                //"Engine",
                //"launcherDownload"
            };
            filesToBackup = new string[] {
                //"ClearThirdParty.exe",
                //"launcherDownloadConfig.json",
                //"LocalGameResources.json",
                //"Wuthering Waves.exe"
            };
        }
    }

    [DataContract]
    public class ClientPaths {
        [DataMember]
        public string Bilibili { get; set; }
        [DataMember]
        public string Official { get; set; }
        [DataMember]
        public string WeGame { get; set; }
    }

    [DataContract]
    public class KRSDKConfig {
        [DataMember]
        public string KR_PackageName { get; set; }
        [DataMember]
        public string KR_GameVersion { get; set; }
        [DataMember]
        public string KR_ChannelId { get; set; }
    }

    public class LauncherConfig {
        [DataMember]
        public string version { get; set; }
        [DataMember]
        public string reUseVersion { get; set; }
        [DataMember]
        public string state { get; set; }
        [DataMember]
        public bool isPreDownload { get; set; }
        [DataMember]
        public string appId { get; set; }
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
