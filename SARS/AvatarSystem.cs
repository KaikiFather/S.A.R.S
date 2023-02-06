using MetroFramework;
using MetroFramework.Forms;
using Microsoft.Win32;
using SARS.Models;
using SARS.Modules;
using SARS.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VRChatAPI;
using VRChatAPI.Models;

namespace SARS
{
    public partial class AvatarSystem : MetroForm
    {
        private ShrekApi shrekApi;
        public List<Avatar> avatars;
        public ConfigSave<Config> configSave;
        public ConfigSave<List<Avatar>> rippedList;
        public ConfigSave<List<Avatar>> favList;
        private HotswapConsole hotSwapConsole;
        private Thread _vrcaThread;
        private string userAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.5112.79 Safari/537.36";
        private string vrcaLocation = "";
        private VRChatApiClient VrChat;
        private string SystemName;
        private static int LatestHsbVersion = 2;

        public AvatarSystem()
        {
            InitializeComponent();
            StyleManager = metroStyleManager1;
        }

        private void AvatarSystem_Load(object sender, EventArgs e)
        {
            
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            SystemName = "Shrek Avatar Recovery System (S.A.R.S) V" + fileVersionInfo.ProductVersion;
            this.Text = SystemName;
            txtAbout.Text = Resources.About;
            cbSearchTerm.SelectedIndex = 0;
            cbLimit.SelectedIndex = 3;
            try
            {
                configSave = new ConfigSave<Config>(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\config.cfg");
                rippedList = new ConfigSave<List<Avatar>>(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\ripped.cfg");
                favList = new ConfigSave<List<Avatar>>(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\fav.cfg");
                tabControl.SelectedIndex = 0;
            } catch { Console.WriteLine("Error with config"); }
            try
            {
                LoadSettings();
            } catch { Console.WriteLine("Error loading settings"); }
            if (string.IsNullOrEmpty(configSave.Config.HotSwapName))
            {
                int randomAmount = RandomFunctions.random.Next(12);
                configSave.Config.HotSwapName = RandomFunctions.RandomString(randomAmount);
                configSave.Save();
            }
            if (!string.IsNullOrEmpty(configSave.Config.ApiKey))
            {
                shrekApi = new ShrekApi(configSave.Config.ApiKey);
            }
            else
            {
                shrekApi = new ShrekApi("");
            }

            MessageBoxManager.Yes = "PC";
            MessageBoxManager.No = "Quest";
            MessageBoxManager.Register();
        }

        private void GetLatestVersion()
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            byte[] raw = wc.DownloadData("https://ares-mod.com/Latest.txt");

            string version = System.Text.Encoding.UTF8.GetString(raw);
            if (Assembly.GetExecutingAssembly().GetName().Version.ToString() != version)
            {
                MessageBox.Show($"You are running an out of date version of SARS please update to stay secure\nYour Version: {Assembly.GetExecutingAssembly().GetName().Version.ToString()}\nLatest Version: {version}");
            }
        }

        private void GetClientVersion()
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            byte[] raw = wc.DownloadData("https://ares-mod.com/Version.txt");

            txtClientVersion.Text = System.Text.Encoding.UTF8.GetString(raw);
            configSave.Config.ClientVersion = txtClientVersion.Text;

            raw = wc.DownloadData("https://ares-mod.com/VersionUpdated.txt");

            configSave.Config.ClientVersionLastUpdated = Convert.ToDateTime(System.Text.Encoding.UTF8.GetString(raw));
            configSave.Save();
        }

        private void LoadSettings()
        {
            txtApiKey.Text = configSave.Config.ApiKey;
            cbThemeColour.Text = configSave.Config.ThemeColor;
            if (configSave.Config.LightMode)
            {
                metroStyleManager1.Theme = MetroThemeStyle.Light;
            }
            GetClientVersion();
            GetLatestVersion();

            if (string.IsNullOrEmpty(configSave.Config.UnityLocation))
            {
                UnitySetup();
            }

            if (string.IsNullOrEmpty(configSave.Config.MacAddress))
            {
                Random rnd = new Random();
                configSave.Config.MacAddress = EasyHash.GetSHA1String(new byte[] { (byte)rnd.Next(254), (byte)rnd.Next(254), (byte)rnd.Next(254), (byte)rnd.Next(254), (byte)rnd.Next(254) });
                configSave.Save();
            }
            if(!string.IsNullOrEmpty(configSave.Config.PreSelectedAvatarLocation))
            {
                txtAvatarOutput.Text = configSave.Config.PreSelectedAvatarLocation;
            }
            if (!string.IsNullOrEmpty(configSave.Config.PreSelectedWorldLocation))
            {
                txtAvatarOutput.Text = configSave.Config.PreSelectedWorldLocation;
            }
            VrChat = new VRChatApiClient(15, configSave.Config.MacAddress);
            var check = VrChat.CustomApiUser.LoginWithExistingSession(configSave.Config.UserId, configSave.Config.AuthKey, configSave.Config.TwoFactor);
            if (check == null)
            {
                MessageBox.Show("VRChat credentials expired, please relogin");
                DeleteLoginInfo();
            }
        }

        private void UnitySetup()
        {
            var unityPath = UnityRegistry();
            if (unityPath != null)
            {
                var dlgResult =
                    MessageBox.Show(
                        $"Possible unity path found, Location: '{unityPath + @"\Editor\Unity.exe"}' is this correct?",
                        "Unity", MessageBoxButtons.YesNo);
                if (dlgResult == DialogResult.Yes)
                {
                    if (File.Exists(unityPath + @"\Editor\Unity.exe"))
                    {
                        configSave.Config.UnityLocation = unityPath + @"\Editor\Unity.exe";
                        configSave.Save();
                        MessageBox.Show(
                            "Leave the command window open it will close by itself after the unity setup is complete");
                    }
                    else
                    {
                        MessageBox.Show("Someone didn't check because that file doesn't exist!");
                        SelectFile();
                    }
                }
                else
                {
                    MessageBox.Show(
                        "Please select unity.exe, after doing this leave the command window open it will close by itself after setup is complete");
                    SelectFile();
                }
            }
            else
            {
                MessageBox.Show(
                    "Please select unity.exe, after doing this leave the command window open it will close by itself after setup is complete");
                SelectFile();
            }
        }

        private void SelectFile()
        {
            var filePath = string.Empty;

            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "Unity (Unity.exe)|Unity.exe";
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = "Select Unity exe";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
            }

            configSave.Config.UnityLocation = filePath;
            configSave.Save();
        }

        private string UnityRegistry()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Unity Technologies\Installer\Unity"))
                {
                    if (key == null) return null;
                    var o = key.GetValue("Location x64");
                    if (o != null) return o.ToString();
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string limit = cbLimit.Text;
            DateTime? before = null;
            DateTime? after = null;
            if (limit == "Max")
            {
                limit = "10000";
            }
            if (limit == "")
            {
                limit = "500";
            }
            if (chkBefore.Checked)
            {
                before = dtBefore.Value;
            }
            if (chkAfter.Checked)
            {
                after = dtAfter.Value;
            }
            if (string.IsNullOrEmpty(txtSearchTerm.Text))
            {
                avatars = shrekApi.blankSearch(!chkPublic.Checked, !chkPrivate.Checked, chkQuest.Checked, chkPC.Checked, Convert.ToInt32(limit), before, after);
            }
            else if (cbSearchTerm.Text == "Avatar Name")
            {
                avatars = shrekApi.avatarNameSearch(txtSearchTerm.Text, chkContains.Checked, !chkPublic.Checked, !chkPrivate.Checked, chkQuest.Checked, chkPC.Checked, Convert.ToInt32(limit), before, after);
            }
            else if (cbSearchTerm.Text == "Author Name")
            {
                avatars = shrekApi.authorNameSearch(txtSearchTerm.Text, chkContains.Checked, !chkPublic.Checked, !chkPrivate.Checked, chkQuest.Checked, chkPC.Checked, Convert.ToInt32(limit), before, after);
            }
            else if (cbSearchTerm.Text == "Avatar ID")
            {
                avatars = shrekApi.avatarIdSearch(txtSearchTerm.Text, !chkPublic.Checked, !chkPrivate.Checked, chkQuest.Checked, chkPC.Checked);
            }
            else if (cbSearchTerm.Text == "Author ID")
            {
                avatars = shrekApi.authorIdSearch(txtSearchTerm.Text, !chkPublic.Checked, !chkPrivate.Checked, chkQuest.Checked, chkPC.Checked, Convert.ToInt32(limit), before, after);
            }
            else if (cbSearchTerm.Text == "World Name")
            {
            }
            else if (cbSearchTerm.Text == "World ID")
            {
            }
            avatarGrid.Rows.Clear();
            LoadData();
            LoadImages();
        }

        private void LoadData()
        {
            Bitmap bitmap2 = null;
            try
            {
                System.Net.WebRequest request = System.Net.WebRequest.Create("https://ares-mod.com/avatars/download.png");
                System.Net.WebResponse response = request.GetResponse();
                System.IO.Stream responseStream = response.GetResponseStream();
                bitmap2 = new Bitmap(responseStream);
            }
            catch { }

            avatarGrid.AllowUserToAddRows = true;
            avatarGrid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            for (int i = 0; i < avatars.Count; i++)
            {
                try
                {
                    DataGridViewRow row = (DataGridViewRow)avatarGrid.Rows[0].Clone();

                    row.Cells[0].Value = bitmap2;
                    row.Cells[1].Value = avatars[i].AvatarName;
                    row.Cells[2].Value = avatars[i].AuthorName;
                    row.Cells[3].Value = avatars[i].AvatarID;
                    row.Cells[4].Value = avatars[i].Created;
                    row.Cells[5].Value = avatars[i].ThumbnailURL;
                    if (rippedList.Config.Any(x => x.AvatarID == avatars[i].AvatarID))
                    {
                        row.Cells[6].Value = true;
                    }
                    if (favList.Config.Any(x => x.AvatarID == avatars[i].AvatarID))
                    {
                        row.Cells[7].Value = true;
                    }
                    avatarGrid.Rows.Add(row);
                }
                catch { }
            }
            avatarGrid.AllowUserToAddRows = false;
            int count = avatarGrid.Rows.Count;

            lblAvatarCount.Text = (count).ToString();

            avatarGrid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
        }

        private void LoadImages()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                for (int i = 0; i < avatarGrid.Rows.Count; i++)
                {
                    try
                    {
                        if (avatarGrid.Rows[i] != null)
                        {
                            if (avatarGrid.Rows[i].Cells[5].Value != null)
                            {
                                if (!string.IsNullOrEmpty(avatarGrid.Rows[i].Cells[5].Value.ToString().Trim()))
                                {
                                    try
                                    {
                                        HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(avatarGrid.Rows[i].Cells[5].Value.ToString());
                                        myRequest.Method = "GET";
                                        myRequest.UserAgent = userAgent;
                                        HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
                                        System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(myResponse.GetResponseStream());
                                        myResponse.Close();
                                        avatarGrid.Rows[i].Cells[0].Value = bmp;
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create("https://ares-mod.com/avatars/Image_not_available.png");
                                            myRequest.Method = "GET";
                                            myRequest.UserAgent = userAgent;
                                            HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
                                            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(myResponse.GetResponseStream());
                                            myResponse.Close();
                                            avatarGrid.Rows[i].Cells[0].Value = bmp;
                                        }
                                        catch { }
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }
            });
        }

        private void btnViewDetails_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in avatarGrid.SelectedRows)
            {
                Avatar info = avatars.FirstOrDefault(x => x.AvatarID == row.Cells[3].Value);
                Avatar_Info avatar = new Avatar_Info();
                avatar.txtAvatarInfo.Text = SetAvatarInfo(info);
                avatar._selectedAvatar = info;
                avatar.Show();
            }
        }

        public string SetAvatarInfo(Avatar avatar)
        {
            string avatarString = $"Time Detected: {avatar.Created} {Environment.NewLine}" +
                $"Avatar Pin: {avatar.PinCode} {Environment.NewLine}" +
                $"Avatar ID: {avatar.AvatarID} {Environment.NewLine}" +
                $"Avatar Name: {avatar.AvatarName} {Environment.NewLine}" +
                $"Avatar Description {avatar.AvatarDescription} {Environment.NewLine}" +
                $"Author ID: {avatar.AuthorID} {Environment.NewLine}" +
                $"Author Name: {avatar.AuthorName} {Environment.NewLine}" +
                $"PC Asset URL: {avatar.PCAssetURL} {Environment.NewLine}" +
                $"Quest Asset URL: {avatar.QUESTAssetURL} {Environment.NewLine}" +
                $"Image URL: {avatar.ImageURL} {Environment.NewLine}" +
                $"Thumbnail URL: {avatar.ThumbnailURL} {Environment.NewLine}" +
                $"Unity Version: {avatar.UnityVersion} {Environment.NewLine}" +
                $"Release Status: {avatar.Releasestatus} {Environment.NewLine}" +
                $"Tags: {avatar.Tags}";
            return avatarString;
        }

        private void btnBrowserView_Click(object sender, EventArgs e)
        {
            if (avatars != null)
            {
                GenerateHtml.GenerateHtmlPage(avatars);
                Process.Start("avatars.html");
            }
        }

        private void metroTabPage2_Click(object sender, EventArgs e)
        {
        }

        private void btnRipped_Click(object sender, EventArgs e)
        {
            avatars = rippedList.Config;
            avatarGrid.Rows.Clear();
            LoadData();
            LoadImages();
        }

        private void btnSearchFavorites_Click(object sender, EventArgs e)
        {
            avatars = favList.Config;
            avatarGrid.Rows.Clear();
            LoadData();
            LoadImages();
        }

        private void btnToggleFavorite_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in avatarGrid.SelectedRows)
            {
                try
                {
                    if (!favList.Config.Any(x => x.AvatarID == row.Cells[3].Value.ToString()))
                    {
                        favList.Config.Add(avatars.FirstOrDefault(x => x.AvatarID == row.Cells[3].Value.ToString()));
                        row.Cells[7].Value = "true";
                    }
                    else
                    {
                        favList.Config.Remove(avatars.FirstOrDefault(x => x.AvatarID == row.Cells[3].Value.ToString()));
                        row.Cells[7].Value = "false";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Some error" + ex.Message);
                }
            }

            favList.Save();
        }

        private void avatarGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            vrcaLocation = "";
            this.Text = SystemName;
            this.Update();
            this.Refresh();
            //if(avatarGrid.SelectedRows.Count == 1)
            //{
            //    Avatar info = avatars.FirstOrDefault(x => x.AvatarID == avatarGrid.SelectedRows[0].Cells[3].Value.ToString());
            //    var versions = AvatarFunctions.GetVersion(info.PCAssetURL, info.QUESTAssetURL, configSave.Config.AuthKey, configSave.Config.TwoFactor, VrChat);
            //    nmPcVersion.Value = versions.Item1;
            //    nmQuestVersion.Value = versions.Item2;
            //}
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            configSave.Config.ApiKey = txtApiKey.Text.Trim();
            configSave.Save();
            shrekApi = new ShrekApi(txtApiKey.Text.Trim());
            if (configSave.Config.ApiKey != "")
            {
                shrekApi.checkLogin();
            }
        }

        private void btnLight_Click(object sender, EventArgs e)
        {
            metroStyleManager1.Theme = MetroThemeStyle.Light;
            configSave.Config.LightMode = true;
            configSave.Save();
        }

        private void btnDark_Click(object sender, EventArgs e)
        {
            metroStyleManager1.Theme = MetroThemeStyle.Dark;
            configSave.Config.LightMode = false;
            configSave.Save();
        }

        private void LoadStyle(string style)
        {
            switch (style)
            {
                case "Black":
                    metroStyleManager1.Style = MetroColorStyle.Black;
                    configSave.Config.ThemeColor = style;
                    configSave.Save();
                    break;

                case "White":
                    metroStyleManager1.Style = MetroColorStyle.White;
                    configSave.Config.ThemeColor = style;
                    configSave.Save();
                    break;

                case "Silver":
                    metroStyleManager1.Style = MetroColorStyle.Silver;
                    configSave.Config.ThemeColor = style;
                    configSave.Save();
                    break;

                case "Green":
                    metroStyleManager1.Style = MetroColorStyle.Green;
                    configSave.Config.ThemeColor = style;
                    configSave.Save();
                    break;

                case "Blue":
                    metroStyleManager1.Style = MetroColorStyle.Blue;
                    configSave.Config.ThemeColor = style;
                    configSave.Save();
                    break;

                case "Lime":
                    metroStyleManager1.Style = MetroColorStyle.Lime;
                    configSave.Config.ThemeColor = style;
                    configSave.Save();
                    break;

                case "Teal":
                    metroStyleManager1.Style = MetroColorStyle.Teal;
                    configSave.Config.ThemeColor = style;
                    configSave.Save();
                    break;

                case "Orange":
                    metroStyleManager1.Style = MetroColorStyle.Orange;
                    configSave.Config.ThemeColor = style;
                    configSave.Save();
                    break;

                case "Brown":
                    metroStyleManager1.Style = MetroColorStyle.Brown;
                    configSave.Config.ThemeColor = style;
                    configSave.Save();
                    break;

                case "Pink":
                    metroStyleManager1.Style = MetroColorStyle.Pink;
                    configSave.Config.ThemeColor = style;
                    configSave.Save();
                    break;

                case "Magenta":
                    metroStyleManager1.Style = MetroColorStyle.Magenta;
                    configSave.Config.ThemeColor = style;
                    configSave.Save();
                    break;

                case "Purple":
                    metroStyleManager1.Style = MetroColorStyle.Purple;
                    configSave.Config.ThemeColor = style;
                    configSave.Save();
                    break;

                case "Red":
                    metroStyleManager1.Style = MetroColorStyle.Red;
                    configSave.Config.ThemeColor = style;
                    configSave.Save();
                    break;

                case "Yellow":
                    metroStyleManager1.Style = MetroColorStyle.Yellow;
                    configSave.Config.ThemeColor = style;
                    configSave.Save();
                    break;

                default:
                    metroStyleManager1.Style = MetroColorStyle.Default;
                    configSave.Config.ThemeColor = "Default";
                    configSave.Save();
                    break;
            }
        }

        private void cbThemeColour_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadStyle(cbThemeColour.Text);
        }

        private void btnHotswap_Click(object sender, EventArgs e)
        {
            hotSwap();   
        }

        private async Task<bool> hotSwap()
        {
            if (_vrcaThread != null)
            {
                if (_vrcaThread.IsAlive)
                {
                    MessageBox.Show("VRCA Still hotswapping please try again later");
                    return false;
                }
            }

            string fileLocation = "";
            if (vrcaLocation == "")
            {
                if (avatarGrid.SelectedRows.Count > 1)
                {
                    MessageBox.Show("Please only select 1 row at a time for hotswapping.");
                    return false;
                }
                Avatar avatar = null;
                foreach (DataGridViewRow row in avatarGrid.SelectedRows)
                {
                    Download download = new Download();
                    download.Show();
                    try
                    {
                        Image myImg = (row.Cells[0].Value as Image);
                        myImg.Save(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                                    $"\\{configSave.Config.HotSwapName}\\Assets\\Shrek SMART\\Resources\\shrekLogo.png", ImageFormat.Png);
                        avatar = avatars.FirstOrDefault(x => x.AvatarID == row.Cells[3].Value);
                    }
                    catch { }
                    await Task.Run(() => AvatarFunctions.DownloadVrcaAsync(avatar, VrChat, configSave.Config.AuthKey, nmPcVersion.Value, nmQuestVersion.Value, configSave.Config.TwoFactor, download));
                }
                if (AvatarFunctions.pcDownload)
                {
                    fileLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"\\VRCA\\{avatar.AvatarID}_pc.vrca";
                } else
                {
                    fileLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"\\VRCA\\{avatar.AvatarID}_quest.vrca";
                }
                if (!File.Exists(fileLocation))
                {
                    return false;
                }
            }
            else
            {
                fileLocation = vrcaLocation;
            }
            hotSwapConsole = new HotswapConsole();
            hotSwapConsole.Show();

            _vrcaThread = new Thread(() => HotSwap.HotswapProcess(hotSwapConsole, this, fileLocation));
            _vrcaThread.Start();
            return true;
        }

        private void btnUnity_Click(object sender, EventArgs e)
        {
            var tempFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                .Replace("\\Roaming", "");
            var unityTemp = $"\\Local\\Temp\\DefaultCompany\\{configSave.Config.HotSwapName}";
            var unityTemp2 = $"\\LocalLow\\Temp\\DefaultCompany\\{configSave.Config.HotSwapName}";

            RandomFunctions.tryDeleteDirectory(tempFolder + unityTemp, false);
            RandomFunctions.tryDeleteDirectory(tempFolder + unityTemp2, false);

            if (configSave.Config.HsbVersion != 2)
            {
                CleanHsb();
                configSave.Config.HsbVersion = 2;
                configSave.Save();
            }

            AvatarFunctions.ExtractHSB(configSave.Config.HotSwapName);
            CopyFiles();
            RandomFunctions.OpenUnity(configSave.Config.UnityLocation, configSave.Config.HotSwapName);

            btnHotswap.Enabled = true;
        }

        private void CopyFiles()
        {
            try
            {
                var programLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                File.Copy(programLocation + @"\Template\SampleScene.unity", programLocation + $"\\{configSave.Config.HotSwapName}\\Assets\\Scenes\\SampleScene.unity", true);
                File.Copy(programLocation + @"\Template\shrekLogo.png", programLocation + $"\\{configSave.Config.HotSwapName}\\Assets\\Shrek SMART\\Resources\\shrekLogo.png", true);
            }
            catch { }
        }

        private void avatarGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.Value == null || e.RowIndex == -1)
                return;

            if (avatarGrid.Columns[e.ColumnIndex].AutoSizeMode != DataGridViewAutoSizeColumnMode.None)
            {

                //throw new InvalidOperationException(Format("dataGridView1 {0} AutoSizeMode <> 'None'", dataGridView1.Columns[e.ColumnIndex].Name));
            }

            var s = e.Graphics.MeasureString(e.Value.ToString(), new Font("Segoe UI", 11, FontStyle.Regular, GraphicsUnit.Pixel));
            if (e.Value.ToString().Length / (double)avatarGrid.Columns[e.ColumnIndex].Width >= .189)
            {
                SolidBrush backColorBrush;
                if (avatarGrid.SelectedRows[0].Index == e.RowIndex)
                    backColorBrush = new SolidBrush(e.CellStyle.SelectionBackColor);
                else
                    backColorBrush = new SolidBrush(e.CellStyle.BackColor);

                using (backColorBrush)
                {
                    e.Graphics.FillRectangle(backColorBrush, e.CellBounds);
                    e.Graphics.DrawString(e.Value.ToString(), avatarGrid.Font, Brushes.Black, e.CellBounds, StringFormat.GenericDefault);
                    //avatarGrid.Rows[e.RowIndex].Height = System.Convert.ToInt32((s.Height * Math.Ceiling(s.Width / (double)avatarGrid.Columns[e.ColumnIndex].Width)));
                    e.Handled = true;
                }
            }
        }

        private void btnResetScene_Click(object sender, EventArgs e)
        {
            CopyFiles();
        }

        private void btnHsbClean_Click(object sender, EventArgs e)
        {
            CleanHsb();
        }

        private void CleanHsb()
        {
            var programLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            RandomFunctions.KillProcess("Unity Hub.exe");
            RandomFunctions.KillProcess("Unity.exe");
            RandomFunctions.tryDeleteDirectory(programLocation + $"\\{configSave.Config.HotSwapName}");
            RandomFunctions.tryDeleteDirectory(@"C:\Users\" + Environment.UserName + $"\\AppData\\Local\\Temp\\DefaultCompany\\{configSave.Config.HotSwapName}");
            RandomFunctions.tryDeleteDirectory(@"C:\Users\" + Environment.UserName + $"\\AppData\\LocalLow\\DefaultCompany\\{configSave.Config.HotSwapName}");
        }

        private void btnLoadVRCA_Click(object sender, EventArgs e)
        {
            vrcaLocation = SelectFileVrca();
        }

        private string SelectFileVrca()
        {
            var filePath = string.Empty;

            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "vrca files (*.vrca)|*.vrca";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    this.Text = SystemName + " | VRCA FILE LOADED";
                    this.Update();
                    this.Refresh();
                }
            }

            return filePath;
        }

        private void avatarGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            vrcaLocation = "";
            this.Text = SystemName;
            this.Update();
            this.Refresh();
            if (avatarGrid.SelectedRows.Count == 1)
            {
                Avatar info = avatars.FirstOrDefault(x => x.AvatarID == avatarGrid.SelectedRows[0].Cells[3].Value.ToString());
                var versions = AvatarFunctions.GetVersion(info.PCAssetURL, info.QUESTAssetURL, configSave.Config.AuthKey, configSave.Config.TwoFactor, VrChat);
                nmPcVersion.Value = versions.Item1;
                nmQuestVersion.Value = versions.Item2;
            }
        }

        private void DeleteLoginInfo()
        {
            configSave.Config.UserId = null;
            configSave.Config.AuthKey = null;
            configSave.Config.TwoFactor = null;
            configSave.Save();
            try
            {
                _ = VrChat.CustomApiUser.Logout().Result;
            }
            catch
            {

            }
            try
            {
                File.Delete("auth.txt");
                File.Delete("2fa.txt");
            } catch { }
        }

        private void btnSaveVRC_Click(object sender, EventArgs e)
        {
            try
            {
                DeleteLoginInfo();
            }
            catch
            {

            }
            if (txtVRCUsername.Text != "" && txtVRCPassword.Text != "" && txtTwoFactor.Text != "")
            {
                VrChat.TwoFactorCode = txtTwoFactor.Text;
                try
                {
                    _ = VrChat.CustomApiUser.Login(txtVRCUsername.Text, txtVRCPassword.Text, CustomApiUser.VerifyTwoFactorAuthCode).Result;
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Couldn't verify 2FA code")
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                if (!File.Exists("auth.txt") || !File.Exists("2fa.txt"))
                {
                    MessageBox.Show("Login Failed");
                }
                else
                {
                    configSave.Config.UserId = File.ReadAllLines("auth.txt")[0];
                    configSave.Config.AuthKey = File.ReadAllLines("auth.txt")[1];
                    configSave.Config.TwoFactor = File.ReadAllLines("2fa.txt")[1];
                    configSave.Save();
                    MessageBox.Show("Login Successful");
                }
            }
            else if (txtVRCUsername.Text != "" && txtVRCPassword.Text != "" && txtTwoFactor.Text == "")
            {
                VrChat.TwoFactorCode = null;
                try
                {
                    _ = VrChat.CustomApiUser.Login(txtVRCUsername.Text, txtVRCPassword.Text, null).Result;
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Couldn't verify 2FA code")
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                if (!File.Exists("auth.txt"))
                {
                    MessageBox.Show("Login Failed");
                }
                else
                {
                    configSave.Config.UserId = File.ReadAllLines("auth.txt")[0];
                    configSave.Config.AuthKey = File.ReadAllLines("auth.txt")[1];
                    configSave.Config.TwoFactor = "";
                    configSave.Save();
                    MessageBox.Show("Login Successful");
                }
            }
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (avatarGrid.SelectedRows.Count > 1)
            {
                Download download = new Download();
                download.Show();
                Avatar avatar = null;
                foreach (DataGridViewRow row in avatarGrid.SelectedRows)
                {
                    avatar = avatars.FirstOrDefault(x => x.AvatarID == row.Cells[3].Value);
                    Task.Run(() => AvatarFunctions.DownloadVrcaAsync(avatar, VrChat, configSave.Config.AuthKey, 0, 0, configSave.Config.TwoFactor,download));
                }
                string fileLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"\\VRCA\\{avatar.AvatarID}.vrca";
            }
            else
            {
                Avatar avatar = null;
                foreach (DataGridViewRow row in avatarGrid.SelectedRows)
                {
                    Download download = new Download();
                    download.Show();
                    avatar = avatars.FirstOrDefault(x => x.AvatarID == row.Cells[3].Value);
                    Task.Run(() => AvatarFunctions.DownloadVrcaAsync(avatar, VrChat, configSave.Config.AuthKey, nmPcVersion.Value, nmQuestVersion.Value, configSave.Config.TwoFactor,download));
                }
                string fileLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"\\VRCA\\{avatar.AvatarID}.vrca";
            }
        }

        private async void btnExtractVRCA_Click(object sender, EventArgs e)
        {
            if (avatarGrid.SelectedRows.Count == 1 || vrcaLocation != "")
            {
                string avatarFile;
                Avatar avatar = null;
                if (vrcaLocation == "")
                {
                    Download download = new Download();
                    download.Show();
                    avatar = avatars.FirstOrDefault(x => x.AvatarID == avatarGrid.SelectedRows[0].Cells[3].Value);
                    if (await Task.Run(() => AvatarFunctions.DownloadVrcaAsync(avatar, VrChat, configSave.Config.AuthKey, nmPcVersion.Value, nmQuestVersion.Value, configSave.Config.TwoFactor, download)) == false) return;
                    avatarFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"\\VRCA\\{avatar.AvatarID}.vrca";
                    
                }
                else
                {
                    avatarFile = vrcaLocation;
                }

                var folderDlg = new FolderBrowserDialog
                {
                    ShowNewFolderButton = true
                };
                // Show the FolderBrowserDialog.
                var result = DialogResult.OK;
                if (!toggleAvatar.Checked || txtAvatarOutput.Text == "")
                    result = folderDlg.ShowDialog();
                else
                    folderDlg.SelectedPath = txtAvatarOutput.Text;
                if (result == DialogResult.OK || toggleAvatar.Checked && txtAvatarOutput.Text != "")
                {
                    var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var invalidFileNameChars = Path.GetInvalidFileNameChars();
                    var folderExtractLocation = folderDlg.SelectedPath + @"\" + Path.GetFileNameWithoutExtension(avatarFile);
                    if (!Directory.Exists(folderExtractLocation)) Directory.CreateDirectory(folderExtractLocation);
                    var commands =
                        string.Format(
                            "/C AssetRipper.exe \"{1}\" -o \"{0}\" -q ",
                             folderExtractLocation, avatarFile);

                    var p = new Process();
                    var psi = new ProcessStartInfo
                    {
                        FileName = "CMD.EXE",
                        Arguments = commands,
                        WorkingDirectory = filePath + @"\AssetRipperConsole_win64"
                    };
                    p.StartInfo = psi;
                    p.Start();
                    p.WaitForExit();

                    RandomFunctions.tryDeleteDirectory(folderExtractLocation + @"\AssetRipper\GameAssemblies", false);
                    try
                    {
                        Directory.Move(folderExtractLocation + @"\Assets\Shader",
                            folderExtractLocation + @"\Assets\.Shader");
                    }
                    catch
                    {
                    }
                    try
                    {
                        Directory.Move(folderExtractLocation + @"\Assets\Scripts",
                            folderExtractLocation + @"\Assets\.Scripts");
                    }
                    catch
                    {
                    }
                    RandomFunctions.tryDeleteDirectory(folderExtractLocation + @"\AuxiliaryFiles", false);
                    RandomFunctions.tryDeleteDirectory(folderExtractLocation + @"\ExportedProject\AssetRipper", false);
                    RandomFunctions.tryDeleteDirectory(folderExtractLocation + @"\ExportedProject\ProjectSettings", false);
                    try
                    {
                        Directory.Move(folderExtractLocation + @"\ExportedProject\Assets\Shader",
                            folderExtractLocation + @"\ExportedProject\Assets\.Shader");
                        Directory.Move(folderExtractLocation + @"\ExportedProject\Assets\Scripts",
                            folderExtractLocation + @"\ExportedProject\Assets\.Scripts");
                        Directory.Move(folderExtractLocation + @"\ExportedProject\Assets\MonoScript",
                            folderExtractLocation + @"\ExportedProject\Assets\.MonoScript");

                    }
                    catch
                    {
                    }

                    if (vrcaLocation == "")
                    {
                        rippedList.Config.Add(avatar);
                    }
                }
            }
            else
            {
                MetroMessageBox.Show(this, "Please select an avatar or world first.", "ERROR", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnUnityLoc_Click(object sender, EventArgs e)
        {
            SelectFile();
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            string fileLocation;

            if (vrcaLocation == "")
            {
                if (avatarGrid.SelectedRows.Count > 1)
                {
                    MessageBox.Show("Please only select 1 row at a time for hotswapping.");
                    return;
                }
                bool downloaded = false;
                Avatar avatar = null;
                foreach (DataGridViewRow row in avatarGrid.SelectedRows)
                {
                    Download download = new Download();
                    download.Show();
                    avatar = avatars.FirstOrDefault(x => x.AvatarID == row.Cells[3].Value);
                    Task.Run(() => AvatarFunctions.DownloadVrcaAsync(avatar, VrChat, configSave.Config.AuthKey, nmPcVersion.Value, nmQuestVersion.Value, configSave.Config.TwoFactor, download).Result);
                }
                fileLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"\\VRCA\\{avatar.AvatarID}.vrca";
                if (!File.Exists(fileLocation))
                {
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(vrcaLocation))
                {
                    MessageBox.Show("Please select an avatar first or load an VRCA file");
                    return;
                }
                fileLocation = vrcaLocation;
            }


            try
            {
                string commands = string.Format(fileLocation);
                Console.WriteLine(commands);
                Process p = new Process();
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "AssetViewer.exe",
                    Arguments = commands,
                    WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\NewestViewer\",
                };
                p.StartInfo = psi;
                p.Start();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            if (configSave.Config.AuthKey != null)
            {
                var check = VrChat.CustomApiUser.LoginWithExistingSession(configSave.Config.UserId, configSave.Config.AuthKey, configSave.Config.TwoFactor);
                if (check == null)
                {
                    MessageBox.Show("VRChat credentials expired, please relogin");
                    DeleteLoginInfo();
                }
                else
                {
                    MessageBox.Show("Token Works :D");
                }
            } else
            {
                MessageBox.Show("Login First");
            }
        }

        private void btnAvatarOut_Click(object sender, EventArgs e)
        {
            var folderDlg = new FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };
            var result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtAvatarOutput.Text = folderDlg.SelectedPath;
                configSave.Config.PreSelectedAvatarLocation= folderDlg.SelectedPath;
                configSave.Save();
            }
        }

        private void btnWorldOut_Click(object sender, EventArgs e)
        {
            var folderDlg = new FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };
            var result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtWorldOutput.Text = folderDlg.SelectedPath;
                configSave.Config.PreSelectedWorldLocation = folderDlg.SelectedPath;
                configSave.Save();
            }
        }
    }
}