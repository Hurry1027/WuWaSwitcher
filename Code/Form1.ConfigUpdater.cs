using System;
using System.Linq;
using System.Windows.Forms;

namespace WuWaSwitcher {
    public partial class Form1 {
        public void UpdateConfigToLatestVersion(Config config)
        {
            bool isUpdated = false;
            string message = "配置文件已更新(config.json)";
            // 检查是否需要更新配置
            if (string.IsNullOrEmpty(config.ConfigVersion) || IsOlderVersion(config.ConfigVersion, "1.2.0"))
            {
                UpdateConfigToVersion1_2_0(config);
                isUpdated = true;
                message += "\n1.2.0 去除了对launcherDownloadConfig.json和LocalGameResources.json的备份";
            }

            if (string.IsNullOrEmpty(config.ConfigVersion) || IsOlderVersion(config.ConfigVersion, "1.3.0")) {
                UpdateConfigToVersion1_3_0(config);
                isUpdated = true;
                message += "\n1.3.0 增加对launcherDownloadConfig.json和LocalGameResources.json的备份";
            }

            if (string.IsNullOrEmpty(config.ConfigVersion) || IsOlderVersion(config.ConfigVersion, "1.3.1")) {
                UpdateConfigToVersion1_3_1(config);
                isUpdated = true;
                message += "\n1.3.1 增加对新启动器launcherDownloadConfig和launcherDownload文件夹的备份";
            }

            

            // 可以在这里添加更多版本更新逻辑
            // if (IsOlderVersion(config.configVersion, "1.2.0"))
            // {
            //     UpdateConfigToVersion1_2_0(config);
            // }

            if (isUpdated)
            {
                // 保存更新后的配置
                txtGameFolderPath.Text = config.GameFolderPath;
                txtBilibiliPath.Text = config.Client_launch_paths.Bilibili;
                txtOfficialPath.Text = config.Client_launch_paths.Official;
                txtWeGamePath.Text = config.Client_launch_paths.WeGame;
                checkBox_isAutoClose.Checked = config.IsAutoCloseSwitcher;
                SaveConfig();
                MessageBox.Show(message, "配置文件更新", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
        } 

        private bool IsOlderVersion(string currentVersion, string targetVersion)
        {
            try
            {
                Version current = new Version(currentVersion);
                Version target = new Version(targetVersion);
                return current < target;
            }
            catch
            {
                // 如果版本号格式不正确，不要更新
                MessageBox.Show($"配置文件版本号解析失败\ncurrentVersion: {currentVersion}\ntargetVersion: {targetVersion}", "配置文件版本号解析失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void UpdateConfigToVersion1_2_0(Config config) {
            // 移除过时的配置项
            if (config.FilesToBackup != null)
            {
                config.FilesToBackup = config.FilesToBackup
                    .Where(file => file != "launcherDownloadConfig.json" && file != "LocalGameResources.json")
                    .ToArray();
            }
            
            // 更新配置版本
            config.ConfigVersion = "1.2.0";
        }

        private void UpdateConfigToVersion1_3_0(Config config) {
            if (config.FilesToBackup != null) { 
                if (!config.FilesToBackup.Contains("LocalGameResources.json")) {
                    config.FilesToBackup = new[] { "LocalGameResources.json" }.Concat(config.FilesToBackup).ToArray();
                }
                if (!config.FilesToBackup.Contains("launcherDownloadConfig.json")) {
                    config.FilesToBackup = new[] { "launcherDownloadConfig.json" }.Concat(config.FilesToBackup).ToArray();
                }
            }
            config.ConfigVersion = "1.3.0";
        }

        private void UpdateConfigToVersion1_3_1(Config config) {
            if (config.FilesToBackup != null) {
                if (!config.DirectoriesToBackup.Contains("launcherDownload")) {
                    config.DirectoriesToBackup = new[] { "launcherDownload" }.Concat(config.DirectoriesToBackup).ToArray();
                }
                if (!config.DirectoriesToBackup.Contains("launcherDownloadConfig")) {
                    config.DirectoriesToBackup = new[] { "launcherDownloadConfig" }.Concat(config.DirectoriesToBackup).ToArray();
                }
            }
            config.ConfigVersion = "1.3.1";
        }
    }
}