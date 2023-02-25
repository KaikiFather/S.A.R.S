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
        public static async Task<bool> DownloadVrcaAsync(Avatar avatar, VRChatApiClient VrChat, string AuthKey, decimal pcVersion, decimal questVersion, string TwoFactor, Download download)
        {
            try
            {
                MessageBoxManager.Yes = "PC";
                MessageBoxManager.No = "Quest";
                MessageBoxManager.Register();
            } catch { }
            var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"\\VRCA\\{avatar.AvatarID}.vrca";
            if (AuthKey == "")
            {
                MessageBox.Show("please enter VRC Details on Settings page");
                download.error = true;
                return false;
            }

            if (download.InvokeRequired)
            {
                // Call this same method but append THREAD2 to the text
                Action safeWrite = delegate { download.Text = avatar.AvatarID; };
                download.Invoke(safeWrite);
            }
            else
            {
                download.Text = avatar.AvatarID;
            }

            if (avatar.PCAssetURL.ToLower() != "none" && avatar.QUESTAssetURL.ToLower() != "none" && avatar.PCAssetURL != null && avatar.QUESTAssetURL != null)
            {
                var dlgResult = MessageBox.Show("Select which version to download", "VRCA Select",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (dlgResult == DialogResult.No)
                {
                    if (avatar.QUESTAssetURL.ToLower() != "none")
                    {
                        try
                        {
                            var version = avatar.QUESTAssetURL.Split('/');
                            if (questVersion > 0)
                            {
                                version[7] = questVersion.ToString();
                            }
                            await Task.Run(() => VrChat.DownloadFile(string.Join("/", version), AuthKey, TwoFactor, filePath.Replace(".vrca", "_quest.vrca"), download.downloadProgress));
                        }
                        catch { await Task.Run(() => VrChat.DownloadFile(avatar.QUESTAssetURL, AuthKey, TwoFactor, filePath.Replace(".vrca", "_quest.vrca"), download.downloadProgress)); }
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
                    if (avatar.PCAssetURL.ToLower() != "none")
                    {
                        try
                        {
                            var version = avatar.PCAssetURL.Split('/');
                            if (pcVersion > 0)
                            {
                                version[7] = pcVersion.ToString();
                            }
                            await Task.Run(() => VrChat.DownloadFile(string.Join("/", version), AuthKey, TwoFactor, filePath.Replace(".vrca", "_pc.vrca"), download.downloadProgress));
                        }
                        catch { await Task.Run(() => VrChat.DownloadFile(avatar.PCAssetURL, AuthKey, TwoFactor, filePath.Replace(".vrca", "_pc.vrca"), download.downloadProgress)); }
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
            else if (avatar.PCAssetURL.ToLower() != "none" && avatar.PCAssetURL != null)
            {
                try
                {
                    var version = avatar.PCAssetURL.Split('/');
                    if (pcVersion > 0)
                    {
                        version[7] = pcVersion.ToString();
                    }
                    await Task.Run(() => VrChat.DownloadFile(string.Join("/", version), AuthKey, TwoFactor, filePath.Replace(".vrca", "_pc.vrca"), download.downloadProgress));
                }
                catch { await Task.Run(() => VrChat.DownloadFile(avatar.PCAssetURL, AuthKey, TwoFactor, filePath.Replace(".vrca", "_pc.vrca"), download.downloadProgress)); }
                pcDownload = true;
            }
            else if (avatar.QUESTAssetURL.ToLower() != "none" && avatar.QUESTAssetURL != null)
            {
                try
                {
                    var version = avatar.QUESTAssetURL.Split('/');
                    if (questVersion > 0)
                    {
                        version[7] = questVersion.ToString();
                    }
                    await Task.Run(() => VrChat.DownloadFile(string.Join("/", version), AuthKey, TwoFactor, filePath.Replace(".vrca", "_quest.vrca"), download.downloadProgress));
                }
                catch { await Task.Run(() => VrChat.DownloadFile(avatar.QUESTAssetURL, AuthKey, TwoFactor, filePath.Replace(".vrca", "_quest.vrca"), download.downloadProgress)); }
                pcDownload = false;
            }
            else
            {
                download.error = true;
                return false;
            }
            return true;
        }

        public static Tuple<int, int> GetVersion(string pcUrl, string questUrl, string authKey, string twoFactor, VRChatApiClient vrChat)
        {
            int pcVersion;
            int questVersion;
            if (!string.IsNullOrEmpty(pcUrl) && pcUrl.ToLower() != "none" && authKey != "")
            {
                try
                {
                    var version = pcUrl.Split('/');
                    var urlCheck = pcUrl.Replace(version[6] + "/" + version[7] + "/file", version[6]);
                    var versionList = vrChat.GetVersions(urlCheck, authKey, twoFactor);
                    if (versionList != null)
                    {
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

            return Tuple.Create(pcVersion, questVersion);
        }
    }
}
