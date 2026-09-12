

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace WuWaSwitcher {
    public partial class Form1 {
        private void btnSaveConfig_Click(object sender, EventArgs e) {
            if (SaveConfig()) {
                MessageBox.Show("当前配置已保存", "成功", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
        }

        private void btnCheckGame_Click(object sender, EventArgs e) {
            if (!CheckGameFolder()) { return; }
            CheckCurrentVersion();
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
                string krsdkPath = Path.Combine(config.GameFolderPath, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland");
                if (!Directory.Exists(krsdkPath)) {
                    throw new Exception($"未找到KrPcSdk_Mainland文件夹\n请检查游戏文件夹路径是否设置正确");
                }
                string help = "[用户自助功能说明]：\n" +
                    "计算各客户端差异文件，重新选定备份文件，并更新配置文件\n" +
                    "(根据各版本的LocalGameResources.json计算差异，\n并更新Config.json的filesToBackup字段)\n\n" +
                    "一般在客户端小版本更新完，发现切换版本总是需要修复游戏时使用\n\n" +
                    "开始前请先确保\"LocalGameResources.json\"已添加到当前Config.json的filesToBackup中，" +
                    "且对所有客户端完成备份。\n\n" +
                    "现在要开始吗?";
                DialogResult isStart = MessageBox.Show(help, "【计算各客户端差异文件】", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (isStart != DialogResult.Yes) {
                    return;
                }
                _ = Task.Run(async () => await Utils.CooldownButton(btnWuWaDiff, 3));
                ResourceDiff.DiffResult o_b = null;
                ResourceDiff.DiffResult o_w = null;
                ResourceDiff.DiffResult b_w = null;
                if (config.Client_launch_paths.Official != "" && config.Client_launch_paths.Bilibili != "") {
                    o_b = ResourceDiff.Compare(ClientType.Official, ClientType.Bilibili);
                }
                if (config.Client_launch_paths.Official != "" && config.Client_launch_paths.WeGame != "") {
                    o_w = ResourceDiff.Compare(ClientType.Official, ClientType.WeGame);
                }
                if (config.Client_launch_paths.Bilibili != "" && config.Client_launch_paths.WeGame != "") {
                    b_w = ResourceDiff.Compare(ClientType.Bilibili, ClientType.WeGame);
                }
                List<ResourceDiff.ReplaceFileList> replaceFileList = ResourceDiff.getFileToBackupPathList(o_b, o_w, b_w);
                // 备份文件配置去除Sdk添加LocalGameResources.json
                List<ResourceDiff.ReplaceFileList> replaceFileList_noSdk = replaceFileList.Where(item => !item.Dest.Contains("Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland")).ToList();
                List<String> replaceFilesToBackup_noSdk = replaceFileList_noSdk.Select(item => item.Dest).ToList();
                replaceFilesToBackup_noSdk.Sort(StringComparer.OrdinalIgnoreCase);//排序
                replaceFilesToBackup_noSdk.Insert(0, "LocalGameResources.json");//排序完添加到前面
                long totalSize = replaceFileList_noSdk.Sum(r => r.Size) + Utils.GetDirectorySize(krsdkPath);
                string totalSizeFormat = ResourceDiff.FormatSize(totalSize);
                string msg = $"需备份最小文件数:  {replaceFilesToBackup_noSdk.Count}\n预计单客户端备份大小:  {totalSizeFormat}";
                toolStripStatusLabel1.Text = $"计算差异完成: 文件数:{replaceFilesToBackup_noSdk.Count} 单客户端备份大小: {totalSizeFormat}";

                DialogResult result = DialogResult.None;
                if (totalSize <= 7516192768) {
                    if (!Utils.AreStringArraysSetEqual(config.FilesToBackup, replaceFilesToBackup_noSdk.ToArray())) {
                        result = MessageBox.Show("【确认更新配置文件】\n计算客户端差异完成:\n(结果已保存到切换器目录)\n" + msg + "\n是否覆盖更新当前Config.json配置?", "计算客户端差异", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    } else {
                        MessageBox.Show("计算客户端差异完成:\n(结果已保存到切换器目录)\n" + msg + "\n计算出的差异文件与当前备份配置相同,无需覆盖", "计算客户端差异");
                    }
                } else { 
                    MessageBox.Show("计算客户端差异完成:\n(结果已保存到切换器目录)\n" + msg + "\n计算出的备份文件超7GB,已阻止覆盖配置", "计算客户端差异");
                }
                if (result == DialogResult.Yes) {
                    config.FilesToBackup = replaceFilesToBackup_noSdk.ToArray();
                    if (SaveConfig()){
                        MessageBox.Show($"已成功覆盖备份配置(filesToBackup字段)\n请重新对所有客户端进行备份", "Config.json保存配置成功", MessageBoxButtons.OK, MessageBoxIcon.None);
                    }
                }
            }
            catch (Exception ex) {
                toolStripStatusLabel1.Text = "计算差异失败";
                MessageBox.Show("计算差异失败: \n" + ex.Message, "计算客户端差异失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
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