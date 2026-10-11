using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace WuWaSwitcher {
    public class Config {
        public Config() {
            this.ConfigVersion = "1.3.0";
            this.IgnoredVersion = "";
            this.Client_launch_paths = new ClientPaths();
            this.DirectoriesToBackup = new string[] {
                "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland",
            };
            this.FilesToBackup = new string[] {};
        }

        public void Load(string configPath) {
            if (File.Exists(configPath)) {
                try {
                    string jsonContent = File.ReadAllText(configPath, Encoding.UTF8);
                    System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    var data = serializer.Deserialize<Dictionary<string, object>>(jsonContent);
                    GameFolderPath = data["gameFolderPath"]?.ToString() ?? "";

                    var clientPathsData = data["client_launch_paths"] as Dictionary<string, object>;
                    this.Client_launch_paths = new ClientPaths();
                    this.Client_launch_paths.Bilibili = clientPathsData != null && clientPathsData.ContainsKey("Bilibili") ? clientPathsData["Bilibili"]?.ToString() ?? "" : "";
                    this.Client_launch_paths.Official = clientPathsData != null && clientPathsData.ContainsKey("Official") ? clientPathsData["Official"]?.ToString() ?? "" : "";
                    this.Client_launch_paths.WeGame = clientPathsData != null && clientPathsData.ContainsKey("WeGame") ? clientPathsData["WeGame"]?.ToString() ?? "" : "";
                    if (data.ContainsKey("isAutoCloseSwitcher") && data["isAutoCloseSwitcher"] != null) {
                        this.IsAutoCloseSwitcher = (bool)data["isAutoCloseSwitcher"];
                    }

                    // 加载备份目录配置
                    if (data.ContainsKey("directoriesToBackup")) {
                        var dirsData = data["directoriesToBackup"] as System.Collections.ArrayList;
                        if (dirsData != null) {
                            this.DirectoriesToBackup = dirsData.Cast<string>().ToArray();
                        }
                    }

                    // 加载备份文件配置
                    if (data.ContainsKey("filesToBackup")) {
                        var filesData = data["filesToBackup"] as System.Collections.ArrayList;
                        if (filesData != null) {
                            this.FilesToBackup = filesData.Cast<string>().ToArray();
                        }
                    }

                    // 加载配置版本
                    if (data.ContainsKey("configVersion") && data["configVersion"] != null) {
                        this.ConfigVersion = data["configVersion"].ToString();
                    }
                    else {
                        this.ConfigVersion = "1.0.0";
                    }

                    // 加载忽略版本号配置
                    if (data.ContainsKey("ignoredVersion") && data["ignoredVersion"] != null) {
                        this.IgnoredVersion = data["ignoredVersion"].ToString();
                    }
                }
                catch (Exception ex) {
                    throw;
                }
            }
            else {
                this.GameFolderPath = "";
                this.Client_launch_paths.Bilibili = "";
                this.Client_launch_paths.Official = "";
                this.Client_launch_paths.WeGame = "";
                this.IsAutoCloseSwitcher = false;
            }
        }

        public bool Save(string configPath) {
            try {
                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var data = new Dictionary<string, object> {
                    ["configVersion"] = ConfigVersion,
                    ["gameFolderPath"] = GameFolderPath,
                    ["isAutoCloseSwitcher"] = IsAutoCloseSwitcher,
                    ["ignoredVersion"] = IgnoredVersion,
                    ["client_launch_paths"] = new Dictionary<string, object> {
                        ["Bilibili"] = Client_launch_paths.Bilibili,
                        ["Official"] = Client_launch_paths.Official,
                        ["WeGame"] = Client_launch_paths.WeGame
                    },
                    ["directoriesToBackup"] = DirectoriesToBackup,
                    ["filesToBackup"] = FilesToBackup
                };
                string json = serializer.Serialize(data);
                File.WriteAllText(configPath, Utils.FormatJson(json, 4), new UTF8Encoding(false));
                return true;
            }
            catch (Exception ex) {
                throw;
            }
        }

        public static KRSDKConfig ParseKRSDKConfig(string json) {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var data = serializer.Deserialize<Dictionary<string, object>>(json);

            return new KRSDKConfig {
                KR_PackageName = data["KR_PackageName"]?.ToString() ?? "",
                KR_GameVersion = data["KR_GameVersion"]?.ToString() ?? "",
                KR_ChannelId = data["KR_ChannelId"]?.ToString() ?? ""
            };
        }

        public static LauncherConfig ParseLauncherConfig(string json) {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var cfg = serializer.Deserialize<LauncherConfig>(json);
            if (cfg.Bundles == null)
                cfg.Bundles = new Dictionary<string, BundleConfig>();
            cfg.Version = cfg.Version ?? "";
            cfg.ReUseVersion = cfg.ReUseVersion ?? "";
            cfg.State = cfg.State ?? "";
            cfg.AppId = cfg.AppId ?? "";
            return cfg;
        }

        // 从备份中获取sdk版本号
        public static string getKrSdkVersion(ClientType clientType) {
            string krsdkConfigPath = Path.Combine(clientType.ToString(), "Client/Binaries/Win64/ThirdParty/KrPcSdk_Mainland/KRSDKRes/KRSDKConfig.json");
            // 检查SDK配置文件是否存在
            if (!File.Exists(krsdkConfigPath)) {
                return null;
            }
            string json = File.ReadAllText(krsdkConfigPath, Encoding.UTF8);
            var krsdkConfig = ParseKRSDKConfig(json);
            return krsdkConfig.KR_GameVersion;
        }


        // 备份文件夹识别结果
        public class BackupFolderCheckResult {
            public ClientType clientType { get; set; }
            public string gameVersion { get; set; }
            public string sdkVersion { get; set; }
            public bool isLocalGameResourcesExists { get; set; }
        }

        [DataMember]
        public string ConfigVersion { get; set; }
        [DataMember]
        public string GameFolderPath { get; set; }
        [DataMember]
        public ClientPaths Client_launch_paths { get; set; }
        [DataMember]
        public bool IsAutoCloseSwitcher { get; set; }
        [DataMember]
        public string[] DirectoriesToBackup { get; set; }
        [DataMember]
        public string[] FilesToBackup { get; set; }
        [DataMember]
        public string IgnoredVersion { get; set; }

        [DataContract]
        public class ClientPaths {
            [DataMember]
            public string Bilibili { get; set; }
            [DataMember]
            public string Official { get; set; }
            [DataMember]
            public string WeGame { get; set; }
        }

        [DataContract]
        public class KRSDKConfig {
            [DataMember]
            public string KR_PackageName { get; set; }
            [DataMember]
            public string KR_GameVersion { get; set; }
            [DataMember]
            public string KR_ChannelId { get; set; }
        }

        [DataContract]
        public class LauncherConfig {
            [DataMember(Name = "version")]
            public string Version { get; set; } = "";

            [DataMember(Name = "reUseVersion")]
            public string ReUseVersion { get; set; } = "";

            [DataMember(Name = "state")]
            public string State { get; set; } = "";

            [DataMember(Name = "isPreDownload")]
            public bool IsPreDownload { get; set; } = false;

            [DataMember(Name = "appId")]
            public string AppId { get; set; } = "";

            [DataMember(Name = "bundles")]
            public Dictionary<string, BundleConfig> Bundles { get; set; } = new Dictionary<string, BundleConfig>();
        }

        [DataContract]
        public class BundleConfig {
            [DataMember(Name = "version")]
            public string Version { get; set; } = "";

            [DataMember(Name = "state")]
            public string State { get; set; } = "";

            [DataMember(Name = "resourcePacks")]
            public List<string> ResourcePacks { get; set; } = new List<string>();
        }
    }
}