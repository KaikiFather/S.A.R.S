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
using System.Net.Mail;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using VRChatAPI;
using VRChatAPI.Models;

namespace SARS
{
    public partial class AvatarSystem : MetroForm
    {
        private ShrekApi shrekApi;
        private List<Avatar> avatars;
        public List<string> rippedList;
        private List<string> favList;
        public ConfigSave<Config> configSave;
        private HotswapConsole hotSwapConsole;
        private Thread _vrcaThread;
        private string userAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.5112.79 Safari/537.36";
        private string vrcaLocation = "";
        private VRChatApiClient VrChat;
        private string AuthKey;
        private string SystemName;

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
            rippedList = new List<string>();
            favList = new List<string>();
            configSave = new ConfigSave<Config>("config.cfg");
            LoadSettings();
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
            if (configSave.Config.FavoriteAvatars != null)
            {
                favList = configSave.Config.FavoriteAvatars;
            }
            if (configSave.Config.RippedAvatars != null)
            {
                rippedList = configSave.Config.RippedAvatars;
            }
            GetClientVersion();

            if (string.IsNullOrEmpty(configSave.Config.UnityLocation))
            {
                UnitySetup();
            }


            VrChat = new VRChatApiClient(15, configSave.Config.MacAddress);
            if (txtVRCUsername.Text != "" && txtVRCPassword.Text != "" && configSave.Config.MacAddress != "")
            {
                VrChat.CustomApiUser.Login(txtVRCUsername.Text, txtVRCPassword.Text);
                Thread.Sleep(1000);
                if (!File.Exists("auth.txt"))
                {
                    MessageBox.Show("Login Failed");
                }
                else
                {
                    AuthKey = File.ReadAllLines("auth.txt")[1];
                }
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
                    if (rippedList.Contains(avatars[i].AvatarID))
                    {
                        row.Cells[6].Value = true;
                    }
                    if (favList.Contains(avatars[i].AvatarID))
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
            avatars = shrekApi.GetList(rippedList);
            avatarGrid.Rows.Clear();
            LoadData();
            LoadImages();
        }

        private void btnSearchFavorites_Click(object sender, EventArgs e)
        {
            avatars = shrekApi.GetList(favList);
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
                    if (!favList.Contains(row.Cells[3].Value.ToString()))
                    {
                        favList.Add(row.Cells[3].Value.ToString());
                        row.Cells[7].Value = "true";
                    }
                    else
                    {
                        favList.Remove(row.Cells[3].Value.ToString());
                        row.Cells[7].Value = "false";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Some error" + ex.Message);
                }
            }

            configSave.Config.FavoriteAvatars = favList;
            configSave.Save();
        }

        private void avatarGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            vrcaLocation = "";
            this.Text = SystemName;
            this.Update();
            this.Refresh();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            configSave.Config.ApiKey = txtApiKey.Text.Trim();
            configSave.Save();
            shrekApi = new ShrekApi(configSave.Config.ApiKey);
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
            if (!_vrcaThread.IsAlive)
            {
                string fileLocation = "";
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
                        Image myImg = (row.Cells[0].Value as Image);
                        myImg.Save(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                                    $"\\{configSave.Config.HotSwapName}\\Assets\\Shrek SMART\\Resources\\shrekLogo.png", ImageFormat.Png);
                        avatar = avatars.FirstOrDefault(x => x.AvatarID == row.Cells[3].Value);
                        downloaded = RandomFunctions.DownloadVrca(avatar, VrChat, AuthKey, nmPcVersion.Value, nmQuestVersion.Value);
                    }
                    fileLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"\\{avatar.AvatarID}.vrca";
                }
                else
                {
                    fileLocation = vrcaLocation;
                }
                hotSwapConsole = new HotswapConsole();
                hotSwapConsole.Show();

                _vrcaThread = new Thread(() => HotSwap.HotswapProcess(hotSwapConsole, this, fileLocation));
                _vrcaThread.Start();
            }
            else
            {
                MessageBox.Show("VRCA Still hotswapping please try again later");
            }
        }

        private void btnUnity_Click(object sender, EventArgs e)
        {
            var tempFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                .Replace("\\Roaming", "");
            var unityTemp = $"\\Local\\Temp\\DefaultCompany\\{configSave.Config.HotSwapName}";
            var unityTemp2 = $"\\LocalLow\\Temp\\DefaultCompany\\{configSave.Config.HotSwapName}";

            RandomFunctions.tryDeleteDirectory(tempFolder + unityTemp, false);
            RandomFunctions.tryDeleteDirectory(tempFolder + unityTemp2, false);

            RandomFunctions.ExtractHSB(configSave.Config.HotSwapName);
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
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "vrc* files (*.vrc*)|*.vrc*";
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
        }

        private void btnSaveVRC_Click(object sender, EventArgs e)
        {
            //IniFile.Write("VRCUsername", txtVRCUsername.Text);
            //IniFile.Write("VRCPassword", txtVRCPassword.Text);
            // Authentication credentials
            if (txtVRCUsername.Text != "" && txtVRCPassword.Text != "" && txtTwoFactor.Text != "")
            {
                VrChat.TwoFactorCode = txtTwoFactor.Text;
                VrChat.CustomApiUser.Login(txtVRCUsername.Text, txtVRCPassword.Text, CustomApiUser.VerifyTwoFactorAuthCode);
                if (!File.Exists("auth.txt"))
                {
                    MessageBox.Show("Login Failed");
                }
                else
                {
                    AuthKey = File.ReadAllLines("auth.txt")[1];
                }
            }
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (avatarGrid.SelectedRows.Count > 1)
            {
                bool downloaded = false;
                Avatar avatar = null;
                foreach (DataGridViewRow row in avatarGrid.SelectedRows)
                {
                    Image myImg = (row.Cells[0].Value as Image);
                    myImg.Save(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                                $"\\{configSave.Config.HotSwapName}\\Assets\\Shrek SMART\\Resources\\shrekLogo.png", ImageFormat.Png);
                    avatar = avatars.FirstOrDefault(x => x.AvatarID == row.Cells[3].Value);
                    downloaded = RandomFunctions.DownloadVrca(avatar, VrChat, AuthKey, 0, 0);
                }
                string fileLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"\\{avatar.AvatarID}.vrca";

            } else
            {
                bool downloaded = false;
                Avatar avatar = null;
                foreach (DataGridViewRow row in avatarGrid.SelectedRows)
                {
                    Image myImg = (row.Cells[0].Value as Image);
                    myImg.Save(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                                $"\\{configSave.Config.HotSwapName}\\Assets\\Shrek SMART\\Resources\\shrekLogo.png", ImageFormat.Png);
                    avatar = avatars.FirstOrDefault(x => x.AvatarID == row.Cells[3].Value);
                    downloaded = RandomFunctions.DownloadVrca(avatar, VrChat, AuthKey, nmPcVersion.Value, nmQuestVersion.Value);
                }
                string fileLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"\\{avatar.AvatarID}.vrca";
            }
        }

        private void btnExtractVRCA_Click(object sender, EventArgs e)
        {

        }
    }
}