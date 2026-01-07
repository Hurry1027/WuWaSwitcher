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
    public partial class Form1 : Form {
        private Config config;
        private string configPath = "config.json";
        private string currentVersion;
        private string currentVersionName;
        private string aimVersion;
        private string aimVersionName;
        private string currentSdkVersion = "";

        // 获取匹配的进程列表（通用方法）
        private IEnumerable<System.Management.ManagementObject> GetMatchingProcesses(string targetExecutable, string targetDirectory, string commandLineContains = null) {
            List<System.Management.ManagementObject> matchingProcesses = new List<System.Management.ManagementObject>();
            
            try {
                // 使用WMI查询获取所有进程信息，避免32/64位进程访问限制
                using (System.Management.ManagementObjectSearcher searcher = 
                    new System.Management.ManagementObjectSearcher("SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process")) {
                    
                    foreach (System.Management.ManagementObject process in searcher.Get()) {
                        try {
                            string executablePath = process["ExecutablePath"]?.ToString();
                            string commandLine = process["CommandLine"]?.ToString();
                            
                            if (executablePath != null) {
                                string processFileName = Path.GetFileName(executablePath);
                                
                                // 检查文件名是否匹配且来自目标目录
                                if (string.Equals(processFileName, targetExecutable, StringComparison.OrdinalIgnoreCase) &&
                                    executablePath.StartsWith(targetDirectory, StringComparison.OrdinalIgnoreCase)) {
                                    
                                    // 如果指定了命令行参数检查，验证命令行
                                    bool matchesCommandLine = (commandLineContains == null);
                                    if (!matchesCommandLine) {
                                        if (commandLine != null && 
                                            commandLine.Contains(commandLineContains)) {
                                            matchesCommandLine = true;
                                        }
                                    }
                                    
                                    // 如果满足所有条件，添加到匹配列表
                                    if (matchesCommandLine) {
                                        matchingProcesses.Add(process);
                                    }
                                }
                            }
                        } catch (Exception) {
                            // 跳过此进程，继续检查其他进程
                            continue;
                        }
                    }
                }
            } catch (Exception ex) {
                //richTextBox1.AppendText($"获取进程列表失败: {ex.Message}\n");
            }
            
            // 逐个返回匹配的进程
            foreach (var process in matchingProcesses) {
                yield return process;
            }
        }
        
        private bool IsProcessRunning(string targetExecutable, string targetDirectory, string commandLineContains = null) {
            try {
                //richTextBox1.AppendText($"检查 {targetExecutable} {targetDirectory}\n");
                
                // 使用通用方法获取匹配的进程列表
                var matchingProcesses = GetMatchingProcesses(targetExecutable, targetDirectory, commandLineContains);
                
                // 遍历匹配的进程
                foreach (var process in matchingProcesses) {
                    string executablePath = process["ExecutablePath"]?.ToString();
                    string processFileName = Path.GetFileName(executablePath);
                    //richTextBox1.AppendText($"{processFileName}: {executablePath}\n");
                    //richTextBox1.AppendText($"发现匹配的文件名: {processFileName}\n");
                    //richTextBox1.AppendText($"来自目标目录: {targetDirectory}\n");
                    
                    // 如果指定了命令行参数检查，显示包含的命令行
                    if (commandLineContains != null) {
                        string commandLine = process["CommandLine"]?.ToString();
                        if (commandLine != null && commandLine.Contains(commandLineContains)) {
                            //richTextBox1.AppendText($"命令行包含: {commandLineContains}\n");
                        }
                    }
                    
                    // 找到匹配的进程，直接返回true
                    return true;
                }
            } catch (Exception ex) {
                //richTextBox1.AppendText($"获取进程列表失败: {ex.Message}\n");
            }
            
            //richTextBox1.AppendText("未找到匹配的进程\n");
            return false;
        }
        
        private void KillProcess(string targetExecutable, string targetDirectory, string commandLineContains = null) {
            try {
                // 使用通用方法获取匹配的进程列表
                var matchingProcesses = GetMatchingProcesses(targetExecutable, targetDirectory, commandLineContains);
                
                // 遍历匹配的进程并结束它们
                foreach (var process in matchingProcesses) {
                    try {
                        string executablePath = process["ExecutablePath"]?.ToString();
                        string processFileName = Path.GetFileName(executablePath);
                        uint processId = (uint)process["ProcessId"];
                        
                        // 结束进程
                        using (Process p = Process.GetProcessById((int)processId)) {
                            p.Kill();
                            p.WaitForExit(1000);
                            //richTextBox1.AppendText($"已终止进程: {processFileName} (ID: {processId})\n");
                        }
                    } catch (Exception) {
                        // 可能没有权限结束进程或进程已退出，跳过
                        continue;
                    }
                }
            } catch (Exception ex) {
                //richTextBox1.AppendText($"获取进程列表失败: {ex.Message}\n");
            }
        }

        public Form1() {
            //this.Icon = Properties.Resources.icon;
            InitializeComponent();
            config = new Config();
            LoadConfig();
            CheckCurrentVersion();
        }

        private string FormatJson(string json, int indentSize = 4) {
            string formatted = "";
            int indent = 0;
            bool inString = false;
            char quoteChar = '"';

            for (int i = 0; i < json.Length; i++) {
                char currentChar = json[i];

                // Handle quote escaping and string state
                if (currentChar == '\\' && i + 1 < json.Length) {
                    formatted += currentChar;
                    formatted += json[++i];
                    continue;
                }

                if (currentChar == quoteChar && !inString) {
                    inString = true;
                    formatted += currentChar;
                    continue;
                }

                if (currentChar == quoteChar && inString) {
                    inString = false;
                    formatted += currentChar;
                    continue;
                }

                if (inString) {
                    formatted += currentChar;
                    continue;
                }

                // Handle formatting based on character
                switch (currentChar) {
                    case '{':
                    case '[':
                        formatted += currentChar;
                        indent += indentSize;
                        formatted += Environment.NewLine + new string(' ', indent);
                        break;
                    case '}':
                    case ']':
                        indent -= indentSize;
                        formatted += Environment.NewLine + new string(' ', indent);
                        formatted += currentChar;
                        break;
                    case ',':
                        formatted += currentChar;
                        formatted += Environment.NewLine + new string(' ', indent);
                        break;
                    case ':':
                        formatted += currentChar + " ";
                        break;
                    case ' ': case '\t': case '\n': case '\r':
                        // Skip existing whitespace
                        break;
                    default:
                        formatted += currentChar;
                        break;
                }
            }

            return formatted;
        }

        private void Form1_Load(object sender, EventArgs e) {
            // 初始化组件
        }

        private void SetDynamicIcon(string clientPath) {
            try {
                // 从配置中获取客户端路径并设置图标
                if (!string.IsNullOrEmpty(clientPath) && File.Exists(clientPath)) {
                    // 提取客户端的图标
                    Icon appIcon = System.Drawing.Icon.ExtractAssociatedIcon(clientPath);
                    this.Icon = appIcon;
                    notifyIcon1.Icon = appIcon;
                }
            }
            catch (Exception ex) {
                // 如果提取图标失败，使用默认图标
                // 这里可以添加日志记录
                System.Diagnostics.Debug.WriteLine("设置动态图标失败: " + ex.Message);
            }
        }

        // 按钮冷却
        private async Task CooldownButton(Button button, int seconds) {
            button.Enabled = false;
            //string originalText = button.Text;
            for (int i = seconds; i > 0; i--) {
                //button.Text = $"{originalText} ({i}s)";
                await Task.Delay(1000);
            }
            //button.Text = originalText;
            button.Enabled = true;
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
                    config.client_launch_paths.Bilibili = clientPathsData?["Bilibili"]?.ToString() ?? "";
                    config.client_launch_paths.Official = clientPathsData?["Official"]?.ToString() ?? "";
                    if (data["isAutoCloseSwitcher"] != null) {
                        config.isAutoCloseSwitcher = (bool)data["isAutoCloseSwitcher"];
                    }

                    // 加载备份目录配置
                    if (data.ContainsKey("directoriesToBackup")) {
                        var dirsData = data["directoriesToBackup"] as System.Collections.ArrayList;
                        if (dirsData != null && dirsData.Count > 0) {
                            config.directoriesToBackup = dirsData.Cast<string>().ToArray();
                        }
                    }

                    // 加载备份文件配置
                    if (data.ContainsKey("filesToBackup")) {
                        var filesData = data["filesToBackup"] as System.Collections.ArrayList;
                        if (filesData != null && filesData.Count > 0) {
                            config.filesToBackup = filesData.Cast<string>().ToArray();
                        }
                    }

                    txtGameFolderPath.Text = config.gameFolderPath;
                    txtBilibiliPath.Text = config.client_launch_paths.Bilibili;
                    txtOfficialPath.Text = config.client_launch_paths.Official;
                    checkBox_isAutoClose.Checked = config.isAutoCloseSwitcher;
                    toolStripStatusLabel1.Text = "配置已加载";
                }
                catch (Exception ex) {
                    MessageBox.Show("加载配置文件失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    toolStripStatusLabel1.Text = "加载配置失败";

                    // 使用默认配置
                    config = new Config();
                    config.gameFolderPath = "";
                    config.client_launch_paths.Bilibili = "";
                    config.client_launch_paths.Official = "";
                }
            }
            else {
                // 默认配置
                config = new Config();
                config.gameFolderPath = "";
                config.client_launch_paths.Bilibili = "";
                config.client_launch_paths.Official = "";
                toolStripStatusLabel1.Text = "未找到配置文件，使用默认配置";
            }
        }

        private void SaveConfig() {
            try {
                if (!CheckTextConfig()) { return; }
                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var data = new Dictionary<string, object> {
                    ["gameFolderPath"] = config.gameFolderPath,
                    ["isAutoCloseSwitcher"] = config.isAutoCloseSwitcher,
                    ["client_launch_paths"] = new Dictionary<string, object> {
                        ["Bilibili"] = config.client_launch_paths.Bilibili,
                        ["Official"] = config.client_launch_paths.Official
                    },
                    ["directoriesToBackup"] = config.directoriesToBackup,
                    ["filesToBackup"] = config.filesToBackup
                };

                string json = serializer.Serialize(data);
                File.WriteAllText(configPath, FormatJson(json,4), new UTF8Encoding(false));
                toolStripStatusLabel1.Text = "当前配置已保存到 config.json";
                _ = Task.Run(async () => await CooldownButton(btnSaveConfig, 3));
                MessageBox.Show("当前配置已保存", "成功", MessageBoxButtons.OK, MessageBoxIcon.None);

                // 保存配置后重新检查当前版本
                CheckCurrentVersion();
            }
            catch (Exception ex) {
                MessageBox.Show("保存配置文件失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = "保存配置失败";
            }
        }

        private bool CheckTextConfig() {
            if (txtGameFolderPath.Text == "") {
                MessageBox.Show("请设置游戏文件夹路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                return false;
            }
            if (txtBilibiliPath.Text == "") {
                MessageBox.Show("请设置B服启动器路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                return false;
            }
            if (txtOfficialPath.Text == "") {
                MessageBox.Show("请设置B服启动器路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                return false;
            }
            if (!txtGameFolderPath.Text.EndsWith("Wuthering Waves Game") || !Directory.Exists(txtGameFolderPath.Text)) {
                MessageBox.Show("游戏文件夹不存在: 请选择游戏目录的Wuthering Waves Game文件夹", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblCurrentVersion.Text = "未设置游戏文件夹路径";
                lblCurrentVersion.ForeColor = Color.Red;
                return false;
            }
            string krsdkPath = Path.Combine(txtGameFolderPath.Text, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland");
            if (!Directory.Exists(krsdkPath)) {
                MessageBox.Show("游戏文件夹识别错误, 未找到KrPcSdk_Mainland文件夹", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!txtBilibiliPath.Text.EndsWith("launcher.exe") || !txtOfficialPath.Text.EndsWith("launcher.exe")) {
                MessageBox.Show("启动器路径错误: 请选择客户端安装目录下的launcher.exe", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!File.Exists(txtBilibiliPath.Text)) {
                MessageBox.Show("B服启动器路径错误: 找不到launcher.exe", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!File.Exists(txtOfficialPath.Text)) {
                MessageBox.Show("官服启动器路径错误: 找不到launcher.exe", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            config.gameFolderPath = txtGameFolderPath.Text;
            config.client_launch_paths.Bilibili = txtBilibiliPath.Text;
            config.client_launch_paths.Official = txtOfficialPath.Text;
            config.isAutoCloseSwitcher = checkBox_isAutoClose.Checked;
            return true;
        }

        private bool CheckCurrentVersion() {
            try {
                string krsdkConfigPath = Path.Combine(txtGameFolderPath.Text, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland/KRSDKRes/KRSDKConfig.json");
                if (!File.Exists(krsdkConfigPath)) {
                    lblCurrentVersion.Text = "未能识别版本";
                    lblCurrentVersion.ForeColor = Color.Red;
                    SetDynamicIcon(Assembly.GetExecutingAssembly().Location);
                    return false;
                }
                string json = File.ReadAllText(krsdkConfigPath, Encoding.UTF8);
                var krsdkConfig = ParseKRSDKConfig(json);
                if (krsdkConfig.KR_PackageName == "com.kurogame.mingchao.bilibili") {
                    currentVersion = "Bilibili";
                    currentVersionName = "Bilibili服";
                    aimVersion = "Official";
                    aimVersionName = "官方服";
                    btnSwitchVersion.Text = "切换到官方服";
                    btnBackupSwitchVersion.Text = "备份并切换到官方服";
                    // 动态设置程序图标
                    SetDynamicIcon(txtBilibiliPath.Text);
                }
                else if (krsdkConfig.KR_PackageName == "com.kurogame.mingchao") {
                    currentVersion = "Official";
                    currentVersionName = "官方服";
                    aimVersion = "Bilibili";
                    aimVersionName = "Bilibili服";
                    btnSwitchVersion.Text = "切换到Bilibili服";
                    btnBackupSwitchVersion.Text = "备份并切换到Bilibili服";
                    // 动态设置程序图标
                    SetDynamicIcon(txtOfficialPath.Text);
                }
                else {
                    lblCurrentVersion.Text = "未能识别版本: " + krsdkConfig.KR_PackageName.ToString();
                    lblCurrentVersion.ForeColor = Color.Red;
                    SetDynamicIcon(Assembly.GetExecutingAssembly().Location);
                    return false;
                }
                currentSdkVersion = krsdkConfig.KR_GameVersion;
                lblCurrentVersion.Text = $"{currentVersionName} {currentSdkVersion}";
                lblCurrentVersion.ForeColor = Color.Green;
                return true;
            }
            catch (Exception ex) {
                MessageBox.Show("读取KRSDK配置文件失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                KR_GameVersion = data["KR_GameVersion"]?.ToString() ?? ""
            };
        }

        private void LaunchCurrentGame() {
            try {
                if (!CheckTextConfig() || !CheckCurrentVersion()) {
                    return;
                }
                string launchPath;
                if (currentVersion == "Bilibili") {
                    launchPath = config.client_launch_paths.Bilibili;
                }
                else if (currentVersion == "Official") {
                    launchPath = config.client_launch_paths.Official;
                }
                else {
                    MessageBox.Show($"未知的版本: {currentVersion}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Process.Start(launchPath);
                if (checkBox_isAutoClose.Checked) {
                    toolStripStatusLabel1.Text = $"已启动 {currentVersionName} 客户端, 2s后关闭切换器...";
                    System.Threading.Tasks.Task.Delay(2500).ContinueWith(t => {
                        this.BeginInvoke(new Action(() => this.Close()));
                    });
                }
            }
            catch (Exception ex) {
                MessageBox.Show($"启动 {currentVersionName} 失败\n" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = $"启动 {currentVersionName} 失败";
            }
        }

        private void SwitchVersion(bool isBackup) {
            // 检查游戏进程是否在运行
            string gameProcessDirectory = Path.Combine(config.gameFolderPath, "Client\\Binaries\\Win64");
            bool isGameRunning = IsProcessRunning("Client-Win64-Shipping.exe", gameProcessDirectory);
            
            // 获取启动器目录
            string launcherDirectory1 = Path.GetDirectoryName(txtBilibiliPath.Text);
            string launcherDirectory2 = Path.GetDirectoryName(txtOfficialPath.Text);
            bool isLauncherRunning = IsProcessRunning("launcher_main.exe", launcherDirectory1) || IsProcessRunning("launcher_main.exe", launcherDirectory2);
            
            if (isGameRunning || isLauncherRunning) {
                StringBuilder message = new StringBuilder("检测到游戏进程正在运行:");
                if (isGameRunning) {
                    message.AppendLine("\nClient-Win64-Shipping.exe (游戏主进程)");
                }
                if (isLauncherRunning) {
                    message.AppendLine("\nlauncher_main.exe (启动器进程)");
                }
                message.AppendLine("\n是否立即结束这些进程并继续？");
                
                DialogResult result = MessageBox.Show(message.ToString(), "请先关闭游戏", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == DialogResult.No) {
                    toolStripStatusLabel1.Text = "版本切换已取消";
                    return;
                }
                
                // 结束进程
                if (isGameRunning) {
                    KillProcess("Client-Win64-Shipping.exe", gameProcessDirectory);
                }
                if (isLauncherRunning) {
                    KillProcess("launcher_main.exe", launcherDirectory1);
                    KillProcess("launcher_main.exe", launcherDirectory2);
                }
                toolStripStatusLabel1.Text = "正在结束游戏进程..";
                // 等待进程完全退出
                System.Threading.Thread.Sleep(500);
                toolStripStatusLabel1.Text = "正在切换版本..";
            }

            if (!CheckTextConfig() || !CheckCurrentVersion()) { return; }
            string krsdkPath = Path.Combine(config.gameFolderPath, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland");
            string currentPath = currentVersion;
            string targetPath = aimVersion;
            bool isKRsdkVersionChange = false;
            string targetKrsdkConfigPath = Path.Combine(targetPath, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland/KRSDKRes/KRSDKConfig.json");
            if (File.Exists(targetKrsdkConfigPath)) {
                // 检测目标版本是否与当前版本号匹配
                string json = File.ReadAllText(targetKrsdkConfigPath, Encoding.UTF8);
                var krsdkConfig = ParseKRSDKConfig(json);
                isKRsdkVersionChange = new Version(krsdkConfig.KR_GameVersion) < new Version(currentSdkVersion);
                if (isKRsdkVersionChange) {
                    DialogResult result = MessageBox.Show($"目标版本({aimVersionName})版本号较旧: {krsdkConfig.KR_GameVersion}\n 当前版本号: {currentSdkVersion}\n修改目标版本号后可能不用重新修复游戏\n是否修改目标版本号并备份当前版本（推荐修改）?", "版本更新提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes) {
                        try {
                            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                            var data = serializer.Deserialize<Dictionary<string, object>>(json);
                            data["KR_GameVersion"] = currentSdkVersion;
                            string updatedJson = serializer.Serialize(data);
                            // 格式化JSON为4个空格缩进
                            updatedJson = FormatJson(updatedJson, 2);
                            File.WriteAllText(targetKrsdkConfigPath, updatedJson, new UTF8Encoding(false));
                            toolStripStatusLabel1.Text = $"已更新 {aimVersionName} 版本号为: {currentSdkVersion}";
                            isBackup = true;
                        } catch (Exception ex) {
                            MessageBox.Show("修改版本号失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
            }
            try {
                // 备份当前版本
                if (!Directory.Exists(currentPath) || (isBackup && Directory.Exists(krsdkPath))) {
                    if (!BackupCurrentVersion()) {
                        return;
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
                    
                    toolStripStatusLabel1.Text = $"已切换到 {aimVersionName} 版本";

                    // 更新当前版本信息
                    CheckCurrentVersion();

                    // 启动游戏
                    //LaunchCurrentGame();
                }
                else {
                    MessageBox.Show($"已备份当前版本, 但未找到 {aimVersionName} 版本的备份\n请手动启动 {aimVersionName} 客户端并修复游戏\n然后再使用此程序备份 {aimVersionName} 版本", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                    toolStripStatusLabel1.Text = $"切换到 {aimVersionName} 失败";
                }
            }
            catch (Exception ex) {
                MessageBox.Show("切换版本失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = $"切换到 {aimVersionName} 失败";
            }
        }

        private bool BackupCurrentVersion() {
            try {
                if (!CheckTextConfig() || !CheckCurrentVersion()) { return false; }
                // 备份目录的根目录
                string rootBackupPath = currentVersion;
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
                toolStripStatusLabel1.Text = $"已备份 {currentVersion} 版本";
                return true;
            }
            catch (Exception ex) {
                toolStripStatusLabel1.Text = $"备份当前版本 {currentVersion} 失败: ";
                MessageBox.Show($"备份当前版本 {currentVersion} 失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void CopyDirectory(string sourceDir, string destDir) {
            // 创建目标目录
            if (!Directory.Exists(destDir)) {
                Directory.CreateDirectory(destDir);
            }

            // 复制文件
            foreach (string file in Directory.GetFiles(sourceDir)) {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            // 递归复制子目录
            foreach (string subDir in Directory.GetDirectories(sourceDir)) {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }




        private void btnBrowseGameFolder_Click(object sender, EventArgs e) {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK) {
                txtGameFolderPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnBrowseBilibili_Click(object sender, EventArgs e) {
            openFileDialog1.Title = "选择Bilibili客户端可执行文件";
            if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                txtBilibiliPath.Text = openFileDialog1.FileName;
            }
        }

        private void btnBrowseOfficial_Click(object sender, EventArgs e) {
            openFileDialog1.Title = "选择Official客户端可执行文件";
            if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                txtOfficialPath.Text = openFileDialog1.FileName;
            }
        }

        private void btnOpenGameFolder_Click(object sender, EventArgs e) {
            try {
                string gameFolderPath = txtGameFolderPath.Text;
                if (string.IsNullOrEmpty(gameFolderPath) || !gameFolderPath.EndsWith("Wuthering Waves Game") || !Directory.Exists(gameFolderPath)) {
                    MessageBox.Show("请先设置正确的游戏文件夹路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                    return;
                }
                Process.Start("explorer.exe", gameFolderPath);
                toolStripStatusLabel1.Text = "已打开游戏文件夹";
            }
            catch (Exception ex) {
                MessageBox.Show("打开游戏文件夹失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = "打开游戏文件夹失败";
            }
        }

        private void btnLaunchBilibili_Click(object sender, EventArgs e) {
            if (File.Exists(txtBilibiliPath.Text)) {
                Process.Start(txtBilibiliPath.Text);
                _ = Task.Run(async () => await CooldownButton(btnLaunchBilibili, 3));
            }
            else {
                MessageBox.Show($"客户端路径不存在\n{txtBilibiliPath.Text}", "错误", MessageBoxButtons.OK, MessageBoxIcon.None);
                toolStripStatusLabel1.Text = "启动失败";
            }
        }

        private void btnLaunchOfficial_Click(object sender, EventArgs e) {
            if (File.Exists(txtOfficialPath.Text)) {
                Process.Start(txtOfficialPath.Text);
                _ = Task.Run(async () => await CooldownButton(btnLaunchOfficial, 3));
            }
            else {
                MessageBox.Show($"客户端路径不存在\n{txtOfficialPath.Text}", "错误", MessageBoxButtons.OK, MessageBoxIcon.None);
                toolStripStatusLabel1.Text = "启动失败";
            }
        }

        private void btnSaveConfig_Click(object sender, EventArgs e) {
            SaveConfig();
        }




        private void btnLaunchCurrent_Click(object sender, EventArgs e) {
            _ = Task.Run(async () => await CooldownButton(btnLaunchCurrent, 3));
            LaunchCurrentGame();
        }

        private void btnBackup_Click(object sender, EventArgs e) {
            _ = Task.Run(async () => await CooldownButton(btn_bakcupCurrentVersion, 5));
            if (BackupCurrentVersion()) {
                MessageBox.Show($"已成功备份当前版本 {currentVersion}", "成功", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
        }

        private void btnSwitchVersion_Click(object sender, EventArgs e) {
            _ = Task.Run(async () => await CooldownButton(btnSwitchVersion, 3));
            if (!CheckTextConfig() || !CheckCurrentVersion()) { return; }
            DialogResult result = MessageBox.Show($"确认要切换到 {aimVersionName} 版本吗？(不备份当前版本)\n当前版本：{currentVersionName}", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes) {
                SwitchVersion(false);
            }
        }

        private void btnBackupSwitchVersion_Click(object sender, EventArgs e) {
            _ = Task.Run(async () => await CooldownButton(btnSwitchVersion, 3));
            if (string.IsNullOrEmpty(currentVersion) || string.IsNullOrEmpty(aimVersion)) {
                MessageBox.Show("请先设置正确的游戏文件夹路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                return;
            }

            DialogResult result = MessageBox.Show($"确认备份当前版本并切换到 {aimVersionName} 版本吗？\n当前版本：{currentVersionName}", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes) {
                SwitchVersion(true);
            }
        }


        private void btnScreenShot_Click(object sender, EventArgs e) {
            try {
                string gameFolderPath = txtGameFolderPath.Text;
                if (string.IsNullOrEmpty(gameFolderPath)) {
                    MessageBox.Show("请先设置游戏文件夹路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                    return;
                }
                string screenShotPath = Path.Combine(gameFolderPath, "Client", "Saved", "ScreenShot");
                if (!Directory.Exists(screenShotPath)) {
                    MessageBox.Show("截图目录不存在\n" + screenShotPath, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Process.Start("explorer.exe", screenShotPath);
                toolStripStatusLabel1.Text = "已打开ScreenShot目录";
            }
            catch (Exception ex) {
                MessageBox.Show("打开ScreenShot目录失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = "打开目录失败";
            }
        }

        private void btnReadme_Click(object sender, EventArgs e) {
            try {
                string readmePath = Path.Combine(Application.StartupPath, "README.md");
                if (File.Exists(readmePath)) {
                    Process.Start(readmePath);
                    toolStripStatusLabel1.Text = "已打开README文件";
                }
                else {
                    MessageBox.Show("README文件不存在\n" + readmePath, "错误", MessageBoxButtons.OK, MessageBoxIcon.None);
                }
            }
            catch (Exception ex) {
                MessageBox.Show("无法打开README文件: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkLabelGitee_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            try {
                Process.Start("https://gitee.com/wn1027/wuwa-switcher");
                toolStripStatusLabel1.Text = "已打开项目主页";
            }
            catch (Exception ex) {
                MessageBox.Show("无法打开链接: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    [DataContract]
    public class Config {
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
        public Config() {
            client_launch_paths = new ClientPaths();
            directoriesToBackup = new string[] { 
                "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland",
                //"Engine",
                //"launcherDownload"
            };
            filesToBackup = new string[] {
                //"ClearThirdParty.exe",
                "launcherDownloadConfig.json",
                "LocalGameResources.json",
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
    }

    [DataContract]
    public class KRSDKConfig {
        [DataMember]
        public string KR_PackageName { get; set; }
        [DataMember]
        public string KR_GameVersion { get; set; }
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
