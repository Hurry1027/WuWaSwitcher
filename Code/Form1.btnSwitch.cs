
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WuWaSwitcher {
    public partial class Form1 {
        private void btnLaunchCurrent_Click(object sender, EventArgs e) {
            _ = Task.Run(async () => await Utils.CooldownButton(btnLaunchCurrent, 3));
            LaunchCurrentGame();
        }

        private void btnBackup_Click(object sender, EventArgs e) {
            _ = Task.Run(async () => await Utils.CooldownButton(btn_bakcupCurrentVersion, 5));
            if (BackupCurrentVersion()) {
                MessageBox.Show($"已成功备份当前版本 {currentClientType}", "成功", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
        }

        private void btnSwitchOfficialVersion_Click(object sender, EventArgs e) {
            if (!CheckGameFolder()) { return; }
            DialogResult result = MessageBox.Show($"确认要切换到 {ClientType.Official} 版本吗？(不备份当前版本)\n当前版本：{currentClientType}", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes) {
                _ = Task.Run(async () => await Utils.CooldownButton(btnSwitchOfficialVersion, 3));
                SwitchVersion(ClientType.Official, false);
            }
        }

        private void btnBackupSwitchOfficialVersion_Click(object sender, EventArgs e) {
            if (!CheckGameFolder()) { return; }
            DialogResult result = MessageBox.Show($"确认备份当前版本并切换到 {ClientType.Official} 版本吗？\n当前版本：{currentClientType}", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes) {
                _ = Task.Run(async () => await Utils.CooldownButton(btnBackupSwitchOfficialVersion, 3));
                SwitchVersion(ClientType.Official, true);
            }
        }

        private void btnSwitchBilibiliVersion_Click(object sender, EventArgs e) {
            if (!CheckGameFolder()) { return; }
            DialogResult result = MessageBox.Show($"确认要切换到 {ClientType.Bilibili} 版本吗？(不备份当前版本)\n当前版本：{currentClientType}", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes) {
                _ = Task.Run(async () => await Utils.CooldownButton(btnSwitchBilibiliVersion, 3));
                SwitchVersion(ClientType.Bilibili, false);
            }
        }

        private void btnBackupSwitchBilibiliVersion_Click(object sender, EventArgs e) {
            if (!CheckGameFolder()) { return; }
            DialogResult result = MessageBox.Show($"确认备份当前版本并切换到 {ClientType.Bilibili} 版本吗？\n当前版本：{currentClientType}", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes) {
                _ = Task.Run(async () => await Utils.CooldownButton(btnBackupSwitchBilibiliVersion, 3));
                SwitchVersion(ClientType.Bilibili, true);
            }
        }

        private void btnSwitchWegameVersion_Click(object sender, EventArgs e) {
            if (!CheckGameFolder()) { return; }
            DialogResult result = MessageBox.Show($"确认要切换到 {ClientType.WeGame} 版本吗？(不备份当前版本)\n当前版本：{currentClientType}", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes) {
                _ = Task.Run(async () => await Utils.CooldownButton(btnSwitchWegameVersion, 3));
                SwitchVersion(ClientType.WeGame, false);
            }
        }

        private void btnBackupSwitchWegameVersion_Click(object sender, EventArgs e) {
            if (!CheckGameFolder()) { return; }
            DialogResult result = MessageBox.Show($"确认备份当前版本并切换到 {ClientType.WeGame} 版本吗？\n当前版本：{currentClientType}", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes) {
                _ = Task.Run(async () => await Utils.CooldownButton(btnBackupSwitchWegameVersion, 3));
                SwitchVersion(ClientType.WeGame, true);
            }
        }
    }
}