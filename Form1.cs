using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

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
                if (txtBilibiliPath.Text == ""){
                    config.client_launch_paths.Bilibili = "";
                }
                if (txtOfficialPath.Text == ""){
                    config.client_launch_paths.Official = "";
                }
                if (txtWeGamePath.Text == ""){
                    config.client_launch_paths.WeGame = "";
                }
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
            string krsdkPath = Path.Combine(txtGameFolderPath.Text, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland");
            if (!Directory.Exists(krsdkPath)) {
                MessageBox.Show("游戏文件夹识别错误, 未找到KrPcSdk_Mainland文件夹", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                string krsdkConfigPath = Path.Combine(txtGameFolderPath.Text, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland/KRSDKRes/KRSDKConfig.json");
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
            bool isCheckCurrentVersion = CheckCurrentVersion(); // 如果未识别则不进行版本判断及备份
            if (currentClientType == aimClientType) {
                toolStripStatusLabel1.Text = $"已切换到 {aimClientType} 版本";
                return;
            }

            // 检查游戏进程和启动器进程是否在运行（批量优化）
            toolStripStatusLabel1.Text = "正在关闭游戏和启动器..";
            List<ProcessCheckInfo> processesToCheck = new List<ProcessCheckInfo>();
            
            // 添加游戏进程检查
            string gameProcessDirectory = Path.Combine(config.gameFolderPath, "Client\\Binaries\\Win64");
            processesToCheck.Add(new ProcessCheckInfo {
                ExecutableName = "Client-Win64-Shipping.exe",
                Directory = gameProcessDirectory,
                DisplayName = "游戏主进程"
            });

            // 添加启动器进程检查
            List<string> runinglauncherDir = new List<string>();
            List<string> runningLauncherFileName = new List<string>();
            if (txtBilibiliPath.Text != "") { 
                if (!CheckBilibiliLauncher()) { return; }
                processesToCheck.Add(new ProcessCheckInfo {
                    ExecutableName = "launcher_main.exe",
                    Directory = Path.GetDirectoryName(txtBilibiliPath.Text),
                    DisplayName = "Bilibili启动器"
                });
            }
            if (txtOfficialPath.Text != "") { 
                if (!CheckOfficialLauncher()) { return; }
                processesToCheck.Add(new ProcessCheckInfo {
                    ExecutableName = "launcher_main.exe",
                    Directory = Path.GetDirectoryName(txtOfficialPath.Text),
                    DisplayName = "官方启动器"
                });
            }
            if (txtWeGamePath.Text != "") { 
                if (!CheckWeGameLauncher()) { return; }
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
                    return;
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
                toolStripStatusLabel1.Text = $"正在切换到 {aimClientType} ..";
            }

            // 定义变量
            string currentPath = currentClientType;
            string targetPath = aimClientType;
            string krsdkPath = Path.Combine(config.gameFolderPath, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland");
            string targetKrsdkConfigPath = Path.Combine(targetPath, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland/KRSDKRes/KRSDKConfig.json");
            
            // 版本号检测
            //if (isCheckCurrentVersion) {
            //    toolStripStatusLabel1.Text = "正在检查版本是否落后..";
            //    bool isKRsdkVersionChange = false;
            //    if (File.Exists(targetKrsdkConfigPath)) {
            //        string json = File.ReadAllText(targetKrsdkConfigPath, Encoding.UTF8);
            //        var krsdkConfig = ParseKRSDKConfig(json);
            //        isKRsdkVersionChange = new Version(krsdkConfig.KR_GameVersion) < new Version(currentSdkVersion);
            //        if (isKRsdkVersionChange) {
            //            DialogResult result = MessageBox.Show($"目标版本({aimClientType})版本号较旧: {krsdkConfig.KR_GameVersion}\n 当前版本号: {currentSdkVersion}\n修改目标版本号后可能不用重新修复游戏\n是否修改目标版本号并备份当前版本（推荐修改）?", "版本更新提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //            if (result == DialogResult.Yes) {
            //                try {
            //                    System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            //                    var data = serializer.Deserialize<Dictionary<string, object>>(json);
            //                    data["KR_GameVersion"] = currentSdkVersion;
            //                    string updatedJson = serializer.Serialize(data);
            //                    updatedJson = FormatJson(updatedJson, 2);
            //                    File.WriteAllText(targetKrsdkConfigPath, updatedJson, new UTF8Encoding(false));
            //                    toolStripStatusLabel1.Text = $"已更新 {aimClientType} 版本号为: {currentSdkVersion}";
            //                    isBackup = true;
            //                }
            //                catch (Exception ex) {
            //                    MessageBox.Show("修改版本号失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //                    return;
            //                }
            //            }
            //        }
            //    }
            //}
            

            try {
                // 备份当前版本
                if (isCheckCurrentVersion) {
                    if (!Directory.Exists(currentPath) || (isBackup && Directory.Exists(krsdkPath))) {
                        if (!BackupCurrentVersion()) {
                            return;
                        }
                    }
                }
                    
                

                // 切换到目标版本
                if (Directory.Exists(targetPath)) {
                    toolStripStatusLabel1.Text = $"正在恢复文件..";
                    // 恢复文件
                    foreach (string fileName in config.filesToBackup) {
                        string sourceFile = Path.Combine(targetPath, fileName);
                        string destFile = Path.Combine(config.gameFolderPath, fileName);
                        if (File.Exists(sourceFile)) {
                            File.Copy(sourceFile, destFile, true);
                        }
                    }
                    
                    // 恢复目录
                    foreach (string dirName in config.directoriesToBackup) {
                        string sourceDir = Path.Combine(targetPath, dirName);
                        string destDir = Path.Combine(config.gameFolderPath, dirName);
                        
                        if (Directory.Exists(sourceDir)) {
                            if (Directory.Exists(destDir)) {
                                Directory.Delete(destDir, true);
                            }
                            Directory.CreateDirectory(Path.GetDirectoryName(destDir));
                            CopyDirectory(sourceDir, destDir);
                        }
                    }
                    
                    toolStripStatusLabel1.Text = $"已切换到 {aimClientType} 版本";

                    // 更新当前版本信息
                    CheckCurrentVersion();

                    // 启动游戏
                    //LaunchCurrentGame();
                }
                else {
                    MessageBox.Show($"未找到 {aimClientType} 的备份\n请手动启动 {aimClientType} 并修复游戏\n然后再使用此程序备份 {aimClientType}\n(官服->WeGame请参考Readme.md使用说明)", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                    toolStripStatusLabel1.Text = $"切换到 {aimClientType} 失败";
                }
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
                // 备份目录的根目录
                if (currentClientType.IsNull) {
                    MessageBox.Show($"未识别客户端", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                string rootBackupPath = currentClientType;

                if (!Directory.Exists(rootBackupPath)) {
                    Directory.CreateDirectory(rootBackupPath);
                }
                // 备份文件
                foreach (string fileName in config.filesToBackup) {
                    string sourceFile = Path.Combine(config.gameFolderPath, fileName);
                    string destFile = Path.Combine(rootBackupPath, fileName);
                    if (File.Exists(sourceFile)) {
                        File.Copy(sourceFile, destFile, true);
                    }
                }
                // 备份目录（包括KrPcSdk_Mainland）
                foreach (string dirName in config.directoriesToBackup) {
                    string sourceDir = Path.Combine(config.gameFolderPath, dirName);
                    string destDir = Path.Combine(rootBackupPath, dirName);
                    
                    if (Directory.Exists(sourceDir)) {
                        if (Directory.Exists(destDir)) {
                            Directory.Delete(destDir, true);
                        }
                        Directory.CreateDirectory(Path.GetDirectoryName(destDir));
                        CopyDirectory(sourceDir, destDir);
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
