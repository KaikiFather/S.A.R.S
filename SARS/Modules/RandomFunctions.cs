using System.IO.Compression;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using MetroFramework;
using System.Windows.Forms;
using SARS.Models;
using VRChatAPI;

namespace SARS.Modules
{
    public static class RandomFunctions
    {
        public static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static void tryDelete(string location)
        {
            try
            {
                if (File.Exists(location))
                {
                    File.Delete(location);
                    //CoreFunctions.WriteLog($"Deleted file {location}", this);
                }
            }
            catch (Exception ex)
            {
                //CoreFunctions.WriteLog($"{ex.Message}", this);
            }
        }

        public static void tryDeleteDirectory(string location, bool showExceptions = true)
        {
            try
            {
                Directory.Delete(location, true);
                //CoreFunctions.WriteLog($"Deleted file {location}", this);
            }
            catch (Exception ex)
            {
                //if (showExceptions) CoreFunctions.WriteLog($"{ex.Message}", this);
            }
        }

        public static void OpenUnity(string unityPath, string hotSwapName)
        {
            string filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string commands = string.Format("/C \"{0}\" -ProjectPath " + hotSwapName, unityPath);

            Process process = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "CMD.EXE",
                Arguments = commands,
                WorkingDirectory = filePath,
            };
            process.StartInfo = processStartInfo;
            process.Start();
        }

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

        public static void KillProcess(string processName)
        {
            try
            {
                Process.Start("taskkill", "/F /IM \"" + processName + "\"");
                Console.WriteLine("Killed Process: " + processName);
                //WriteLog(string.Format("Killed Process", processName), main);
            }
            catch { }
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