using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArmadaBot
{
    public partial class LoginForm : Form
    {
        CookieCollection cookies = null;
        private const string netlifyUrl = "https://armadabot.netlify.com/.netlify/identity/token";

        public LoginForm()
        {
            InitializeComponent();
            AddVersionNumber();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            GetUserSettings();
        }

        private void GetUserSettings()
        {
            textBoxNUsername.Text = Properties.Settings.Default.NUsername;
            textBoxNPassword.Text = Properties.Settings.Default.NPassword;
            checkBoxNRemember.Checked = Properties.Settings.Default.NRemember;
        }

        private void AddVersionNumber()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            this.Text += $" v.{ versionInfo.FileVersion }";
        }

        private void ButtonLogin_Click(object sender, EventArgs e)
        {
            var postParams = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", textBoxNUsername.Text },
                { "password", textBoxNPassword.Text },
            };

            string html = "";
            if (HTTP.PostRequest(netlifyUrl, null, new Dictionary<string, string> { }, postParams, ref cookies, ref html))
            {
                DialogResult = DialogResult.OK;

                if (checkBoxNRemember.Checked)
                    SaveUserSettings();
                else
                    ResetUserSettings();
            }
            else
            {
                MessageBox.Show("Giriş başarız, e-mail ve şifrenizi kontrol edin.");
            }
        }

        private void ResetUserSettings()
        {
            Properties.Settings.Default.Reset();
        }
        private void SaveUserSettings()
        {
            Properties.Settings.Default.NUsername = textBoxNUsername.Text;
            Properties.Settings.Default.NPassword = textBoxNPassword.Text;
            Properties.Settings.Default.NRemember = checkBoxNRemember.Checked;
            Properties.Settings.Default.Save();
        }
    }
}
