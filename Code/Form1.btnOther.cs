

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WuWaSwitcher {
    public partial class Form1 {
        private void btnSaveConfig_Click(object sender, EventArgs e) {
            if (SaveConfig()) {
                MessageBox.Show("当前配置已保存", "成功", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
        }

        private void btnLoadConfig_Click(object sender, EventArgs e) {
            LoadAndDisplayConfig();
            CheckCurrentVersion();
            CheckGameVersionUpdate();
        }

        private void btnCheckGame_Click(object sender, EventArgs e) {
            if (!CheckGameFolder()) { return; }
            CheckCurrentVersion();
            CheckGameVersionUpdate();
            toolStripStatusLabel1.Text = $"检查到版本: {currentClientType} {currentGameVersion} (SDK:{currentSdkVersion})";
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

        private void btnBrowseWeGame_Click(object sender, EventArgs e) {
            openFileDialog1.Title = "选择WeGame客户端可执行文件";
            if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                txtWeGamePath.Text = openFileDialog1.FileName;
            }
        }

        private void btnOpenGameFolder_Click(object sender, EventArgs e) {
            if (!CheckGameFolder()) { return; }
            try {
                string gameFolderPath = txtGameFolderPath.Text;
                Process.Start("explorer.exe", gameFolderPath);
                toolStripStatusLabel1.Text = "已打开游戏文件夹";
            }
            catch (Exception ex) {
                MessageBox.Show("打开游戏文件夹失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = "打开游戏文件夹失败";
            }
        }

        private void btnLaunchBilibili_Click(object sender, EventArgs e) {
            if (!CheckBilibiliLauncher()) { return; }
            _ = Task.Run(async () => await Utils.CooldownButton(btnLaunchBilibili, 3));
            Process.Start(txtBilibiliPath.Text);
        }

        private void btnLaunchOfficial_Click(object sender, EventArgs e) {
            if (!CheckOfficialLauncher()) { return; }
            _ = Task.Run(async () => await Utils.CooldownButton(btnLaunchOfficial, 3));
            Process.Start(txtOfficialPath.Text);
        }

        private void btnLaunchWeGame_Click(object sender, EventArgs e) {
            if (!CheckWeGameLauncher()) { return; }
            _ = Task.Run(async () => await Utils.CooldownButton(btnLaunchWeGame, 3));
            Process.Start(txtWeGamePath.Text);
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

        private void btnSwitcherFolder_Click(object sender, EventArgs e) {
            try {
                Process.Start("explorer.exe", Path.Combine("."));
                toolStripStatusLabel1.Text = "已打开切换器目录";
            }
            catch (Exception ex) {
                MessageBox.Show("打开切换器目录失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = "打开切换器目录失败";
            }
        }

        private void btnWuWaDiff_Click(object sender, EventArgs e) {
            try {
                btnWuWaDiff.Enabled = false;
                DialogResult result = MessageBox.Show(
                    "[功能说明]：\n" +
                    "对比客户端差异重新选定备份文件路径，并更新配置文件。\n" +
                    "可选择 [网络] 或 [本地] 两种模式获取游戏资源列表\n\n"+
                    "[在什么情况下使用]\n" +
                    "一般在客户端小版本更新完，发现切换版本总是需要修复游戏时使用\n" +
                    "如果网络模式下计算出的备份配置失效，可以采用本地模式\n\n" +
                    "请选择模式：\n\n是 = (优先)网络拉取最新资源列表对比差异\n否 = (保底)使用本地LocalGameResources.json对比差异\n取消 = 取消操作",
                    "计算客户端差异",
                    MessageBoxButtons.YesNoCancel,   // 是 / 否 / 取消
                    MessageBoxIcon.Question);
                string help;
                ResourceDiff.Mode mode;
                DialogResult isStart = DialogResult.No;
                string modeStr;
                switch (result) {
                    case DialogResult.Yes:
                        mode = ResourceDiff.Mode.NetWork;
                        modeStr = "网络";
                        help = "[功能说明]：\n" +
                            "网络请求需要几秒时间\n" +
                            "现在要开始计算差异吗?";
                        isStart = MessageBox.Show(help, $"计算客户端差异({modeStr})", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        break;
                    case DialogResult.No:
                        mode = ResourceDiff.Mode.Local;
                        modeStr = "本地";
                        CheckCanWuWaDiffResult checkRes = checkCanWuWaDiffLocal();
                        help = "[功能说明]：\n" +
                            "请先确保\"LocalGameResources.json\"\n已添加到Config.json的filesToBackup中,\n" +
                            "且所有客户端都已完成客户端更新及备份\n" +
                            "（确保备份的LocalGameResources.json都是最新的）\n\n" +
                            "当前所有备份版本:\n" +
                            $"{checkRes.msg}\n\n";
                        if (checkRes.success) {
                            help += "现在要开始计算差异吗?";
                            isStart = MessageBox.Show(help, $"计算客户端差异({modeStr})", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        }
                        else {
                            help += "条件不足无法计算差异";
                            MessageBox.Show(help, $"计算客户端差异({modeStr})", MessageBoxButtons.OK, MessageBoxIcon.None);
                            isStart = DialogResult.No;
                        }
                        break;
                    case DialogResult.Cancel:
                        btnWuWaDiff.Enabled = true;
                        isStart = DialogResult.No;
                        return;
                    default:
                        btnWuWaDiff.Enabled = true;
                        isStart = DialogResult.No;
                        return;
                }
                if (isStart != DialogResult.Yes) {
                    btnWuWaDiff.Enabled = true;
                    return;
                }
                WuWaDiff(mode);
            }
            catch (Exception ex) {
                btnWuWaDiff.Enabled = true;
                toolStripStatusLabel1.Text = "计算差异失败";
                statusStrip1.Refresh();
                MessageBox.Show("计算差异失败: \n" + ex.Message, "计算客户端差异失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class CheckCanWuWaDiffResult {
            public string msg { get; set; }
            public bool success { get; set; }
        }

        private CheckCanWuWaDiffResult checkCanWuWaDiffLocal() {
            List<string> msgList = new List<string>();
            // 检查至少配置了两个客户端
            int setClientNum = 0;
            int backupClientNum = 0;
            bool isSetOfficialClient = !string.IsNullOrWhiteSpace(txtOfficialPath.Text);
            bool isSetBilibiliClient = !string.IsNullOrWhiteSpace(txtBilibiliPath.Text);
            bool isSetWeGameClient = !string.IsNullOrWhiteSpace(txtWeGamePath.Text);
            if (isSetOfficialClient) {
                setClientNum++;
                Config.BackupFolderCheckResult officalVerInfo = CheckBackupFolderVersion(ClientType.Official);
                string officalLocalResourcesPath = Path.Combine(ClientType.Official.ToString(), "LocalGameResources.json");
                string backupStr = File.Exists(officalLocalResourcesPath) ? "已备份" : "未备份";
                backupClientNum += File.Exists(officalLocalResourcesPath) ? 1 : 0;
                msgList.Add($" - {ClientType.Official} {officalVerInfo.gameVersion} (SDK:{officalVerInfo.sdkVersion}) {backupStr}");
            }
            if (isSetBilibiliClient) {
                setClientNum++;
                Config.BackupFolderCheckResult bilibiliVerInfo = CheckBackupFolderVersion(ClientType.Bilibili);
                string bilibiliLocalResourcesPath = Path.Combine(ClientType.Bilibili.ToString(), "LocalGameResources.json");
                string backupStr = File.Exists(bilibiliLocalResourcesPath) ? "已备份" : "未备份";
                backupClientNum += File.Exists(bilibiliLocalResourcesPath) ? 1 : 0;
                msgList.Add($" - {ClientType.Bilibili} {bilibiliVerInfo.gameVersion} (SDK:{bilibiliVerInfo.sdkVersion}) {backupStr}");
            }
            if (isSetWeGameClient) {
                setClientNum++;
                Config.BackupFolderCheckResult wegameVerInfo = CheckBackupFolderVersion(ClientType.WeGame);
                string wegameLocalResourcesPath = Path.Combine(ClientType.WeGame.ToString(), "LocalGameResources.json");
                string backupStr = File.Exists(wegameLocalResourcesPath) ? "已备份" : "未备份";
                backupClientNum += File.Exists(wegameLocalResourcesPath) ? 1 : 0;
                msgList.Add($" - {ClientType.WeGame} {wegameVerInfo.gameVersion} (SDK:{wegameVerInfo.sdkVersion}) {backupStr}");
            }
            if (setClientNum < 2) {
                msgList.Add("请至少配置两个客户端(主界面填入启动器路径)");
                return new CheckCanWuWaDiffResult {
                    msg = string.Join("\n", msgList),
                    success = false
                };
            }
            if (backupClientNum < 2) {
                msgList.Add("请至少完成两个客户端的备份(包含LocalGameResources.json)");
                return new CheckCanWuWaDiffResult {
                    msg = string.Join("\n", msgList),
                    success = false
                };
            }
            return new CheckCanWuWaDiffResult {
                msg = string.Join("\n", msgList),
                success = true
            };
        }

        private bool WuWaDiff(ResourceDiff.Mode mode, bool isTip = true, bool isStrictSaveConfig = false) {
            btnWuWaDiff.Enabled = false;
            string krsdkPath = Path.Combine(txtGameFolderPath.Text, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland");
            if (!Directory.Exists(krsdkPath)) {
                MessageBox.Show("请设置游戏文件夹路径\n(WeGame端为Wuthering Waves(2002137))", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                btnWuWaDiff.Enabled = true;
                return false;
            }

            string modeStr;
            if (mode == ResourceDiff.Mode.Local) {
                modeStr = "本地";
                toolStripStatusLabel1.Text = "正在收集并计算差异...";
                statusStrip1.Refresh();
            }
            else {
                modeStr = "网络";
                toolStripStatusLabel1.Text = "请稍后, 正在拉取资源列表并计算差异...";
                statusStrip1.Refresh();
            }

            try {
                ResourceDiff.DiffResult o_b = null;
                ResourceDiff.DiffResult o_w = null;
                ResourceDiff.DiffResult b_w = null;
                if (mode == ResourceDiff.Mode.NetWork) {
                    o_b = ResourceDiff.Compare(ClientType.Official, ClientType.Bilibili, mode);
                }
                else {
                    CheckCanWuWaDiffResult checkRes = checkCanWuWaDiffLocal();
                    if (!checkRes.success) {
                        throw new Exception(checkRes.msg);
                    }
                    if (txtOfficialPath.Text != "" && txtBilibiliPath.Text != "") {
                        o_b = ResourceDiff.Compare(ClientType.Official, ClientType.Bilibili, mode);
                    }
                    if (txtOfficialPath.Text != "" && txtWeGamePath.Text != "") {
                        o_w = ResourceDiff.Compare(ClientType.Official, ClientType.WeGame, mode);
                    }
                    if (txtBilibiliPath.Text != "" && txtWeGamePath.Text != "") {
                        b_w = ResourceDiff.Compare(ClientType.Bilibili, ClientType.WeGame, mode);
                    }
                }
                if (o_b == null && o_w == null && b_w == null) {
                    throw new Exception($"未能计算差异");
                }
                List<ResourceDiff.ReplaceFileList> replaceFileList = ResourceDiff.getFileToBackupPathList(o_b, o_w, b_w);
                // 备份文件配置去除Sdk添加LocalGameResources.json
                List<ResourceDiff.ReplaceFileList> replaceFileList_noSdk = replaceFileList.Where(item => !item.Dest.Contains("Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland")).ToList();
                List<String> replaceFilesToBackup_noSdk = replaceFileList_noSdk.Select(item => item.Dest).ToList();
                replaceFilesToBackup_noSdk.Sort(StringComparer.OrdinalIgnoreCase);//排序
                replaceFilesToBackup_noSdk.Insert(0, "LocalGameResources.json");//排序完添加到前面
                replaceFilesToBackup_noSdk.Insert(0, "launcherDownloadConfig.json");//排序完添加到前面
                long totalSize = replaceFileList_noSdk.Sum(r => r.Size) + Utils.GetDirectorySize(krsdkPath);
                string totalSizeFormat = ResourceDiff.FormatSize(totalSize);
                string msg = $"备份最小文件数:  {replaceFilesToBackup_noSdk.Count}\n预计单客户端备份大小:  {totalSizeFormat}";
                toolStripStatusLabel1.Text = $"计算差异完成: 文件数:{replaceFilesToBackup_noSdk.Count} 单客户端备份大小: {totalSizeFormat}";
                statusStrip1.Refresh();

                DialogResult result = DialogResult.None;
                if (totalSize <= 7516192768) {
                    if (!Utils.AreStringArraysSetEqual(config.FilesToBackup, replaceFilesToBackup_noSdk.ToArray())) {
                        if (isStrictSaveConfig) {
                            result = DialogResult.Yes;
                        }
                        else {
                            if (isTip) {
                                result = MessageBox.Show("计算客户端差异完成:\n" + msg + "\n备份配置列表与当前配置不同\n\n是否覆盖当前备份配置?", $"计算客户端差异({modeStr})", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            }
                            else {
                                result = DialogResult.No;
                            }
                        }
                        
                    }
                    else {
                        if (isTip) {
                            MessageBox.Show("计算客户端差异完成:\n" + msg + "\n备份配置列表与当前配置相同,无需更新", $"计算客户端差异({modeStr})");
                        }
                        toolStripStatusLabel1.Text += " 一致无需覆盖";
                        statusStrip1.Refresh();
                    }
                }
                else {
                    if (isTip) {
                        MessageBox.Show("计算客户端差异完成:\n(结果已保存到切换器目录)\n" + msg + "\n计算出的备份文件超7GB,已阻止覆盖备份配置", $"计算客户端差异({modeStr})");
                    }
                    toolStripStatusLabel1.Text += " 体积过大已拒绝覆盖备份配置";
                    statusStrip1.Refresh();
                }
                if (result == DialogResult.Yes) {
                    config.FilesToBackup = replaceFilesToBackup_noSdk.ToArray();
                    if (SaveConfig()) {
                        if (isTip) {
                            result = MessageBox.Show($"已成功覆盖备份配置\n请重新对所有客户端进行备份\n\n是否立即备份当前客户端?", "Config.json保存配置成功", MessageBoxButtons.YesNo, MessageBoxIcon.None);
                            if (result == DialogResult.Yes) {
                                _ = Task.Run(async () => await Utils.CooldownButton(btn_bakcupCurrentVersion, 5));
                                bool? backupResult = BackupCurrentVersion();
                                if (backupResult == null) {
                                    MessageBox.Show($"备份已取消: {currentClientType}, 你可以重新备份", "备份已取消", MessageBoxButtons.OK, MessageBoxIcon.None);
                                    btnWuWaDiff.Enabled = true;
                                    return true;
                                }
                                if (BackupCurrentVersion() == true) {
                                    MessageBox.Show($"已成功备份当前版本 {currentClientType}", "成功", MessageBoxButtons.OK, MessageBoxIcon.None);
                                    btnWuWaDiff.Enabled = true;
                                    return true;
                                }
                                else {
                                    //无操作，如果备份失败已经有弹窗提示了
                                }
                            }
                            else {
                                btnWuWaDiff.Enabled = true;
                                return true;
                            }
                        }
                        else {
                            // 无提示时，这里不进行备份
                            btnWuWaDiff.Enabled = true;
                            return true;
                        }
                    }
                }
                btnWuWaDiff.Enabled = true;
                return true;
            }
            catch (Exception ex) {
                btnWuWaDiff.Enabled = true;
                toolStripStatusLabel1.Text = $"计算差异失败({modeStr})";
                statusStrip1.Refresh();
                MessageBox.Show("计算差异失败\n你可以重试或手动添加备份配置(\"filesToBackup\") \n\n" + ex.Message, $"计算客户端差异失败({modeStr})", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
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

        private void linkLabelReadme_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            try {
                Process.Start("https://gitee.com/wn1027/wuwa-switcher");
                toolStripStatusLabel1.Text = "已打开项目主页使用说明";
            }
            catch (Exception ex) {
                MessageBox.Show("无法打开链接: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnCheckUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            btnCheckUpdate.Enabled = false;
            CheckForUpdate(true, true);
            await Task.Delay(3000);
            //只能调用一次，防止触发频率限制
            //btnCheckUpdate.Enabled = true;
        }

        private void lblCurrentVersion_DoubleClick(object sender, EventArgs e) {
            try {
                string launcherDownloadConfigPath = Path.Combine(txtGameFolderPath.Text, "launcherDownloadConfig.json");
                if (File.Exists(launcherDownloadConfigPath)) {
                    Process.Start(launcherDownloadConfigPath);
                    toolStripStatusLabel1.Text = "已打开launcherDownloadConfig.json文件";
                } else {
                    //MessageBox.Show("launcherDownloadConfig.json文件不存在\n" + launcherDownloadConfigPath, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    toolStripStatusLabel1.Text = "launcherDownloadConfig.json文件不存在";
                }
            } catch (Exception ex) {
                MessageBox.Show("打开文件失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = "打开文件失败";
            }
        }

    }
}