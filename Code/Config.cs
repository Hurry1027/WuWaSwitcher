using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WuWaSwitcher {
    public class Config {
        public Config() {
            this.ConfigVersion = "1.2.0";
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

        public KRSDKConfig ParseKRSDKConfig(string json) {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var data = serializer.Deserialize<Dictionary<string, object>>(json);

            return new KRSDKConfig {
                KR_PackageName = data["KR_PackageName"]?.ToString() ?? "",
                KR_GameVersion = data["KR_GameVersion"]?.ToString() ?? "",
                KR_ChannelId = data["KR_ChannelId"]?.ToString() ?? ""
            };
        }

        public LauncherConfig ParseLauncherConfig(string json) {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var data = serializer.Deserialize<Dictionary<string, object>>(json);

            return new LauncherConfig {
                Version = data["version"]?.ToString() ?? "",
                ReUseVersion = data["reUseVersion"]?.ToString() ?? "",
                State = data["state"]?.ToString() ?? "",
                IsPreDownload = data.ContainsKey("isPreDownload") && data["isPreDownload"] is bool ? (bool)data["isPreDownload"] : false,
                AppId = data["appId"]?.ToString() ?? ""
            };
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

        public class LauncherConfig {
            [DataMember]
            public string Version { get; set; }
            [DataMember]
            public string ReUseVersion { get; set; }
            [DataMember]
            public string State { get; set; }
            [DataMember]
            public bool IsPreDownload { get; set; }
            [DataMember]
            public string AppId { get; set; }
        }
    }
}