

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
            toolStripStatusLabel1.Text = $"检查到版本: {currentClientType} {currentSdkVersion}";
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
}