


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WuWaSwitcher {
    public partial class Form1 {

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

        // 定义要检查的进程信息结构
        public struct ProcessCheckInfo {
            public string ExecutableName { get; set; }
            public string Directory { get; set; }
            public string DisplayName { get; set; } // 用于结果显示
        }

        // 批量检查进程是否运行的优化方法
        private List<ProcessCheckInfo> CheckProcessesBatch(List<ProcessCheckInfo> processesToCheck) {
            List<ProcessCheckInfo> runningProcesses = new List<ProcessCheckInfo>();
            
            // 优化：将进程按文件名分组，加速查找
            Dictionary<string, List<ProcessCheckInfo>> processesByFileName = new Dictionary<string, List<ProcessCheckInfo>>(StringComparer.OrdinalIgnoreCase);
            foreach (var processToCheck in processesToCheck) {
                if (!processesByFileName.ContainsKey(processToCheck.ExecutableName)) {
                    processesByFileName[processToCheck.ExecutableName] = new List<ProcessCheckInfo>();
                }
                processesByFileName[processToCheck.ExecutableName].Add(processToCheck);
            }
            
            try {
                // 只执行一次WMI查询，获取所有进程信息
                using (System.Management.ManagementObjectSearcher searcher = 
                    new System.Management.ManagementObjectSearcher("SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process")) {
                    
                    // 遍历所有进程
                    foreach (System.Management.ManagementObject process in searcher.Get()) {
                        try {
                            string executablePath = process["ExecutablePath"]?.ToString();
                            
                            if (executablePath != null) {
                                string processFileName = Path.GetFileName(executablePath);
                                
                                // 检查这个进程是否匹配任何要检查的进程
                                if (processesByFileName.TryGetValue(processFileName, out List<ProcessCheckInfo> matchingProcesses)) {
                                    // 检查目录是否匹配（如果指定了目录）
                                    foreach (var processToCheck in matchingProcesses) {
                                        if (processToCheck.Directory == null || 
                                            executablePath.StartsWith(processToCheck.Directory, StringComparison.OrdinalIgnoreCase)) {
                                            // 如果匹配，添加到运行进程列表
                                            runningProcesses.Add(processToCheck);
                                        }
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
            
            return runningProcesses;
        }
        
        // 批量终止进程的优化方法
        private void KillProcessesBatch(List<ProcessCheckInfo> processesToKill) {
            try {
                // 将进程按文件名分组，加速查找
                Dictionary<string, List<ProcessCheckInfo>> processesByFileName = new Dictionary<string, List<ProcessCheckInfo>>(StringComparer.OrdinalIgnoreCase);
                foreach (var processToKill in processesToKill) {
                    if (!processesByFileName.ContainsKey(processToKill.ExecutableName)) {
                        processesByFileName[processToKill.ExecutableName] = new List<ProcessCheckInfo>();
                    }
                    processesByFileName[processToKill.ExecutableName].Add(processToKill);
                }
                
                // 只执行一次WMI查询，获取所有进程信息
                using (System.Management.ManagementObjectSearcher searcher = 
                    new System.Management.ManagementObjectSearcher("SELECT ProcessId, ExecutablePath FROM Win32_Process")) {
                    
                    // 遍历所有进程
                    foreach (System.Management.ManagementObject process in searcher.Get()) {
                        try {
                            string executablePath = process["ExecutablePath"]?.ToString();
                            
                            if (executablePath != null) {
                                string processFileName = Path.GetFileName(executablePath);
                                
                                // 检查这个进程是否匹配任何要终止的进程
                                if (processesByFileName.TryGetValue(processFileName, out List<ProcessCheckInfo> matchingProcesses)) {
                                    foreach (var processToKill in matchingProcesses) {
                                        if (processToKill.Directory == null || 
                                            executablePath.StartsWith(processToKill.Directory, StringComparison.OrdinalIgnoreCase)) {
                                            // 结束进程
                                            uint processId = (uint)process["ProcessId"];
                                            using (Process p = Process.GetProcessById((int)processId)) {
                                                p.Kill();
                                                p.WaitForExit(1000);
                                            }
                                            break; // 已经找到并终止，跳出循环
                                        }
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

        private bool ExecuteCommand(string command, string workingDirectory = null) {
            try {
                ProcessStartInfo startInfo = new ProcessStartInfo {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (Process process = Process.Start(startInfo)) {
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            } catch (Exception ex) {
                MessageBox.Show("执行命令失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool IsSymlink(string path) {
            try {
                FileAttributes attributes = File.GetAttributes(path);
                return (attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
            } catch {
                return false;
            }
        }

        private ClientType GetSymlinkClinetType(string path) {
            if (IsSymlink(path)){
                string linkTarget = GetSymlinkTarget(path);
                if (string.IsNullOrEmpty(linkTarget)) {
                    return ClientType.Null;
                } else if (linkTarget == "Failed Open Dir") {
                    return ClientType.Null;
                }else {
                    string linkClientType;
                    const string suffix = @"\Client\Binaries\Win64\ThirdParty\KrPcSdk_Mainland";
                    if (linkTarget.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) {
                        int suffixStart = linkTarget.Length - suffix.Length;
                        string prefix = linkTarget.Substring(0, suffixStart);
                        linkClientType = Path.GetFileName(prefix);
                    }else {
                        linkClientType = "";
                    }
                    if (linkClientType == "官方客户端") {
                        return ClientType.Official;
                    }
                    else if (linkClientType == "Bilibili客户端") {
                        return ClientType.Bilibili;
                    }
                    else if (linkClientType == "WeGame客户端") {
                        return ClientType.WeGame;
                    }
                    else {
                        return ClientType.Null;
                    }
                }
            } else {
                return ClientType.Null;
            }
        }


        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint GetFinalPathNameByHandle(
            IntPtr hFile,
            [Out] StringBuilder lpszFilePath,
            uint cchFilePath,
            uint dwFlags
        );

        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint FILE_SHARE_DELETE = 0x00000004;
        private const uint OPEN_EXISTING = 3;
        private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
        private const uint VOLUME_NAME_DOS = 0x0;

        public string GetSymlinkTarget(string symlinkPath) {
            IntPtr handle = IntPtr.Zero;
            try {
                // 打开目录（需要使用FILE_FLAG_BACKUP_SEMANTICS）
                handle = CreateFile(
                    symlinkPath,
                    0, // 不需要读取权限
                    FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE,
                    IntPtr.Zero,
                    OPEN_EXISTING,
                    FILE_FLAG_BACKUP_SEMANTICS,
                    IntPtr.Zero
                );

                if (handle == new IntPtr(-1)) {
                    return "Failed Open Dir";
                }

                // 分配缓冲区
                StringBuilder sb = new StringBuilder(1024);
                uint result = GetFinalPathNameByHandle(handle, sb, (uint)sb.Capacity, VOLUME_NAME_DOS);

                if (result > 0 && result < sb.Capacity) {
                    string path = sb.ToString();
                    // 移除"\\?\"前缀
                    if (path.StartsWith("\\\\?\\")) {
                        path = path.Substring(4);
                    }
                    return path;
                }
            } catch {}
            finally {
                if (handle != IntPtr.Zero && handle != new IntPtr(-1)) {
                    CloseHandle(handle);
                }
            }
            return string.Empty;
        }
    }
}