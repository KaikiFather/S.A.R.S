using MetroFramework.Forms;
using SARS.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SARS
{
    public partial class AvatarSystem : MetroForm
    {
        public AvatarSystem()
        {
            InitializeComponent();
        }

        private void AvatarSystem_Load(object sender, EventArgs e)
        {
            txtAbout.Text = Resources.About;
        }
    }
}
