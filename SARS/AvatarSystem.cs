using MetroFramework.Forms;
using SARS.Models;
using SARS.Modules;
using SARS.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SARS
{
    public partial class AvatarSystem : MetroForm
    {
        private ShrekApi shrekApi;
        private List<Avatar> avatars;
        public AvatarSystem()
        {
            InitializeComponent();
        }

        private void AvatarSystem_Load(object sender, EventArgs e)
        {
            shrekApi = new ShrekApi("9519694b-5938-44d6-904f-19477a0331cb");
            txtAbout.Text = Resources.About;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string limit = cbLimit.Text;
            if(limit == "Max")
            {
                limit = "10000";
            }
            if(limit == "")
            {
                limit = "500";
            }
            avatars = shrekApi.blankSearch(amount: Convert.ToInt32(limit));
            avatarGrid.Rows.Clear();
            Bitmap bitmap2 = null;
            try
            {
                System.Net.WebRequest request = System.Net.WebRequest.Create("https://ares-mod.com/avatars/download.png");
                System.Net.WebResponse response = request.GetResponse();
                System.IO.Stream responseStream = response.GetResponseStream();
                bitmap2 = new Bitmap(responseStream);
            }
            catch { }

            foreach (var item in avatars)
            {

                try
                {
                    DataGridViewRow row = (DataGridViewRow)avatarGrid.Rows[0].Clone();

                    row.Cells[0].Value = bitmap2;
                    row.Cells[1].Value = item.AvatarName;
                    row.Cells[2].Value = item.AuthorName;
                    row.Cells[3].Value = item.AvatarID;
                    row.Cells[4].Value = item.Created;
                    row.Cells[5].Value = item.ThumbnailURL;
                    avatarGrid.Rows.Add(row);
                }
                catch { }
            }
            lblAvatarCount.Text = (avatarGrid.Rows.Count - 1).ToString();
            LoadImages();
        }

        private void LoadImages()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                foreach (DataGridViewRow row in avatarGrid.Rows)
                {
                    if (row.Cells[5].Value != null)
                    {
                        try
                        {
                            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(row.Cells[5].Value.ToString());
                            myRequest.Method = "GET";
                            myRequest.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.5112.79 Safari/537.36";
                            HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
                            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(myResponse.GetResponseStream());
                            myResponse.Close();
                            row.Cells[0].Value = bmp;
                        }
                        catch
                        {
                            try
                            {
                                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create("https://ares-mod.com/avatars/Image_not_available.png");
                                myRequest.Method = "GET";
                                myRequest.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.5112.79 Safari/537.36";
                                HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
                                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(myResponse.GetResponseStream());
                                myResponse.Close();
                                row.Cells[0].Value = bmp;
                            }
                            catch { }
                        }
                    }
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
            string avatarString =
                $"Time Detected: {avatar.Created} {Environment.NewLine}Avatar Pin: {avatar.PinCode} {Environment.NewLine}Avatar ID: {avatar.AvatarID} {Environment.NewLine}Avatar Name: {avatar.AvatarName} {Environment.NewLine}Avatar Description {avatar.AvatarDescription} {Environment.NewLine}Author ID: {avatar.AuthorID} {Environment.NewLine}Author Name: {avatar.AuthorName} {Environment.NewLine}PC Asset URL: {avatar.PCAssetURL} {Environment.NewLine}Quest Asset URL: {avatar.QUESTAssetURL} {Environment.NewLine}Image URL: {avatar.ImageURL} {Environment.NewLine}Thumbnail URL: {avatar.ThumbnailURL} {Environment.NewLine}Unity Version: {avatar.UnityVersion} {Environment.NewLine}Release Status: {avatar.Releasestatus} {Environment.NewLine}Tags: {avatar.Tags}";
            return avatarString;
        }
    }
}
