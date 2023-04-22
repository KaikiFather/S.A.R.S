using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System;
using SARS.Models;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;
using System.Windows.Forms;
using MetroFramework.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Text.RegularExpressions;

namespace SARS.Modules
{
    public class CacheScanner
    {
        private MetroTextBox textbox;
        public CacheScanner(MetroTextBox logBox)
        {
            textbox = logBox;
        }
        public List<string> avatarIds = new List<string>();
        public void ReadUntilAvatarId(string filePath)
        {
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open), Encoding.ASCII))
                {
                    string prefab = "prefab-id-v1";
                    Regex avatarIdRegEx = new Regex(@"avtr_[\w]{8}-[\w]{4}-[\w]{4}-[\w]{4}-[\w]{12}");
                    var buffer = new StringBuilder();
                    bool foundEndOfLine = false;
                    char character;
                    while (!foundEndOfLine)
                    {
                        try
                        {
                            character = reader.ReadChar();
                            buffer.Append(character);
                            if (buffer.Length > prefab.Length)
                            {
                                buffer.Remove(0, 1);
                            }
                            if (buffer.ToString() == prefab)
                            {
                                bool endRead = false;
                                var avatarIdBuilder = new StringBuilder();
                                int underscores = 0;
                                while (!endRead)
                                {
                                    character = reader.ReadChar();
                                    avatarIdBuilder.Append(character);
                                    if (character == '_')
                                    {
                                        underscores++;
                                        if (underscores >= 3)
                                        {
                                            string avatarId = "avtr_" + avatarIdBuilder.ToString().Split('_')[2];
                                            if (avatarIdRegEx.IsMatch(avatarId))
                                            {
                                                if (!avatarIds.Contains(avatarId) && avatarId.Length == 41)
                                                {
                                                    avatarIds.Add(avatarId.Trim());
                                                }
                                            }
                                            return;
                                        }
                                    }
                                }
                                return;
                            }
                        }
                        catch (EndOfStreamException)
                        {
                            return;
                        }
                    }
                    return;
                }
            }
            catch (IOException e)
            {
                if (IsFileLocked(e))
                {
                    Console.WriteLine("File is locked");
                }
                return;
            }
        }

        private bool IsFileLocked(IOException exception)
        {
            int errorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);
            return errorCode == 32 || errorCode == 33;
        }

        public async Task ScanCache(string cacheLocation)
        {
            
            int avatarsFound = 0;
            List<VRCCacheScannerModel> avatars = new List<VRCCacheScannerModel>();

            Stopwatch stop = new Stopwatch();
            stop.Start();

            List<Task> tasks = new List<Task>();
            Dictionary<string, DateTime> locations = getCacheLocations(cacheLocation);

            foreach (string cache in locations.Keys)
            {
                tasks.Add(Task.Run(() => ReadUntilAvatarId(cache)));
            }
            Task.WaitAll(tasks.ToArray());

            string outputBuffer = "";

            avatarIds = avatarIds.Distinct().ToList();
            string appendText = "";
            foreach (var item in avatarIds)
            {
                outputBuffer += $"{item};\n";
                appendText = $"Avatar found: {item}{Environment.NewLine}" + appendText;
                avatarsFound++;
            }
            File.WriteAllText("avatarLog.txt", outputBuffer);
            stop.Stop();
            string timeRan = TimeSpan.FromMilliseconds(stop.ElapsedMilliseconds).TotalSeconds.ToString();
            AppendTextBox($"Found {avatarsFound} avatars in {timeRan} seconds{Environment.NewLine}{appendText}");
        }
        
        public void AppendTextBox(string value)
        {
            if (textbox.InvokeRequired)
            {
                textbox.Invoke(new Action<string>(AppendTextBox), new object[] { value });
                return;
            }
            textbox.Text += value + textbox.Text;
        }

        public static Dictionary<string, DateTime> getCacheLocations(string path)
        {
            Dictionary<string, DateTime> dataLocations = new Dictionary<string, DateTime>();

            string[] directory = Directory.GetDirectories(path);
            foreach (string item in directory)
            {
                try
                {
                    string[] dataFolders = Directory.GetDirectories(item);
                    if (dataFolders.Length > 0)
                    {
                        //Has data cache ?
                        string cacheDataPath = dataFolders[0] + "\\__data";
                        if (File.Exists(cacheDataPath))
                        {
                            FileInfo info = new FileInfo(cacheDataPath);
                            dataLocations.Add(cacheDataPath, info.CreationTime);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

            }

            var dicList = dataLocations.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            return dicList;
        }
    }

}