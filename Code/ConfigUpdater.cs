using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WuWaSwitcher {
    public partial class Form1 {
        public void UpdateConfigToLatestVersion(Config config)
        {
            bool isUpdated = false;
            string message = "配置文件已更新(config.json)";
            // 检查是否需要更新配置
            if (string.IsNullOrEmpty(config.configVersion) || IsOlderVersion(config.configVersion, "1.2.0"))
            {
                UpdateConfigToVersion1_2_0(config);
                isUpdated = true;
                message += "\n1.2.0 去除了对launcherDownloadConfig.json和LocalGameResources.json的备份";
            }

            // 可以在这里添加更多版本更新逻辑
            // if (IsOlderVersion(config.configVersion, "1.2.0"))
            // {
            //     UpdateConfigToVersion1_2_0(config);
            // }

            if (isUpdated)
            {
                // 保存更新后的配置
                txtGameFolderPath.Text = config.gameFolderPath;
                txtBilibiliPath.Text = config.client_launch_paths.Bilibili;
                txtOfficialPath.Text = config.client_launch_paths.Official;
                txtWeGamePath.Text = config.client_launch_paths.WeGame;
                checkBox_isAutoClose.Checked = config.isAutoCloseSwitcher;
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
                // 如果版本号格式不正确，认为是旧版本
                return true;
            }
        }

        private void UpdateConfigToVersion1_2_0(Config config)
        {
            // 移除过时的配置项
            if (config.filesToBackup != null)
            {
                config.filesToBackup = config.filesToBackup
                    .Where(file => file != "launcherDownloadConfig.json" && file != "LocalGameResources.json")
                    .ToArray();
            }
            
            // 更新配置版本
            config.configVersion = "1.2.0";
        }
    }
}