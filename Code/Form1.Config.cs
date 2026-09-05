using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WuWaSwitcher {
    public partial class Form1 {
        public void LoadAndDisplayConfig() {
            try {
                config.Load("config.json");
            }
            catch (Exception ex) {
                MessageBox.Show("加载配置文件失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = "加载配置失败";
                config.GameFolderPath = "";
                config.Client_launch_paths.Bilibili = "";
                config.Client_launch_paths.Official = "";
                config.Client_launch_paths.WeGame = "";
                config.IsAutoCloseSwitcher = false;
                txtGameFolderPath.Text = config.GameFolderPath;
                txtBilibiliPath.Text = config.Client_launch_paths.Bilibili;
                txtOfficialPath.Text = config.Client_launch_paths.Official;
                txtWeGamePath.Text = config.Client_launch_paths.WeGame;
                checkBox_isAutoClose.Checked = config.IsAutoCloseSwitcher;
                return;
            }
            txtGameFolderPath.Text = config.GameFolderPath;
            txtBilibiliPath.Text = config.Client_launch_paths.Bilibili;
            txtOfficialPath.Text = config.Client_launch_paths.Official;
            txtWeGamePath.Text = config.Client_launch_paths.WeGame;
            checkBox_isAutoClose.Checked = config.IsAutoCloseSwitcher;
            toolStripStatusLabel1.Text = "配置已加载";
            // 检查并更新配置
            UpdateConfigToLatestVersion(config);
        }

        public bool SaveConfig() {
            if (!CheckGameFolder()) { return false; }
            if (txtBilibiliPath.Text != "" && !CheckBilibiliLauncher()) { return false; }
            if (txtOfficialPath.Text != "" && !CheckOfficialLauncher()) { return false; }
            if (txtWeGamePath.Text != "" && !CheckWeGameLauncher()) { return false; }
            if (txtBilibiliPath.Text == "") {
                config.Client_launch_paths.Bilibili = "";
            }
            if (txtOfficialPath.Text == "") {
                config.Client_launch_paths.Official = "";
            }
            if (txtWeGamePath.Text == "") {
                config.Client_launch_paths.WeGame = "";
            }
            _ = Task.Run(async () => await Utils.CooldownButton(btnSaveConfig, 3));

            try {
                config.Save("config.json");
            }
            catch (Exception ex) {
                MessageBox.Show("保存配置文件失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = "保存配置失败";
                return false;
            }

            toolStripStatusLabel1.Text = "当前配置已保存到 json";
            // 保存配置后重新检查当前版本
            CheckCurrentVersion();
            return true;
        }

        public bool CheckGameFolder() {
            _ = Task.Run(async () => await Utils.CooldownButton(btnCheckGame, 3));
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
            config.GameFolderPath = txtGameFolderPath.Text;
            config.IsAutoCloseSwitcher = checkBox_isAutoClose.Checked;
            return true;
        }

        public bool CheckBilibiliLauncher() {
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
            config.Client_launch_paths.Bilibili = txtBilibiliPath.Text;
            return true;
        }

        public bool CheckOfficialLauncher() {
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
            config.Client_launch_paths.Official = txtOfficialPath.Text;
            return true;
        }

        public bool CheckWeGameLauncher() {
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
            config.Client_launch_paths.WeGame = txtWeGamePath.Text;
            return true;
        }

        public bool CheckCurrentVersion() {
            try {
                string krsdkConfigPath = Path.Combine(txtGameFolderPath.Text, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland/KRSDKRes/KRSDKConfig.json");
                string launcherDownloadConfigPath = Path.Combine(txtGameFolderPath.Text, "launcherDownloadConfig.json");
                // 检查SDK配置文件是否存在
                if (!File.Exists(krsdkConfigPath)) {
                    lblCurrentVersion.Text = "未识别客户端(SDK版本识别失败)";
                    lblCurrentVersion.ForeColor = Color.Red;
                    //SetDynamicIcon(Assembly.GetExecutingAssembly().Location);
                    return false;
                }
                // 检查launcherDownloadConfig.json, 读取版本号
                if (File.Exists(launcherDownloadConfigPath)) {
                    string launcherJson = File.ReadAllText(launcherDownloadConfigPath, Encoding.UTF8);
                    var launcherConfig = config.ParseLauncherConfig(launcherJson);
                    currentGameVersion = launcherConfig.Version;
                }
                else {
                    currentGameVersion = "未知";
                }
                // 识别SDK渠道ID
                string json = File.ReadAllText(krsdkConfigPath, Encoding.UTF8);
                var krsdkConfig = config.ParseKRSDKConfig(json);
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
    }
}