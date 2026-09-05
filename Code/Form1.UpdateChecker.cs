using System;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace WuWaSwitcher {
    public partial class Form1 {
        private const string LATEST_RELEASE_API = "https://gitee.com/api/v5/repos/wn1027/wuwa-switcher/releases/latest";
        public void CheckForUpdate(bool isStrict = false)
        {
            //开发测试
            //return;
            try
            {
                // 获取当前程序版本
                Version assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string currentVersion = $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";
                
                // 从Gitee API获取最新版本信息
                string jsonResponse = GetLatestReleaseInfo();
                if (!string.IsNullOrEmpty(jsonResponse)) {
                    // 解析JSON响应
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    dynamic releaseInfo = serializer.DeserializeObject(jsonResponse);

                    // 提取最新版本号
                    string latestVersion = releaseInfo["tag_name"].ToString().Replace("v", "");
                    string releaseName = releaseInfo["name"].ToString();
                    string releaseNotes = releaseInfo["body"].ToString();

                    // 比较版本
                    if (IsNewerVersion(latestVersion, currentVersion)) {
                        // 检查是否忽略了此版本
                        if (isStrict || config.IgnoredVersion != latestVersion) {
                            // 显示更新提示
                            toolStripStatusLabel1.Text = $"发现新版本: {latestVersion}";
                            ShowUpdateNotification(latestVersion, releaseName, releaseNotes);
                        }
                    }
                    else {
                        toolStripStatusLabel1.Text = $"已是最新版本: {latestVersion}";
                    }
                }
                else {
                    toolStripStatusLabel1.Text = $"暂无法获取更新信息";
                }
            }
            catch (Exception ex)
            {
                // 如果检查更新失败，不影响程序正常运行
                toolStripStatusLabel1.Text = "检查更新失败: " + ex.Message;
                return;
            }
        }
        
        private string GetLatestReleaseInfo()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "WuWaSwitcher");
                    client.Encoding = System.Text.Encoding.UTF8;
                    return client.DownloadString(LATEST_RELEASE_API);
                }
            }
            catch
            {
                return null;
            }
        }
        
        private bool IsNewerVersion(string latestVersion, string currentVersion)
        {
            try
            {
                Version latest = new Version(latestVersion);
                Version current = new Version(currentVersion);
                return latest > current;
            }
            catch
            {
                return false;
            }
        }
        
        private void ShowUpdateNotification(string latestVersion, string releaseName, string releaseNotes)
        {
            string message = string.Format(
                "发现新版本: {0}\n\n版本号: {1}\n更新内容:\n{2}\n\n[是]: 前往下载页面\n[否]: 此版本不再弹出提示\n[取消]: 关闭对话框",
                releaseName, latestVersion, releaseNotes
            );
            
            DialogResult result = MessageBox.Show(
                message, 
                "更新提示", 
                MessageBoxButtons.YesNoCancel, 
                MessageBoxIcon.Information
            );
            
            if (result == DialogResult.Yes)
            {
                // 打开发布页面
                System.Diagnostics.Process.Start("https://gitee.com/wn1027/wuwa-switcher/releases");
            }
            else if (result == DialogResult.No)
            {
                // 忽略此版本
                config.IgnoredVersion = latestVersion;
                SaveConfig();
            }
            // Cancel: 不做任何处理，下次还会提示
        }
    }
}