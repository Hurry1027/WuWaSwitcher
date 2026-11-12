using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WuWaSwitcher;
using static WuWaSwitcher.ResourceDiff;

public static class ReplaceFileDownloader {
    /// <summary>
    /// 下载差异文件，弹窗显示进度，支持用户取消。
    /// 返回 true = 全部完成；false = 用户取消。
    /// </summary>
    public static async Task<bool> DownloadReplaceFilesAsync(
        ClientType clientType,
        List<ReplaceFileList> files,
        string prefix) {
        if (files == null || files.Count == 0)
            return true;

        string rootDir = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            ".cache",
            clientType.ToString() + "-差异文件");

        long totalSize = files.Sum(f => f.Size);

        using (var form = new DownloadProgressForm(clientType, totalSize)) {
            var token = form.Token;
            form.Show();
            form.Refresh();

            long completedBytes = 0;
            int completedCount = 0;
            bool cancelled = false;

            try {
                foreach (var file in files) {
                    // 每次循环检查一次取消
                    if (token.IsCancellationRequested) { cancelled = true; break; }

                    string url = CombineUrl(prefix, file.Dest);
                    string relative = NormalizeToRelative(file.Dest);
                    string savePath = Path.Combine(rootDir, relative);
                    string tempPath = savePath + ".downloading";

                    string dir = Path.GetDirectoryName(savePath);
                    if (!string.IsNullOrEmpty(dir))
                        Directory.CreateDirectory(dir);

                    var currentFile = file;
                    long baseBytes = completedBytes;
                    int baseCount = completedCount;

                    bool thisFileCancelled = false;

                    using (var wc = new WebClient()) {
                        wc.Headers.Add("User-Agent", "WuWaSwitcher");

                        wc.DownloadProgressChanged += (s, e) => {
                            int percent;
                            if (totalSize > 0 && currentFile.Size > 0) {
                                double curFileBytes =
                                    e.TotalBytesToReceive > 0
                                        ? (double)e.BytesReceived / e.TotalBytesToReceive * currentFile.Size
                                        : currentFile.Size * e.ProgressPercentage / 100.0;

                                long done = baseBytes + (long)curFileBytes;
                                percent = (int)(done * 100.0 / totalSize);
                            }
                            else {
                                percent = (int)((baseCount * 100.0 + e.ProgressPercentage) / files.Count);
                            }

                            form.UpdateProgress(
                                currentFile.Dest,
                                FormatSize(currentFile.Size),
                                percent);
                        };

                        // 取消时通知 WebClient 中断当前下载
                        using (token.Register(() => {
                            try { wc.CancelAsync(); } catch { }
                        })) {
                            try {
                                await wc.DownloadFileTaskAsync(url, tempPath);

                                // 成功：替换正式文件
                                if (File.Exists(savePath)) File.Delete(savePath);
                                File.Move(tempPath, savePath);
                            }
                            catch (Exception ex) {
                                // 清理半截临时文件
                                try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }

                                if (token.IsCancellationRequested || IsCancellation(ex)) {
                                    thisFileCancelled = true;
                                }
                                else {
                                    throw;   // 真正的网络错误往上抛
                                }
                            }
                        }
                    }

                    if (thisFileCancelled) { cancelled = true; break; }

                    completedBytes += file.Size;
                    completedCount++;

                    int finished = totalSize > 0
                        ? (int)(completedBytes * 100.0 / totalSize)
                        : (int)(completedCount * 100.0 / files.Count);

                    form.UpdateProgress(file.Dest, FormatSize(file.Size),
                                        Math.Min(100, finished));
                }
            }
            finally {
                try { form.Close(); } catch { }
            }

            return !cancelled;
        }
    }

    /// <summary>判断异常是否属于"取消"</summary>
    private static bool IsCancellation(Exception ex) {
        if (ex is OperationCanceledException) return true;

        var we = ex as WebException;
        if (we != null && we.Status == WebExceptionStatus.RequestCanceled) return true;

        var ae = ex as AggregateException;
        if (ae != null && ae.InnerExceptions.Any(IsCancellation)) return true;

        return false;
    }

    private static string CombineUrl(string prefix, string path) {
        if (string.IsNullOrEmpty(prefix))
            return path ?? string.Empty;

        return prefix.TrimEnd('/') + "/" + (path ?? string.Empty).TrimStart('/');
    }

    /// <summary>把 Dest 归一化为安全的相对路径（去掉盘符/前导斜杠/..）</summary>
    private static string NormalizeToRelative(string dest) {
        if (string.IsNullOrWhiteSpace(dest))
            return string.Empty;

        string p = dest.Replace('\\', '/').Trim();

        if (p.Length >= 2 && p[1] == ':')
            p = p.Substring(2);

        p = p.TrimStart('/');

        var segs = p.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        var safe = new List<string>();
        foreach (var s in segs) {
            if (s == ".") continue;
            if (s == "..") continue;
            safe.Add(s);
        }

        return string.Join(Path.DirectorySeparatorChar.ToString(), safe);
    }
}