using SARS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VRChatAPI;
using VRChatAPI.Models;

namespace SARS.Modules
{
    internal static class AvatarFunctions
    {
        public static void ExtractHSB(string hotSwapName)
        {
            string filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!Directory.Exists(filePath + $"\\{hotSwapName}\\"))
            {
                ZipFile.ExtractToDirectory(filePath + @"\SARS.zip", filePath + $"\\{hotSwapName}");
                try
                {
                    string text = File.ReadAllText(filePath + $"\\{hotSwapName}\\ProjectSettings\\ProjectSettings.asset");
                    text = text.Replace("SARS", hotSwapName);
                    File.WriteAllText(filePath + $"\\{hotSwapName}\\ProjectSettings\\ProjectSettings.asset", text);
                }
                catch { }
            }
        }

        public static bool pcDownload = true;
        public static async Task<bool> DownloadVrcaAsync(Avatar avatar, VRChatApiClient VrChat, string AuthKey, decimal pcVersion, decimal questVersion, string TwoFactor, Download download, bool both = false)
        {
            try
            {
                MessageBoxManager.Yes = "PC";
                MessageBoxManager.No = "Quest";
                MessageBoxManager.Register();
            } catch { }
            var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"\\VRCA\\{RandomFunctions.ReplaceInvalidChars(avatar.avatar.avatarName)}-{avatar.avatar.avatarId}.vrca";
            if (AuthKey == "")
            {
                MessageBox.Show("please enter VRC Details on Settings page");
                download.error = true;
                return false;
            }

            try
            {
                if (download.InvokeRequired)
                {
                    Action safeWrite = delegate { download.Text = avatar.avatar.avatarId; };
                    download.Invoke(safeWrite);
                }
                else
                {
                    download.Text = avatar.avatar.avatarId;
                }
            }
            catch { }

            if (avatar.avatar.pcAssetUrl.ToLower() != "none" && avatar.avatar.questAssetUrl.ToLower() != "none" && avatar.avatar.pcAssetUrl != null && avatar.avatar.questAssetUrl != null)
            {
                if (both)
                {
                    Console.WriteLine("Starting Download");
                    if (avatar.avatar.questAssetUrl.ToLower() != "none")
                    {
                        try
                        {
                            var version = avatar.avatar.questAssetUrl.Split('/');
                            if (questVersion > 0)
                            {
                                version[7] = questVersion.ToString();
                            }
                            await Task.Run(() => VrChat.DownloadFile(string.Join("/", version), AuthKey, TwoFactor, filePath.Replace(".vrca", "_quest.vrca"), download.downloadProgress, both));
                        }
                        catch { await Task.Run(() => VrChat.DownloadFile(avatar.avatar.questAssetUrl, AuthKey, TwoFactor, filePath.Replace(".vrca", "_quest.vrca"), download.downloadProgress, both)); }                       
                    }
                    Console.WriteLine("QUEST DOWNLOAD");
                    if (avatar.avatar.pcAssetUrl.ToLower() != "none")
                    {
                        try
                        {
                            var version = avatar.avatar.pcAssetUrl.Split('/');
                            if (pcVersion > 0)
                            {
                                version[7] = pcVersion.ToString();
                            }
                            await Task.Run(() => VrChat.DownloadFile(string.Join("/", version), AuthKey, TwoFactor, filePath.Replace(".vrca", "_pc.vrca"), download.downloadProgress, both));
                        }
                        catch { await Task.Run(() => VrChat.DownloadFile(avatar.avatar.pcAssetUrl, AuthKey, TwoFactor, filePath.Replace(".vrca", "_pc.vrca"), download.downloadProgress, both)); }
                    }
                    Console.WriteLine("PC DOWNLOAD");
                    return true;
                }
                else
                {
                    var dlgResult = MessageBox.Show("Select which version to download", "VRCA Select",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (dlgResult == DialogResult.No)
                    {
                        if (avatar.avatar.questAssetUrl.ToLower() != "none")
                        {
                            try
                            {
                                var version = avatar.avatar.questAssetUrl.Split('/');
                                if (questVersion > 0)
                                {
                                    version[7] = questVersion.ToString();
                                }
                                await Task.Run(() => VrChat.DownloadFile(string.Join("/", version), AuthKey, TwoFactor, filePath.Replace(".vrca", "_quest.vrca"), download.downloadProgress));
                            }
                            catch { await Task.Run(() => VrChat.DownloadFile(avatar.avatar.questAssetUrl, AuthKey, TwoFactor, filePath.Replace(".vrca", "_quest.vrca"), download.downloadProgress)); }
                            pcDownload = false;
                        }
                        else
                        {
                            MessageBox.Show("Quest version doesn't exist", "ERROR", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            download.error = true;
                            return false;
                        }
                    }
                    else if (dlgResult == DialogResult.Yes)
                    {
                        if (avatar.avatar.pcAssetUrl.ToLower() != "none")
                        {
                            try
                            {
                                var version = avatar.avatar.pcAssetUrl.Split('/');
                                if (pcVersion > 0)
                                {
                                    version[7] = pcVersion.ToString();
                                }
                                await Task.Run(() => VrChat.DownloadFile(string.Join("/", version), AuthKey, TwoFactor, filePath.Replace(".vrca", "_pc.vrca"), download.downloadProgress));
                            }
                            catch { await Task.Run(() => VrChat.DownloadFile(avatar.avatar.pcAssetUrl, AuthKey, TwoFactor, filePath.Replace(".vrca", "_pc.vrca"), download.downloadProgress)); }
                            pcDownload = true;
                        }
                        else
                        {
                            MessageBox.Show("PC version doesn't exist", "ERROR", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            download.error = true;
                            return false;
                        }
                    }
                    else
                    {
                        download.error = true;
                        return false;
                    }
                }
            }
            else if (avatar.avatar.pcAssetUrl.ToLower() != "none" && avatar.avatar.pcAssetUrl != null)
            {
                try
                {
                    var version = avatar.avatar.pcAssetUrl.Split('/');
                    if (pcVersion > 0)
                    {
                        version[7] = pcVersion.ToString();
                    }
                    await Task.Run(() => VrChat.DownloadFile(string.Join("/", version), AuthKey, TwoFactor, filePath.Replace(".vrca", "_pc.vrca"), download.downloadProgress));
                }
                catch { await Task.Run(() => VrChat.DownloadFile(avatar.avatar.pcAssetUrl, AuthKey, TwoFactor, filePath.Replace(".vrca", "_pc.vrca"), download.downloadProgress)); }
                pcDownload = true;
            }
            else if (avatar.avatar.questAssetUrl.ToLower() != "none" && avatar.avatar.questAssetUrl != null)
            {
                try
                {
                    var version = avatar.avatar.questAssetUrl.Split('/');
                    if (questVersion > 0)
                    {
                        version[7] = questVersion.ToString();
                    }
                    await Task.Run(() => VrChat.DownloadFile(string.Join("/", version), AuthKey, TwoFactor, filePath.Replace(".vrca", "_quest.vrca"), download.downloadProgress));
                }
                catch { await Task.Run(() => VrChat.DownloadFile(avatar.avatar.questAssetUrl, AuthKey, TwoFactor, filePath.Replace(".vrca", "_quest.vrca"), download.downloadProgress)); }
                pcDownload = false;
            }
            else
            {
                download.error = true;
                return false;
            }
            return true;
        }

        public static Tuple<int, int, RootClass, RootClass> GetVersion(string pcUrl, string questUrl, string authKey, string twoFactor, VRChatApiClient vrChat)
        {
            int pcVersion;
            int questVersion;
            RootClass pcRoot = null;
            RootClass questRoot = null;
            if (!string.IsNullOrEmpty(pcUrl) && pcUrl.ToLower() != "none" && authKey != "")
            {
                try
                {
                    var version = pcUrl.Split('/');
                    var urlCheck = pcUrl.Replace(version[6] + "/" + version[7] + "/file", version[6]);
                    var versionList = vrChat.GetVersions(urlCheck, authKey, twoFactor);
                    if (versionList != null)
                    {
                        pcRoot = versionList;
                        pcVersion = Convert.ToInt32(versionList.versions.LastOrDefault().version);
                    }
                    else
                    {
                        pcVersion = 1;
                    }
                }
                catch
                {
                    pcVersion = 1;
                }
            }
            else
            {
                pcVersion = 0;
            }

            if (!string.IsNullOrEmpty(questUrl) && questUrl.ToLower() != "none" && authKey != "")
            {
                try
                {
                    var version = questUrl.Split('/');
                    var urlCheck = questUrl.Replace(version[6] + "/" + version[7] + "/file", version[6]);
                    var versionList = vrChat.GetVersions(urlCheck, authKey, twoFactor);
                    if (versionList != null)
                    {
                        questRoot = versionList;
                        questVersion = Convert.ToInt32(versionList.versions.LastOrDefault().version);
                    }
                    else
                    {
                        questVersion = 0;
                    }
                }
                catch
                {
                    questVersion = 1;
                }
            }
            else
            {
                questVersion = 0;
            }

            return Tuple.Create(pcVersion, questVersion, pcRoot, questRoot);
        }
    }
}
