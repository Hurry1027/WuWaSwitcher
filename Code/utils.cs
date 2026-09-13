


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WuWaSwitcher {
    public class Utils {
        public static string FormatJson(string json, int indentSize = 4) {
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
        public static List<ProcessCheckInfo> CheckProcessesBatch(List<ProcessCheckInfo> processesToCheck) {
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
        public static void KillProcessesBatch(List<ProcessCheckInfo> processesToKill) {
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

        //public static void SetDynamicIcon(string clientPath) {
        //    try {
        //        // 从配置中获取客户端路径并设置图标
        //        if (!string.IsNullOrEmpty(clientPath) && File.Exists(clientPath)) {
        //            // 提取客户端的图标
        //            Icon appIcon = System.Drawing.Icon.ExtractAssociatedIcon(clientPath);
        //            this.Icon = appIcon;
        //            notifyIcon1.Icon = appIcon;
        //        }
        //    }
        //    catch (Exception ex) {
        //        // 如果提取图标失败，使用默认图标
        //        // 这里可以添加日志记录
        //        System.Diagnostics.Debug.WriteLine("设置动态图标失败: " + ex.Message);
        //    }
        //}

        public static async Task CooldownButton(Button button, int seconds) {
            button.Enabled = false;
            //string originalText = button.Text;
            for (int i = seconds; i > 0; i--) {
                //button.Text = $"{originalText} ({i}s)";
                await Task.Delay(1000);
            }
            //button.Text = originalText;
            button.Enabled = true;
        }

        public static void CopyDirectory(string sourceDir, string destDir) {
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

        public static long GetDirectorySize(string directoryPath) {
            if (!Directory.Exists(directoryPath)) {
                return 0;
            }

            long size = 0;
            try {
                // 获取目录下所有文件的大小
                foreach (string file in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories)) {
                    try {
                        FileInfo fileInfo = new FileInfo(file);
                        size += fileInfo.Length;
                    }
                    catch (Exception) {
                        // 忽略无法访问的文件
                        continue;
                    }
                }
            }
            catch (Exception) {
                // 忽略无法访问的目录
            }

            return size;
        }

        public static string FormatFileSize(long bytes) {
            if (bytes < 1024) {
                return bytes + " B";
            }
            if (bytes < 1024L * 1024) {
                return string.Format("{0:F2} KB", bytes / 1024.0);
            }
            if (bytes < 1024L * 1024 * 1024) {
                return string.Format("{0:F2} MB", bytes / (1024.0 * 1024));
            }
            return string.Format("{0:F2} GB", bytes / (1024.0 * 1024 * 1024));
        }

        // 判断数组set相等(无序)
        public static bool AreStringArraysSetEqual(string[] arr1, string[] arr2) {
            if (arr1 == null && arr2 == null) return true;
            if (arr1 == null || arr2 == null) return false;
            if (arr1.Length != arr2.Length) return false;
            var set = new HashSet<string>(arr1);
            return arr2.All(x => set.Contains(x));
        }

        // 判断游戏版本是否更新(根据currentGameVersion)
        public static Version TryParseGameVersion(string s) {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim();
            if (!s.Contains('.')) s += ".0";   // 兼容 "5" → "5.0"
            if (!Version.TryParse(s, out var v)) return null;
            // 归一化：1.2.3 与 1.2.3.0 视为相同
            return new Version(
                v.Major,
                v.Minor,
                v.Build < 0 ? 0 : v.Build,
                v.Revision < 0 ? 0 : v.Revision);
        }

        /// <summary>
        /// 判断 newVer 是否比 oldVer 新。
        /// 返回 true  = newVer 更高
        /// 返回 false = newVer 不高于 oldVer（相同或更低）
        /// 返回 null  = 任一边无法解析（"未知"、null、空、格式非法），无法比较
        /// </summary>
        public static bool? IsNewerGameVersion(string newVer, string oldVer) {
            var a = TryParseGameVersion(oldVer);
            var b = TryParseGameVersion(newVer);
            if (a == null || b == null) return null;
            return b > a;
        }
    }
}