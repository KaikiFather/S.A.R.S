using MetroFramework.Forms;
using SARS.Models;
using System;
using System.Windows.Forms;

namespace SARS
{
    public partial class Avatar_Info : MetroForm
    {
        public Avatar _selectedAvatar;

        public Avatar_Info()
        {
            InitializeComponent();
        }

        private void Avatar_Info_Load(object sender, EventArgs e)
        {
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (cbCopy.Text == "Time Detected")
            {
                Clipboard.SetText(_selectedAvatar.TimeDetected);
                MessageBox.Show(this, "information copied to clipboard.", "Copied", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            if (cbCopy.Text == "Avatar ID")
            {
                Clipboard.SetText(_selectedAvatar.AvatarID);
                MessageBox.Show(this, "information copied to clipboard.", "Copied", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            if (cbCopy.Text == "Avatar Name")
            {
                Clipboard.SetText(_selectedAvatar.AvatarName);
                MessageBox.Show(this, "information copied to clipboard.", "Copied", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            if (cbCopy.Text == "Avatar Description")
            {
                Clipboard.SetText(_selectedAvatar.AvatarDescription);
                MessageBox.Show(this, "information copied to clipboard.", "Copied", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            if (cbCopy.Text == "Author ID")
            {
                Clipboard.SetText(_selectedAvatar.AuthorID);
                MessageBox.Show(this, "information copied to clipboard.", "Copied", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            if (cbCopy.Text == "Author Name")
            {
                Clipboard.SetText(_selectedAvatar.AuthorName);
                MessageBox.Show(this, "information copied to clipboard.", "Copied", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            if (cbCopy.Text == "PC Asset URL")
            {
                Clipboard.SetText(_selectedAvatar.PCAssetURL);
                MessageBox.Show(this, "information copied to clipboard.", "Copied", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            if (cbCopy.Text == "Quest Asset URL")
            {
                Clipboard.SetText(_selectedAvatar.QUESTAssetURL);
                MessageBox.Show(this, "information copied to clipboard.", "Copied", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            if (cbCopy.Text == "Image URL")
            {
                Clipboard.SetText(_selectedAvatar.ImageURL);
                MessageBox.Show(this, "information copied to clipboard.", "Copied", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            if (cbCopy.Text == "Thumbnail URL")
            {
                Clipboard.SetText(_selectedAvatar.ThumbnailURL);
                MessageBox.Show(this, "information copied to clipboard.", "Copied", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            if (cbCopy.Text == "Unity Version")
            {
                Clipboard.SetText(_selectedAvatar.UnityVersion);
                MessageBox.Show(this, "information copied to clipboard.", "Copied", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            if (cbCopy.Text == "Release Status")
            {
                Clipboard.SetText(_selectedAvatar.Releasestatus);
                MessageBox.Show(this, "information copied to clipboard.", "Copied", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            if (cbCopy.Text == "Tags")
            {
                Clipboard.SetText(_selectedAvatar.Tags);
                MessageBox.Show(this, "information copied to clipboard.", "Copied", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
    }
}