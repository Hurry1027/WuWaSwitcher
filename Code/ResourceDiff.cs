using System;
using System.Collections.Generic;
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

        private static readonly string OutputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ResourcesDiff.json");

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
            public long OfficialSize { get; set; }
            public long BServerSize { get; set; }
            public bool SizeChanged { get; set; }
            public string OfficialMd5 { get; set; }
            public string BServerMd5 { get; set; }
            public bool Md5Changed { get; set; }
        }

        public class DiffSummary
        {
            public int OfficialOnlyCount { get; set; }
            public int BServerOnlyCount { get; set; }
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
            public SwitchPlan OfficialToB { get; set; }
            public SwitchPlan BToOfficial { get; set; }
        }

        public class DiffResult
        {
            public DiffSummary Summary { get; set; }
            public List<ResourceDiffItem> OfficialOnly { get; set; }
            public List<ResourceDiffItem> BServerOnly { get; set; }
            public List<ResourceReplaceItem> NeedReplace { get; set; }
            public SwitchPlans SwitchPlans { get; set; }
        }


        #endregion

        #region 核心方法

        public static DiffResult Compare(string GameFolderPath, ClientType targetClientType)
        {
            string currentClientJsonPath = Path.Combine(GameFolderPath, "LocalGameResources.json");
            string targetClientJsonPath = Path.Combine(targetClientType.ToString(), "LocalGameResources.json");
            if (!File.Exists(currentClientJsonPath))
            {
                MessageBox.Show("当前游戏文件夹LocalGameResources.json不存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                return null;
            }
            if (!File.Exists(targetClientJsonPath))
            {
                MessageBox.Show($"目标备份{targetClientType.ToString()}不存在LocalGameResources.json", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                return null;
            }

            Console.WriteLine("正在加载资源文件...");
            Dictionary<string, ResourceItem> officialMap = LoadResources(OfficialJsonPath);
            Dictionary<string, ResourceItem> bServerMap = LoadResources(BServerJsonPath);

            Console.WriteLine("官方资源数: " + officialMap.Count);
            Console.WriteLine("B服资源数: " + bServerMap.Count);
            Console.WriteLine();

            DiffData diff = DiffResources(officialMap, bServerMap);
            DiffResult result = new DiffResult
            {
                Summary = new DiffSummary
                {
                    OfficialOnlyCount = diff.OfficialOnly.Count,
                    BServerOnlyCount = diff.BServerOnly.Count,
                    NeedReplaceCount = diff.NeedReplace.Count
                },
                OfficialOnly = diff.OfficialOnly,
                BServerOnly = diff.BServerOnly,
                NeedReplace = diff.NeedReplace,
                SwitchPlans = new SwitchPlans
                {
                    OfficialToB = BuildSwitchPlan(diff, true),
                    BToOfficial = BuildSwitchPlan(diff, false)
                }
            };

            string json = SerializeResult(result);
            File.WriteAllText(OutputPath, json, new UTF8Encoding(false));
            Console.WriteLine("详细对比结果已保存到: " + OutputPath);

            PrintSummary(result);

            return result;
        }

        public static DiffResult LoadDiffResult()
        {
            if (!File.Exists(OutputPath))
            {
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
                            OfficialSize = officialItem.Size,
                            BServerSize = bServerItem.Size,
                            SizeChanged = sizeDiff,
                            OfficialMd5 = officialItem.Md5,
                            BServerMd5 = bServerItem.Md5,
                            Md5Changed = md5Diff
                        });
                    }
                }
            }

            return new DiffData
            {
                OfficialOnly = officialOnly,
                BServerOnly = bServerOnly,
                NeedReplace = needReplace
            };
        }

        private static SwitchPlan BuildSwitchPlan(DiffData diff, bool fromOfficialToB)
        {
            if (fromOfficialToB)
            {
                return new SwitchPlan
                {
                    Direction = "官方 -> B服",
                    Remove = diff.OfficialOnly.Select(r => r.Dest).ToList(),
                    Replace = diff.NeedReplace.Select(r => r.Dest).ToList(),
                    Add = diff.BServerOnly.Select(r => r.Dest).ToList()
                };
            }
            else
            {
                return new SwitchPlan
                {
                    Direction = "B服 -> 官方",
                    Remove = diff.BServerOnly.Select(r => r.Dest).ToList(),
                    Replace = diff.NeedReplace.Select(r => r.Dest).ToList(),
                    Add = diff.OfficialOnly.Select(r => r.Dest).ToList()
                };
            }
        }

        private static string SerializeResult(DiffResult result)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            return serializer.Serialize(result);
        }

        private static void PrintSummary(DiffResult result)
        {
            Console.WriteLine("========== 资源对比总结 ==========");
            Console.WriteLine();

            Console.WriteLine("【官方独有】 " + result.Summary.OfficialOnlyCount + " 个文件");
            if (result.OfficialOnly.Count > 0)
            {
                long totalSize = result.OfficialOnly.Sum(r => r.Size);
                Console.WriteLine("  总大小: " + FormatSize(totalSize));
                foreach (ResourceDiffItem r in result.OfficialOnly)
                {
                    Console.WriteLine("  - " + r.Dest + " (" + FormatSize(r.Size) + ")");
                }
            }
            Console.WriteLine();

            Console.WriteLine("【B服独有】 " + result.Summary.BServerOnlyCount + " 个文件");
            if (result.BServerOnly.Count > 0)
            {
                long totalSize = result.BServerOnly.Sum(r => r.Size);
                Console.WriteLine("  总大小: " + FormatSize(totalSize));
                foreach (ResourceDiffItem r in result.BServerOnly)
                {
                    Console.WriteLine("  - " + r.Dest + " (" + FormatSize(r.Size) + ")");
                }
            }
            Console.WriteLine();

            Console.WriteLine("【共有但需要替换】 " + result.Summary.NeedReplaceCount + " 个文件");
            if (result.NeedReplace.Count > 0)
            {
                long officialTotal = result.NeedReplace.Sum(r => r.OfficialSize);
                long bServerTotal = result.NeedReplace.Sum(r => r.BServerSize);
                Console.WriteLine("  官方版本总大小: " + FormatSize(officialTotal));
                Console.WriteLine("  B服版本总大小:  " + FormatSize(bServerTotal));
                long diff = bServerTotal - officialTotal;
                Console.WriteLine("  大小差异: " + FormatSize(diff));
                foreach (ResourceReplaceItem r in result.NeedReplace)
                {
                    string sizeInfo;
                    if (r.SizeChanged)
                    {
                        sizeInfo = FormatSize(r.OfficialSize) + " -> " + FormatSize(r.BServerSize);
                    }
                    else
                    {
                        sizeInfo = FormatSize(r.OfficialSize) + " (大小不变)";
                    }
                    string md5Info = r.Md5Changed ? "MD5不同" : "MD5相同";
                    Console.WriteLine("  - " + r.Dest + " | " + sizeInfo + " | " + md5Info);
                }
            }
            Console.WriteLine();

            Console.WriteLine("========== 切换方案 ==========");
            Console.WriteLine();

            Console.WriteLine("【从官方切换到B服】");
            Console.WriteLine("  1. 删除 " + result.SwitchPlans.OfficialToB.Remove.Count + " 个官方独有文件");
            Console.WriteLine("  2. 替换 " + result.SwitchPlans.OfficialToB.Replace.Count + " 个共有文件");
            Console.WriteLine("  3. 新增 " + result.SwitchPlans.OfficialToB.Add.Count + " 个B服独有文件");
            Console.WriteLine();

            Console.WriteLine("【从B服切换到官方】");
            Console.WriteLine("  1. 删除 " + result.SwitchPlans.BToOfficial.Remove.Count + " 个B服独有文件");
            Console.WriteLine("  2. 替换 " + result.SwitchPlans.BToOfficial.Replace.Count + " 个共有文件");
            Console.WriteLine("  3. 新增 " + result.SwitchPlans.BToOfficial.Add.Count + " 个官方独有文件");
        }

        private static string FormatSize(long bytes)
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
            public List<ResourceDiffItem> OfficialOnly { get; set; }
            public List<ResourceDiffItem> BServerOnly { get; set; }
            public List<ResourceReplaceItem> NeedReplace { get; set; }
        }

        #endregion
    }
}