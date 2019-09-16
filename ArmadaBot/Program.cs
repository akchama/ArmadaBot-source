using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArmadaBot.Proxy;
using ArmadaBot.Util;
using Fiddler;
using SKM.V3.Methods;
using SKM.V3.Models;
using Squirrel;

namespace ArmadaBot
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            HelpTools.CoInternetSetFeatureEnabled();
            HelpTools.UrlMkSetSessionOption("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");
            CertMaker.createRootCert();
            CertMaker.trustRootCert();
            StartBrowserProxy();

            RunUpdater();

            //LoginForm loginForm = new LoginForm();
            //if (loginForm.ShowDialog() == DialogResult.OK)
            //{
                Application.Run(new Form1());
            //    loginForm.Close();
            //}

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExHandler);
        }

        public static void StartBrowserProxy()
        {
            FiddlerProxy.Start();
            //WinInetInterop.SetConnectionProxy(string.Concat(new object[]
            //{
            //    "127.0.0.1",
            //    ":",
            //    "7777"
            //}));
        }

        static void ExHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"\Log.txt",
                "Exception caught : " + e.Message + "\n" + args.IsTerminating);
        }

        private static async void RunUpdater()
        {
            try
            {
                using (var mgr = UpdateManager.GitHubUpdateManager("https://github.com/Pengii/ArmadaBot").Result)
                {
                    await mgr.UpdateApp();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while updating: " + e.ToString());
            }
        }
    }
}
