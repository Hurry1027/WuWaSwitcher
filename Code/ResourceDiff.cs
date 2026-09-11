using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace WuWaSwitcher
{
    public static class ResourceDiff
    {
        // ====== 配置区：修改这两个路径即可 ======
        private const string OfficialJsonPath = @"d:\Work\Code\C#\WuWaSwitcher\Test\LocalGameResources - 官方.json";
        private const string BServerJsonPath = @"d:\Work\Code\C#\WuWaSwitcher\Test\LocalGameResources - B服.json";
        // ==========================================

        //private static readonly string OutputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ResourcesDiff.json");

        #region 数据模型

        public class ChunkInfo
        {
            public long Start { get; set; }
            public long End { get; set; }
            public string Md5 { get; set; }
        }

        public class ResourceItem
        {
            public string Dest { get; set; }
            public long Size { get; set; }
            public string Md5 { get; set; }
            public object FromFolder { get; set; }
            public List<ChunkInfo> ChunkInfos { get; set; }
        }

        public class ResourceFile
        {
            public List<ResourceItem> Resource { get; set; }
        }

        public class ResourceDiffItem
        {
            public string Dest { get; set; }
            public long Size { get; set; }
            public string Md5 { get; set; }
        }

        public class ResourceReplaceItem
        {
            public string Dest { get; set; }
            public long ASize { get; set; }
            public long BSize { get; set; }
            public bool SizeChanged { get; set; }
            public string AMd5 { get; set; }
            public string BMd5 { get; set; }
            public bool Md5Changed { get; set; }
        }

        public class DiffSummary
        {
            public int AOnlyCount { get; set; }
            public int BOnlyCount { get; set; }
            public int NeedReplaceCount { get; set; }
        }

        public class SwitchPlan
        {
            public string Direction { get; set; }
            public List<string> Remove { get; set; }
            public List<string> Replace { get; set; }
            public List<string> Add { get; set; }
        }

        public class SwitchPlans
        {
            public SwitchPlan AToB { get; set; }
            public SwitchPlan BToA { get; set; }
        }

        public class DiffResult
        {
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

        public static DiffResult Compare(ClientType clientType1, ClientType clientType2)
        {
            if (!Directory.Exists(clientType1.ToString())) {
                throw new Exception($"未找到 {clientType1} 的备份\n请先备份所有客户端后重试");
            }
            if (!Directory.Exists(clientType2.ToString())) {
                throw new Exception($"未找到 {clientType2} 的备份\n请先备份所有客户端后重试");
            }
            string clientType1JsonPath = Path.Combine(clientType1.ToString(), "LocalGameResources.json");
            string clientType2JsonPath = Path.Combine(clientType2.ToString(), "LocalGameResources.json");
            if (!File.Exists(clientType1JsonPath))
            {
                throw new Exception($"{clientType1}的备份文件夹中未找到LocalGameResources.json\n请修改Config.json将LocalGameResources.json添加到filesToBackup字段，且备份客户端后重试");
            }
            if (!File.Exists(clientType2JsonPath))
            {
                throw new Exception($"{clientType2}的备份文件夹中未找到LocalGameResources.json\n请修改Config.json将LocalGameResources.json添加到filesToBackup字段，且备份客户端后重试");
            }
            string clientType1Ver = Config.getKrSdkVersion(clientType1);
            string clientType2Ver = Config.getKrSdkVersion(clientType2);
            if (clientType1Ver == null || clientType2Ver == null || clientType1Ver?.Split('.')[0] != clientType2Ver?.Split('.')[0]) {
                throw new Exception($"大版本号不同计算客户端差异无意义\n请更新且备份客户端后重试\n{clientType1}SDK版本: {clientType1Ver}\n{clientType2}SDK版本: {clientType2Ver}");
            }

            Console.WriteLine("正在加载资源文件...");
            Dictionary<string, ResourceItem> officialMap = LoadResources(clientType1JsonPath);
            Dictionary<string, ResourceItem> bServerMap = LoadResources(clientType2JsonPath);

            Console.WriteLine($"{clientType1}资源数: " + officialMap.Count);
            Console.WriteLine($"{clientType2}资源数: " + bServerMap.Count);
            Console.WriteLine();

            DiffData diff = DiffResources(officialMap, bServerMap);
            DiffResult result = new DiffResult
            {
                Summary = new DiffSummary
                {
                    AOnlyCount = diff.AOnly.Count,
                    BOnlyCount = diff.BOnly.Count,
                    NeedReplaceCount = diff.NeedReplace.Count
                },
                AOnly = diff.AOnly,
                BOnly = diff.BOnly,
                NeedReplace = diff.NeedReplace,
                SwitchPlans = new SwitchPlans
                {
                    AToB = BuildSwitchPlan(diff, true, clientType1, clientType2),
                    BToA = BuildSwitchPlan(diff, false, clientType1, clientType2)
                }
            };

            string json = SerializeResult(result);
            string OutputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"ResourcesDiff_{clientType1}_{clientType2}.json");
            File.WriteAllText(OutputPath, json, new UTF8Encoding(false));
            Console.WriteLine("详细对比结果已保存到: " + OutputPath);

            //PrintSummary(result);

            return result;
        }

        // 获取总计所需备份文件列表（去重）
        public static List<ReplaceFileList> getFileToBackupPathList(DiffResult d1, DiffResult d2, DiffResult d3) {
            List<ReplaceFileList> d1Files = getFileToBackup(d1) ?? new List<ReplaceFileList>();
            List<ReplaceFileList> d2Files = getFileToBackup(d2) ?? new List<ReplaceFileList>();
            List<ReplaceFileList> d3Files = getFileToBackup(d3) ?? new List<ReplaceFileList>();
            List<ReplaceFileList> res = d1Files.Concat(d2Files).Concat(d3Files).GroupBy(u => u.Dest).Select(g => g.First()).ToList();         
            return res;
        }

        // 获取所需备份文件列表
        private static List<ReplaceFileList> getFileToBackup(DiffResult d1) {
            if (d1 == null) return null;
            return d1.AOnly.Concat(d1.BOnly).Select(b => new ReplaceFileList { Dest = b.Dest, Size = b.Size }).ToList()
                .Concat(d1.NeedReplace.Select(b => new ReplaceFileList { Dest = b.Dest, Size = Math.Max(b.ASize, b.BSize)}).ToList()).ToList();
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
        
        private static Dictionary<string, ResourceItem> LoadResources(string filePath)
        {
            string json = File.ReadAllText(filePath, Encoding.UTF8);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            ResourceFile resourceFile = serializer.Deserialize<ResourceFile>(json);

            Dictionary<string, ResourceItem> map = new Dictionary<string, ResourceItem>(StringComparer.OrdinalIgnoreCase);
            if (resourceFile != null && resourceFile.Resource != null)
            {
                foreach (ResourceItem item in resourceFile.Resource)
                {
                    map[item.Dest] = item;
                }
            }
            return map;
        }

        private static DiffData DiffResources(
            Dictionary<string, ResourceItem> officialMap,
            Dictionary<string, ResourceItem> bServerMap)
        {
            List<ResourceDiffItem> officialOnly = new List<ResourceDiffItem>();
            List<ResourceDiffItem> bServerOnly = new List<ResourceDiffItem>();
            List<ResourceReplaceItem> needReplace = new List<ResourceReplaceItem>();

            HashSet<string> allKeys = new HashSet<string>(officialMap.Keys.Concat(bServerMap.Keys));

            foreach (string dest in allKeys.OrderBy(x => x, StringComparer.Ordinal))
            {
                bool inOfficial = officialMap.ContainsKey(dest);
                bool inBServer = bServerMap.ContainsKey(dest);

                if (inOfficial && !inBServer)
                {
                    ResourceItem item = officialMap[dest];
                    officialOnly.Add(new ResourceDiffItem
                    {
                        Dest = dest,
                        Size = item.Size,
                        Md5 = item.Md5
                    });
                }
                else if (!inOfficial && inBServer)
                {
                    ResourceItem item = bServerMap[dest];
                    bServerOnly.Add(new ResourceDiffItem
                    {
                        Dest = dest,
                        Size = item.Size,
                        Md5 = item.Md5
                    });
                }
                else
                {
                    ResourceItem officialItem = officialMap[dest];
                    ResourceItem bServerItem = bServerMap[dest];

                    bool sizeDiff = officialItem.Size != bServerItem.Size;
                    bool md5Diff = officialItem.Md5 != bServerItem.Md5;

                    if (sizeDiff || md5Diff)
                    {
                        needReplace.Add(new ResourceReplaceItem
                        {
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

            return new DiffData
            {
                AOnly = officialOnly,
                BOnly = bServerOnly,
                NeedReplace = needReplace
            };
        }

        private static SwitchPlan BuildSwitchPlan(DiffData diff, bool fromOfficialToB, ClientType clientType1, ClientType clientType2)
        {
            if (fromOfficialToB)
            {
                return new SwitchPlan
                {
                    Direction = $"{clientType1} -> {clientType2}",
                    Remove = diff.AOnly.Select(r => r.Dest).ToList(),
                    Replace = diff.NeedReplace.Select(r => r.Dest).ToList(),
                    Add = diff.BOnly.Select(r => r.Dest).ToList()
                };
            }
            else
            {
                return new SwitchPlan
                {
                    Direction = $"{clientType2} -> {clientType1}",
                    Remove = diff.BOnly.Select(r => r.Dest).ToList(),
                    Replace = diff.NeedReplace.Select(r => r.Dest).ToList(),
                    Add = diff.AOnly.Select(r => r.Dest).ToList()
                };
            }
        }

        private static string SerializeResult(DiffResult result)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            return Utils.FormatJson(serializer.Serialize(result));
        }

        private static void PrintSummary(DiffResult result)
        {
            Console.WriteLine("========== 资源对比总结 ==========");
            Console.WriteLine();

            Console.WriteLine("【官方独有】 " + result.Summary.AOnlyCount + " 个文件");
            if (result.AOnly.Count > 0)
            {
                long totalSize = result.AOnly.Sum(r => r.Size);
                Console.WriteLine("  总大小: " + FormatSize(totalSize));
                foreach (ResourceDiffItem r in result.AOnly)
                {
                    Console.WriteLine("  - " + r.Dest + " (" + FormatSize(r.Size) + ")");
                }
            }
            Console.WriteLine();

            Console.WriteLine("【B服独有】 " + result.Summary.BOnlyCount + " 个文件");
            if (result.BOnly.Count > 0)
            {
                long totalSize = result.BOnly.Sum(r => r.Size);
                Console.WriteLine("  总大小: " + FormatSize(totalSize));
                foreach (ResourceDiffItem r in result.BOnly)
                {
                    Console.WriteLine("  - " + r.Dest + " (" + FormatSize(r.Size) + ")");
                }
            }
            Console.WriteLine();

            Console.WriteLine("【共有但需要替换】 " + result.Summary.NeedReplaceCount + " 个文件");
            if (result.NeedReplace.Count > 0)
            {
                long officialTotal = result.NeedReplace.Sum(r => r.ASize);
                long bServerTotal = result.NeedReplace.Sum(r => r.BSize);
                Console.WriteLine("  官方版本总大小: " + FormatSize(officialTotal));
                Console.WriteLine("  B服版本总大小:  " + FormatSize(bServerTotal));
                long diff = bServerTotal - officialTotal;
                Console.WriteLine("  大小差异: " + FormatSize(diff));
                foreach (ResourceReplaceItem r in result.NeedReplace)
                {
                    string sizeInfo;
                    if (r.SizeChanged)
                    {
                        sizeInfo = FormatSize(r.ASize) + " -> " + FormatSize(r.BSize);
                    }
                    else
                    {
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

        public static string FormatSize(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("F2") + " KB";
            if (bytes < 1024L * 1024 * 1024) return (bytes / (1024.0 * 1024)).ToString("F2") + " MB";
            return (bytes / (1024.0 * 1024 * 1024)).ToString("F2") + " GB";
        }

        #endregion

        #region 辅助结构

        private class DiffData
        {
            public List<ResourceDiffItem> AOnly { get; set; }
            public List<ResourceDiffItem> BOnly { get; set; }
            public List<ResourceReplaceItem> NeedReplace { get; set; }
        }

        #endregion
    }
}