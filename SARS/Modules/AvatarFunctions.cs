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

        public static bool DownloadVrca(Avatar avatar, VRChatApiClient VrChat, string AuthKey, decimal pcVersion, decimal questVersion)
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
                            VrChat.DownloadFile(string.Join("/", version), AuthKey, filePath);
                        }
                        catch { VrChat.DownloadFile(avatar.QUESTAssetURL, AuthKey, filePath); }
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
                            VrChat.DownloadFile(string.Join("/", version), AuthKey, filePath);
                        }
                        catch { VrChat.DownloadFile(avatar.PCAssetURL, AuthKey, filePath); }
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
                    VrChat.DownloadFile(string.Join("/", version), AuthKey, filePath);
                }
                catch { VrChat.DownloadFile(avatar.PCAssetURL, AuthKey, filePath); }
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
                    VrChat.DownloadFile(string.Join("/", version), AuthKey, filePath);
                }
                catch { VrChat.DownloadFile(avatar.QUESTAssetURL, AuthKey, filePath); }
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
