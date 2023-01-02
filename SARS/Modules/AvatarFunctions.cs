using SARS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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

        public static bool DownloadVrca(Avatar avatar, VRChatApiClient VrChat, string AuthKey, decimal pcVersion, decimal questVersion, string TwoFactor)
        {
            var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"\\{avatar.AvatarID}.vrca";
            if (AuthKey == "")
            {
                MessageBox.Show("please enter VRC Details on Settings page");
                return false;
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
                            VrChat.DownloadFile(string.Join("/", version), AuthKey, TwoFactor, filePath);
                        }
                        catch { VrChat.DownloadFile(avatar.QUESTAssetURL, AuthKey, TwoFactor, filePath); }
                    }
                    else
                    {
                        MessageBox.Show("Quest version doesn't exist", "ERROR", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
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
                            VrChat.DownloadFile(string.Join("/", version), AuthKey, TwoFactor, filePath);
                        }
                        catch { VrChat.DownloadFile(avatar.PCAssetURL, AuthKey, TwoFactor, filePath); }
                    }
                    else
                    {
                        MessageBox.Show("PC version doesn't exist", "ERROR", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return false;
                    }
                }
                else
                {
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
                    VrChat.DownloadFile(string.Join("/", version), AuthKey, TwoFactor, filePath);
                }
                catch { VrChat.DownloadFile(avatar.PCAssetURL, AuthKey, TwoFactor, filePath); }
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
                    VrChat.DownloadFile(string.Join("/", version), AuthKey, TwoFactor, filePath);
                }
                catch { VrChat.DownloadFile(avatar.QUESTAssetURL, AuthKey, TwoFactor, filePath); }
            }
            else
            {
                return false;
            }
            return true;
        }

        public static Tuple<int, int> GetVersion(string pcUrl, string questUrl, string authKey, string twoFactor, VRChatApiClient vrChat)
        {
            int pcVersion;
            int questVersion;
            if (!string.IsNullOrEmpty(pcUrl) && authKey != "")
            {
                try
                {
                    var version = pcUrl.Split('/');
                    var urlCheck = pcUrl.Replace(version[6] + "/" + version[7] + "/file", version[6]);
                    var versionList = vrChat.GetVersions(urlCheck, authKey, twoFactor);
                    pcVersion = Convert.ToInt32(versionList.versions.LastOrDefault().version);
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

            if (!string.IsNullOrEmpty(questUrl) && authKey != "")
            {
                try
                {
                    var version = questUrl.Split('/');
                    var urlCheck = questUrl.Replace(version[6] + "/" + version[7] + "/file", version[6]);
                    var versionList = vrChat.GetVersions(urlCheck, authKey, twoFactor);
                    questVersion = Convert.ToInt32(versionList.versions.LastOrDefault().version);
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
