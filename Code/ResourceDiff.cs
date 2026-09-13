using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace WuWaSwitcher
{
    public static class ResourceDiff {
        // ====== 配置区：修改这两个路径即可 ======
        private const string OfficialJsonPath = @"d:\Work\Code\C#\WuWaSwitcher\Test\LocalGameResources - 官方.json";
        private const string BServerJsonPath = @"d:\Work\Code\C#\WuWaSwitcher\Test\LocalGameResources - B服.json";
        public enum Mode {
            NetWork,
            Local
        }

        // ==========================================

        //private static readonly string OutputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ResourcesDiff.json");

        #region 数据模型

        public class ChunkInfo {
            public long Start { get; set; }
            public long End { get; set; }
            public string Md5 { get; set; }
        }

        public class ResourceItem {
            public string Dest { get; set; }
            public long Size { get; set; }
            public string Md5 { get; set; }
            public object FromFolder { get; set; }
            public List<ChunkInfo> ChunkInfos { get; set; }
        }

        public class ResourceFile {
            public List<ResourceItem> Resource { get; set; }
        }

        public class ResourceDiffItem {
            public string Dest { get; set; }
            public long Size { get; set; }
            public string Md5 { get; set; }
        }

        public class ResourceReplaceItem {
            public string Dest { get; set; }
            public long ASize { get; set; }
            public long BSize { get; set; }
            public bool SizeChanged { get; set; }
            public string AMd5 { get; set; }
            public string BMd5 { get; set; }
            public bool Md5Changed { get; set; }
        }

        public class DiffSummary {
            public int AOnlyCount { get; set; }
            public int BOnlyCount { get; set; }
            public int NeedReplaceCount { get; set; }
        }

        public class SwitchPlan {
            public string Direction { get; set; }
            public List<string> Remove { get; set; }
            public List<string> Replace { get; set; }
            public List<string> Add { get; set; }
        }

        public class SwitchPlans {
            public SwitchPlan AToB { get; set; }
            public SwitchPlan BToA { get; set; }
        }

        public class DiffResult {
            public DiffSummary Summary { get; set; }
            public List<ResourceDiffItem> AOnly { get; set; }
            public List<ResourceDiffItem> BOnly { get; set; }
            public List<ResourceReplaceItem> NeedReplace { get; set; }
            public SwitchPlans SwitchPlans { get; set; }
        }

        public class ReplaceFileList {
            public string Dest { get; set; }
            public long Size { get; set; }
        }

        #endregion

        #region 核心方法
        public static DiffResult Compare(ClientType clientType1, ClientType clientType2, Mode mode = Mode.NetWork) {
            Dictionary<string, ResourceItem> officialMap;
            Dictionary<string, ResourceItem> bServerMap;
            Console.WriteLine("正在加载资源文件...");
            if (mode == Mode.NetWork) {
                officialMap = GetLatestResourceManifest(clientType1);
                bServerMap = GetLatestResourceManifest(clientType2);
            }
            else {
                if (!Directory.Exists(Path.Combine(clientType1.ToString()))) {
                    throw new Exception($"未找到 {clientType1} 的备份\n请先备份所有客户端后重试");
                }
                if (!Directory.Exists(Path.Combine(clientType2.ToString()))) {
                    throw new Exception($"未找到 {clientType2} 的备份\n请先备份所有客户端后重试");
                }
                string clientType1JsonPath = Path.Combine(clientType1.ToString(), "LocalGameResources.json");
                string clientType2JsonPath = Path.Combine(clientType2.ToString(), "LocalGameResources.json");
                if (!File.Exists(clientType1JsonPath)) {
                    throw new Exception($"{clientType1}的备份文件夹中未找到LocalGameResources.json\n请修改Config.json将LocalGameResources.json添加到filesToBackup字段，且备份客户端后重试");
                }
                if (!File.Exists(clientType2JsonPath)) {
                    throw new Exception($"{clientType2}的备份文件夹中未找到LocalGameResources.json\n请修改Config.json将LocalGameResources.json添加到filesToBackup字段，且备份客户端后重试");
                }
                string clientType1Ver = Config.getKrSdkVersion(clientType1);
                string clientType2Ver = Config.getKrSdkVersion(clientType2);
                if (clientType1Ver == null || clientType2Ver == null || clientType1Ver?.Split('.')[0] != clientType2Ver?.Split('.')[0]) {
                    throw new Exception($"大版本号不同计算客户端差异无意义\n请更新且备份客户端后重试\n{clientType1}SDK版本: {clientType1Ver}\n{clientType2}SDK版本: {clientType2Ver}");
                }
                officialMap = LoadResources(clientType1JsonPath);
                bServerMap = LoadResources(clientType2JsonPath);
            }
            //Console.WriteLine($"{clientType1}资源数: " + officialMap.Count);
            //Console.WriteLine($"{clientType2}资源数: " + bServerMap.Count);
            if (officialMap == null || bServerMap == null) {
                return null;
            }
            DiffData diff = DiffResources(officialMap, bServerMap);
            DiffResult result = new DiffResult {
                Summary = new DiffSummary {
                    AOnlyCount = diff.AOnly.Count,
                    BOnlyCount = diff.BOnly.Count,
                    NeedReplaceCount = diff.NeedReplace.Count
                },
                AOnly = diff.AOnly,
                BOnly = diff.BOnly,
                NeedReplace = diff.NeedReplace,
                SwitchPlans = new SwitchPlans {
                    AToB = BuildSwitchPlan(diff, true, clientType1, clientType2),
                    BToA = BuildSwitchPlan(diff, false, clientType1, clientType2)
                }
            };

            string json = SerializeResult(result);
            string OutputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $".cache/ResourcesDiff_{mode}_{clientType1}_{clientType2}.json");
            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
            File.WriteAllText(OutputPath, json, new UTF8Encoding(false));
            Console.WriteLine("详细对比结果已保存到: " + OutputPath);

            //PrintSummary(result);

            return result;
        }

        // 获取总计所需备份文件列表（去重）
        public static List<ReplaceFileList> getFileToBackupPathList(DiffResult d1, DiffResult d2, DiffResult d3) {
            List<ReplaceFileList> all = new List<ReplaceFileList>();
            all.AddRange(getFileToBackup(d1));
            all.AddRange(getFileToBackup(d2));
            all.AddRange(getFileToBackup(d3));
            return all
                .GroupBy(u => u.Dest, StringComparer.OrdinalIgnoreCase)
                .Select(g => new ReplaceFileList {
                    Dest = g.Key,
                    Size = g.Max(x => x.Size)
                })
                .ToList();
        }

        // 获取所需备份文件列表（同名文件则记录最大的size）
        private static List<ReplaceFileList> getFileToBackup(DiffResult d1) {
            if (d1 == null) return new List<ReplaceFileList>();
            List<ReplaceFileList> list = new List<ReplaceFileList>();
            list.AddRange(d1.AOnly.Select(b => new ReplaceFileList { Dest = b.Dest, Size = b.Size }));
            list.AddRange(d1.BOnly.Select(b => new ReplaceFileList { Dest = b.Dest, Size = b.Size }));
            list.AddRange(d1.NeedReplace.Select(b => new ReplaceFileList {
                Dest = b.Dest,
                Size = Math.Max(b.ASize, b.BSize)
            }));
            return list;
        }

        public static DiffResult LoadDiffResult(string OutputPath) {
            if (!File.Exists(OutputPath)) {
                throw new FileNotFoundException("对比结果文件不存在", OutputPath);
            }

            string json = File.ReadAllText(OutputPath, Encoding.UTF8);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            return serializer.Deserialize<DiffResult>(json);
        }

        #endregion

        #region 私有方法

        private static Dictionary<string, ResourceItem> LoadResources(string filePath) {
            try {
                string json = File.ReadAllText(filePath, Encoding.UTF8);
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializer.MaxJsonLength = int.MaxValue;
                ResourceFile resourceFile = serializer.Deserialize<ResourceFile>(json);

                Dictionary<string, ResourceItem> map = new Dictionary<string, ResourceItem>(StringComparer.OrdinalIgnoreCase);
                if (resourceFile != null && resourceFile.Resource != null) {
                    foreach (ResourceItem item in resourceFile.Resource) {
                        map[item.Dest] = item;
                    }
                }
                return map;
            }
            catch (Exception ex) {
                MessageBox.Show($"加载备份目录里的LocalGameResources.json失败: \n({filePath})\n" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private static DiffData DiffResources(
            Dictionary<string, ResourceItem> officialMap,
            Dictionary<string, ResourceItem> bServerMap) {
            List<ResourceDiffItem> officialOnly = new List<ResourceDiffItem>();
            List<ResourceDiffItem> bServerOnly = new List<ResourceDiffItem>();
            List<ResourceReplaceItem> needReplace = new List<ResourceReplaceItem>();

            HashSet<string> allKeys = new HashSet<string>(officialMap.Keys.Concat(bServerMap.Keys));

            foreach (string dest in allKeys.OrderBy(x => x, StringComparer.Ordinal)) {
                bool inOfficial = officialMap.ContainsKey(dest);
                bool inBServer = bServerMap.ContainsKey(dest);

                if (inOfficial && !inBServer) {
                    ResourceItem item = officialMap[dest];
                    officialOnly.Add(new ResourceDiffItem {
                        Dest = dest,
                        Size = item.Size,
                        Md5 = item.Md5
                    });
                }
                else if (!inOfficial && inBServer) {
                    ResourceItem item = bServerMap[dest];
                    bServerOnly.Add(new ResourceDiffItem {
                        Dest = dest,
                        Size = item.Size,
                        Md5 = item.Md5
                    });
                }
                else {
                    ResourceItem officialItem = officialMap[dest];
                    ResourceItem bServerItem = bServerMap[dest];

                    bool sizeDiff = officialItem.Size != bServerItem.Size;
                    bool md5Diff = officialItem.Md5 != bServerItem.Md5;

                    if (sizeDiff || md5Diff) {
                        needReplace.Add(new ResourceReplaceItem {
                            Dest = dest,
                            ASize = officialItem.Size,
                            BSize = bServerItem.Size,
                            SizeChanged = sizeDiff,
                            AMd5 = officialItem.Md5,
                            BMd5 = bServerItem.Md5,
                            Md5Changed = md5Diff
                        });
                    }
                }
            }

            return new DiffData {
                AOnly = officialOnly,
                BOnly = bServerOnly,
                NeedReplace = needReplace
            };
        }

        private static SwitchPlan BuildSwitchPlan(DiffData diff, bool fromOfficialToB, ClientType clientType1, ClientType clientType2) {
            if (fromOfficialToB) {
                return new SwitchPlan {
                    Direction = $"{clientType1} -> {clientType2}",
                    Remove = diff.AOnly.Select(r => r.Dest).ToList(),
                    Replace = diff.NeedReplace.Select(r => r.Dest).ToList(),
                    Add = diff.BOnly.Select(r => r.Dest).ToList()
                };
            }
            else {
                return new SwitchPlan {
                    Direction = $"{clientType2} -> {clientType1}",
                    Remove = diff.BOnly.Select(r => r.Dest).ToList(),
                    Replace = diff.NeedReplace.Select(r => r.Dest).ToList(),
                    Add = diff.AOnly.Select(r => r.Dest).ToList()
                };
            }
        }

        private static string SerializeResult(DiffResult result) {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            return Utils.FormatJson(serializer.Serialize(result));
        }

        private static void PrintSummary(DiffResult result) {
            Console.WriteLine("========== 资源对比总结 ==========");
            Console.WriteLine();

            Console.WriteLine("【官方独有】 " + result.Summary.AOnlyCount + " 个文件");
            if (result.AOnly.Count > 0) {
                long totalSize = result.AOnly.Sum(r => r.Size);
                Console.WriteLine("  总大小: " + FormatSize(totalSize));
                foreach (ResourceDiffItem r in result.AOnly) {
                    Console.WriteLine("  - " + r.Dest + " (" + FormatSize(r.Size) + ")");
                }
            }
            Console.WriteLine();

            Console.WriteLine("【B服独有】 " + result.Summary.BOnlyCount + " 个文件");
            if (result.BOnly.Count > 0) {
                long totalSize = result.BOnly.Sum(r => r.Size);
                Console.WriteLine("  总大小: " + FormatSize(totalSize));
                foreach (ResourceDiffItem r in result.BOnly) {
                    Console.WriteLine("  - " + r.Dest + " (" + FormatSize(r.Size) + ")");
                }
            }
            Console.WriteLine();

            Console.WriteLine("【共有但需要替换】 " + result.Summary.NeedReplaceCount + " 个文件");
            if (result.NeedReplace.Count > 0) {
                long officialTotal = result.NeedReplace.Sum(r => r.ASize);
                long bServerTotal = result.NeedReplace.Sum(r => r.BSize);
                Console.WriteLine("  官方版本总大小: " + FormatSize(officialTotal));
                Console.WriteLine("  B服版本总大小:  " + FormatSize(bServerTotal));
                long diff = bServerTotal - officialTotal;
                Console.WriteLine("  大小差异: " + FormatSize(diff));
                foreach (ResourceReplaceItem r in result.NeedReplace) {
                    string sizeInfo;
                    if (r.SizeChanged) {
                        sizeInfo = FormatSize(r.ASize) + " -> " + FormatSize(r.BSize);
                    }
                    else {
                        sizeInfo = FormatSize(r.ASize) + " (大小不变)";
                    }
                    string md5Info = r.Md5Changed ? "MD5不同" : "MD5相同";
                    Console.WriteLine("  - " + r.Dest + " | " + sizeInfo + " | " + md5Info);
                }
            }
            Console.WriteLine();

            Console.WriteLine("========== 切换方案 ==========");
            Console.WriteLine();

            Console.WriteLine("【从官方切换到B服】");
            Console.WriteLine("  1. 删除 " + result.SwitchPlans.AToB.Remove.Count + " 个官方独有文件");
            Console.WriteLine("  2. 替换 " + result.SwitchPlans.AToB.Replace.Count + " 个共有文件");
            Console.WriteLine("  3. 新增 " + result.SwitchPlans.AToB.Add.Count + " 个B服独有文件");
            Console.WriteLine();

            Console.WriteLine("【从B服切换到官方】");
            Console.WriteLine("  1. 删除 " + result.SwitchPlans.BToA.Remove.Count + " 个B服独有文件");
            Console.WriteLine("  2. 替换 " + result.SwitchPlans.BToA.Replace.Count + " 个共有文件");
            Console.WriteLine("  3. 新增 " + result.SwitchPlans.BToA.Add.Count + " 个官方独有文件");
        }

        public static string FormatSize(long bytes) {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("F2") + " KB";
            if (bytes < 1024L * 1024 * 1024) return (bytes / (1024.0 * 1024)).ToString("F2") + " MB";
            return (bytes / (1024.0 * 1024 * 1024)).ToString("F2") + " GB";
        }

        #endregion

        #region 辅助结构

        private class DiffData {
            public List<ResourceDiffItem> AOnly { get; set; }
            public List<ResourceDiffItem> BOnly { get; set; }
            public List<ResourceReplaceItem> NeedReplace { get; set; }
        }

        #endregion

        #region 拉取资源清单
        private class ResourceIndexApiUrl {
            public string url { get; set; }
            public string appId { get; set; }
        }
        private static Dictionary<ClientType, ResourceIndexApiUrl> ResourceIndexApiMap = new Dictionary<ClientType, ResourceIndexApiUrl> {
            [ClientType.Official] = new ResourceIndexApiUrl {
                url = "https://prod-cn-alicdn-gamestarter.kurogame.com/launcher/game/G152/10003_Y8xXrXk65DqFHEDgApn3cpK5lfczpFx5/index.json",
                appId = "10003"
            },
            [ClientType.Bilibili] = new ResourceIndexApiUrl {
                url = "https://prod-cn-alicdn-gamestarter.kurogame.com/launcher/game/G152/10004_j5GWFuUFlb8N31Wi2uS3ZAVHcb7ZGN7y/index.json",
                appId = "10004"
            },
            [ClientType.WeGame] = new ResourceIndexApiUrl {
                url = "https://prod-cn-alicdn-gamestarter.kurogame.com/launcher/game/G152/10003_Y8xXrXk65DqFHEDgApn3cpK5lfczpFx5/index.json",
                appId = "10003"
            },
        };


        private static dynamic GetResourceIndex(ClientType clientType) {
            try {
                using (WebClient client = new WebClient()) {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    serializer.MaxJsonLength = int.MaxValue;
                    client.Headers.Add("User-Agent", "WuWaSwitcher");
                    client.Encoding = System.Text.Encoding.UTF8;
                    byte[] data = client.DownloadData(ResourceIndexApiMap[clientType].url);
                    string jsonResponse;
                    string contentEncoding = client.ResponseHeaders["Content-Encoding"];
                    if (!string.IsNullOrEmpty(contentEncoding) &&
                        contentEncoding.ToLower().Contains("gzip")) {
                        using (var ms = new MemoryStream(data))
                        using (var gz = new GZipStream(ms, CompressionMode.Decompress))
                        using (var sr = new StreamReader(gz, Encoding.UTF8)) {
                            jsonResponse = sr.ReadToEnd();// 被gzip压缩了
                        }
                    }
                    else {
                        jsonResponse = Encoding.UTF8.GetString(data);
                    }
                    //string rawPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $".cache/raw_response_{clientType}.bin");
                    //File.WriteAllBytes(rawPath, data);
                    if (string.IsNullOrEmpty(jsonResponse)) { throw new Exception("Index获取失败"); }
                    string IndexSavePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $".cache/ResourcesIndex_{clientType}.json");
                    Directory.CreateDirectory(Path.GetDirectoryName(IndexSavePath));
                    File.WriteAllText(IndexSavePath, Utils.FormatJson(jsonResponse), new UTF8Encoding(false));
                    // 解析资源json地址
                    dynamic ResourceIndexInfo = serializer.DeserializeObject(jsonResponse);
                    return ResourceIndexInfo;
                }
            }
            catch (Exception ex) {
                MessageBox.Show($"{clientType}拉取资源列表索引失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private static Dictionary<string, ResourceItem> GetLatestResourceManifest(ClientType clientType) {
            try {
                using (WebClient client = new WebClient()) {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    serializer.MaxJsonLength = int.MaxValue;
                    // 解析资源json地址
                    dynamic ResourceIndexInfo = GetResourceIndex(clientType);
                    if (ResourceIndexInfo == null) { return null; }
                    string cdn_host_url = ResourceIndexInfo["default"]["cdnList"][0]["url"];
                    string resources = ResourceIndexInfo["default"]["resources"];
                    string resources_url = cdn_host_url + resources;
                    // 请求资源json
                    string jsonResponse = client.DownloadString(resources_url);
                    if (string.IsNullOrEmpty(jsonResponse)) { throw new Exception("资源列表获取失败"); }
                    string ResourceSavePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $".cache/Resources_{clientType}.json");
                    Directory.CreateDirectory(Path.GetDirectoryName(ResourceSavePath));
                    File.WriteAllText(ResourceSavePath, Utils.FormatJson(jsonResponse), new UTF8Encoding(false));
                    ResourceFile resourceFile = serializer.Deserialize<ResourceFile>(jsonResponse);
                    Dictionary<string, ResourceItem> map = new Dictionary<string, ResourceItem>(StringComparer.OrdinalIgnoreCase);
                    if (resourceFile != null && resourceFile.Resource != null) {
                        foreach (ResourceItem item in resourceFile.Resource) {
                            map[item.Dest] = item;
                        }
                    }
                    return map;
                }
            }
            catch (Exception ex) {
                MessageBox.Show($"{clientType}拉取资源列表失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        #endregion
    }
}