

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            _ = Task.Run(async () => await CooldownButton(btnLaunchBilibili, 3));
            Process.Start(txtBilibiliPath.Text);
        }

        private void btnLaunchOfficial_Click(object sender, EventArgs e) {
            if (!CheckOfficialLauncher()) { return; }
            _ = Task.Run(async () => await CooldownButton(btnLaunchOfficial, 3));
            Process.Start(txtOfficialPath.Text);
        }

        private void btnLaunchWeGame_Click(object sender, EventArgs e) {
            if (!CheckWeGameLauncher()) { return; }
            _ = Task.Run(async () => await CooldownButton(btnLaunchWeGame, 3));
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

        private void btnkrSdkDir_Click(object sender, EventArgs e) {
            try {
                string gameFolderPath = txtGameFolderPath.Text;
                if (string.IsNullOrEmpty(gameFolderPath)) {
                    MessageBox.Show("请先设置游戏文件夹路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                    return;
                }
                string krSdkPath = Path.Combine(gameFolderPath, "Client\\Binaries\\Win64\\ThirdParty");
                if (!Directory.Exists(krSdkPath)) {
                    MessageBox.Show("KrPcSdk目录不存在\n" + krSdkPath, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Process.Start("explorer.exe", krSdkPath);
                toolStripStatusLabel1.Text = "已打开KrPcSdk目录";
            }
            catch (Exception ex) {
                MessageBox.Show("打开KrPcSdk目录失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = "打开目录失败";
            }
        }

        private void btnReadme_Click(object sender, EventArgs e) {
            try {
                Process.Start("https://gitee.com/wn1027/wuwa-switcher");
                toolStripStatusLabel1.Text = "已打开项目主页";
            }
            catch (Exception ex) {
                MessageBox.Show("无法打开链接: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //try {
            //    string readmePath = Path.Combine(Application.StartupPath, "README.md");
            //    if (File.Exists(readmePath)) {
            //        Process.Start(readmePath);
            //        toolStripStatusLabel1.Text = "已打开README文件";
            //    }
            //    else {
            //        MessageBox.Show("README文件不存在\n" + readmePath, "错误", MessageBoxButtons.OK, MessageBoxIcon.None);
            //    }
            //}
            //catch (Exception ex) {
            //    MessageBox.Show("无法打开README文件: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
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

        private void linkMklinkState_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            string krsdkPath = Path.Combine(txtGameFolderPath.Text, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland");
            if (!IsSymlink(krsdkPath)) {
                MessageBox.Show($"用途：切换器将该KrPcSdk_Mainland目录转为符号链接形式，以便在不移动文件的情况下切换游戏版本\n具体路径: Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland");
                return;
            }
            DialogResult result = MessageBox.Show($"是否取消KrPcSdk_Mainland目录的符号链接?\n用途：切换器将该普通目录转为符号链接形式，以便在不移动文件的情况下切换游戏版本\n结果：取消符号链接后恢复游戏文件到原本状态", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes) {
                string currentBuckupKrsdkDir = Path.Combine(currentClientType, "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland");
                if (!Directory.Exists(currentBuckupKrsdkDir)) {
                    DialogResult result1 = MessageBox.Show($"未找到当前版本({currentClientType})的备份文件，确认取消符号链接吗？（你需要打开启动器修复文件，预计下载500MB左右）", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result1 == DialogResult.Yes) {
                        if (!ExecuteCommand($"rd \"{krsdkPath}\"")) {
                            MessageBox.Show("删除符号链接失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        MessageBox.Show("已取消KrPcSdk_Mainland目录的符号链接");
                        CheckGameFolder();
                        return;
                    }
                    else {
                        return;
                    }
                }
                if (!ExecuteCommand($"rd \"{krsdkPath}\"")) {
                    MessageBox.Show("删除符号链接失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (Directory.Exists(currentBuckupKrsdkDir)) {
                    CopyDirectory(currentBuckupKrsdkDir, krsdkPath);
                }
                CheckGameFolder();
                MessageBox.Show("已取消KrPcSdk_Mainland目录的符号链接");
            }
        }

        private async void btnCheckUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            btnCheckUpdate.Enabled = false;
            CheckForUpdate(true);
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