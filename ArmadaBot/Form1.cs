using CefSharp;
using CefSharp.WinForms;
using Fiddler;
using SKM.V3.Methods;
using SKM.V3.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArmadaBot.ArmadaBattle;
using NuGet;

namespace ArmadaBot
{
    public partial class Form1 : Form
    {
        public static Form1 form1;
        public delegate void writerDelegate(string message);
        public delegate void scriptRunnerDelegate();
        public writerDelegate writer;
        public scriptRunnerDelegate scriptrunner;
        private ChromiumWebBrowser browser;
        public Thread worker;

        public Form1()
        {
            InitializeComponent();
            this.Init();
            InitializeChromium();
            this.OnStartUp();
        }

        public void InitializeChromium()
        {
            CefSettings settings = new CefSettings();
            settings.CachePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\CEF";
            // Initialize cef with the provided settings
            Cef.Initialize(settings);
            // Create a browser component
            LifeSpanHandler lifeSpanHandler = new LifeSpanHandler();

            RequestContext rc = new RequestContext();

            try
            {
                Cef.UIThreadTaskFactory.StartNew(delegate
                {
                    var v = new Dictionary<string, object>();
                    v["mode"] = "fixed_servers";
                    v["server"] = "127.0.0.1:7777";
                    string error;
                    bool success = rc.SetPreference("proxy", v, out error);
                });
            }
            catch (Exception e)
            {
                throw new System.InvalidOperationException(e.ToString());
            }
            browser = new ChromiumWebBrowser("http://www.armadabattle.com", rc);
            tabPage1.Controls.Add(browser);
            
            browser.LifeSpanHandler = lifeSpanHandler;
            browser.FrameLoadEnd += Browser_FrameLoadEnd;
            browser.LoadingStateChanged += ChromeBrowser_LoadingStateChanged;
        }

        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                pictureBox1.Invoke((MethodInvoker)(() =>
                {
                    loginButton.Enabled = true;
                }));

                if (browser.Address.Contains("homepage"))
                {
                    browser.ExecuteScriptAsync("document.getElementsByClassName('col-md-offset-4 playbutton fs-30 hvr-wobble-to-bottom-right')[0].click();");
                    browser.GetSourceAsync().ContinueWith(taskHtml =>
                    {
                        var html = taskHtml.Result;
                        string pattern = @"([0-9]+)<\/span>";
                        var m = Regex.Matches(html, pattern);
                        var reggold = m[0].Groups[1].Value;
                        var regdiamond = m[1].Groups[1].Value;
                        var regexp = m[3].Groups[1].Value;

                        if (!string.IsNullOrEmpty(reggold)
                            && !string.IsNullOrEmpty(regdiamond)
                            && !string.IsNullOrEmpty(regexp))
                        {
                            BotSession.PlayerGold = reggold;
                            BotSession.PlayerDiamond = regdiamond;
                            BotSession.PlayerExp = regexp;
                        }

                        pattern = @"([A-Za-z0-9]+)<\/b>";
                        Account.ID = Regex.Matches(html, pattern)[0].Groups[1].Value;
                        Account.UserName = Regex.Matches(html, pattern)[1].Groups[1].Value;
                        Account.Level = Regex.Matches(html, pattern)[2].Groups[1].Value;
                        Account.Rank = Regex.Matches(html, pattern)[3].Groups[1].Value;
                    });
                }
                else if (browser.Address.Contains("play"))
                {
                    pictureBox1.Invoke((MethodInvoker)(() => pictureBox1.Visible = false));
                    browser.ExecuteScriptAsync("document.getElementsByClassName('btn btn-sm btn-default')[2].click();");
                }
            }
        }

        private void ChromeBrowser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == false)
            {
                if (!browser.Address.Contains("play") && !browser.Address.Contains("homepage"))
                {

                }
            }
        }

        private void Init()
        {
            this.writer = new Form1.writerDelegate(this.Log);
            this.scriptrunner = new Form1.scriptRunnerDelegate(this.ExecuteJavaScript);
            form1 = this;
        }

        private void OnStartUp()
        {
            CertMaker.createRootCert();
            CertMaker.trustRootCert();
            this.StartLocalProxy();
            loginButton.Enabled = false;
    }

        private void SaveUserSettings()
        {
            Properties.Settings.Default.Username = userTextBox.Text;
            Properties.Settings.Default.Password = passTextBox.Text;
            Properties.Settings.Default.Remember = rememberMeCheckbox.Checked;
            Properties.Settings.Default.AvoidIslands = avoidislandcheckbox.Checked;
            Properties.Settings.Default.CollectBox = collectboxcheckbox.Checked;
            Properties.Settings.Default.ShootMonster = shootmonstercheckbox.Checked;
            Properties.Settings.Default.ShootNPC = shootnpccheckbox.Checked;
            Properties.Settings.Default.Speedhack = speedhackcheckbox.Checked;
            Properties.Settings.Default.Save();
        }

        public void StartLocalProxy()
        {
            Server server = new Server();
            //this.localThread = new Thread(server.Start);
            //this.localThread.IsBackground = false;
            //this.localThread.Start();
            //this.StartBrowserProxy();
        }

        public void Log(string message)
        {
            var dt = DateTime.Now;
            string time = "[" + dt.ToString("HH:mm:ss") + "]: ";
            LogBox.AppendText(time + message + System.Environment.NewLine);
            LogBox.ScrollToCaret();
        }

        private void ButtonLoginPlayer_Click(object sender, EventArgs e)
        {
            SaveUserSettings();
            try
            {
                loginButton.Enabled = false;
                BotSession.Username = userTextBox.Text;
                BotSession.Password = passTextBox.Text;
                browser.ExecuteScriptAsync("var event = new Event('input', { bubbles: true }); \ndocument.getElementsByName('username')[0].value = '" + userTextBox.Text + "'; document.getElementsByName('username')[0].dispatchEvent(event);");
                browser.ExecuteScriptAsync("document.getElementsByName('password')[0].value = '" + passTextBox.Text + "'; document.getElementsByName('password')[0].dispatchEvent(event);");
                Task.Delay(300).Wait();
                browser.ExecuteScriptAsync("document.getElementsByClassName('nk-btn col-md-12 col-xs-12 bg-main-2')[0].click();");
            }
            catch(Exception ex)
            {
                Bot.Log("Can't navigate to webpage: " + ex.ToString());
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (rememberMeCheckbox.Checked)
                SaveUserSettings();
            else
                ResetUserSettings();

            Cef.Shutdown();
            FiddlerApplication.Shutdown();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GetUserSettings();
            Assembly assembly = Assembly.GetExecutingAssembly();

            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            this.Text += $" v{ versionInfo.FileVersion }";

            Bot.Log("Armadabattle bot - ArmadaBot");
            Bot.Log("Current version: v" + assembly.GetName().Version.ToString(3));
            speedhackcheckbox.Checked = true;
            speedhackcheckbox.Enabled = false;
            BotSession.newTrialKey = Key.CreateTrialKey(
                "WyI4NjU2IiwiNlVHU0l1M0ptNHVURmZNOGpZbHRSZmVwMk1BaU1MUWxHZHcrTXFSLyJd",
                new CreateTrialKeyModel
                {
                    ProductId = 5006,
                    MachineCode = SKGL.SKM.getMachineCode(SKGL.SKM.getSHA1)
                });

            if (BotSession.newTrialKey == null || BotSession.newTrialKey.Result == ResultType.Error)
            {
                Bot.Log("Error during creating a license key.");
                buttonStart.Enabled = false;
                buttonStop.Enabled = false;
            }
            else
            {
                Bot.Log("License key: " + BotSession.newTrialKey.Key);
                var activate = Key.Activate("WyI4NjU1IiwiR294d3UvSC9GZkttbkRkdXdRTlFHb3ZvemlrdUFMQUVQT0hSOUZQdiJd",
                    new ActivateModel
                    {
                        ProductId = 5006,
                        Sign = true,
                        MachineCode = SKGL.SKM.getMachineCode(SKGL.SKM.getSHA1),
                        Key = BotSession.newTrialKey.Key,
                        Metadata = true
                    });

                if (activate == null || activate.Result == ResultType.Error)
                {
                    Bot.Log(activate.Result.ToString());
                }
                else
                {
                    Console.WriteLine("License is valid!");
                    // now we can verify some basic properties
                    label2.Text = BotSession.newTrialKey.Key;
                    if (activate.Metadata.LicenseStatus.IsValid)
                    {
                        Bot.Log("Trial version is activated untill: " + activate.LicenseKey.Expires);
                        Bot.Log("Patch notes and more information at: https://armadabot.netlify.com/.");
                    }
                    else
                    {
                        Console.WriteLine("Invalid license key.");
                        buttonStart.Enabled = false;
                    }
                }
            }
        }
        private void GetUserSettings()
        {
            userTextBox.Text = Properties.Settings.Default.Username;
            passTextBox.Text = Properties.Settings.Default.Password;
            rememberMeCheckbox.Checked = Properties.Settings.Default.Remember;
            speedhackcheckbox.Checked = Properties.Settings.Default.Speedhack;
            avoidislandcheckbox.Checked = Properties.Settings.Default.AvoidIslands;
            shootmonstercheckbox.Checked = Properties.Settings.Default.ShootMonster;
            shootnpccheckbox.Checked = Properties.Settings.Default.ShootNPC;
            collectboxcheckbox.Checked = Properties.Settings.Default.CollectBox;
        }

        private void ResetUserSettings()
        {
            Properties.Settings.Default.Reset();
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            BotSession.sessionStartTime = DateTime.Now;
            Bot.Running = true;
            BotSession.Collecting = false;
            worker = new Thread(() =>
            {
                while (true)
                {
                    if (BotSession.Collecting == false)
                    {
                        ExecuteJavaScript();
                    }
                    Thread.Sleep(100);
                }
            });
            worker.Start();
            Bot.Log("Bot started.");
            buttonStart.Invoke((MethodInvoker)(() =>
            {
                buttonStart.Enabled = false;
            }));
        }

        private void ExecuteJavaScript()
        {
            if (!browser.IsBrowserInitialized)
            {
                Bot.Log("Browser isn't initialized yet!");
                return;
            }

            BotSession.Collecting = true;
            if (Client.GameObjects.Count < 1)
            {
                Bot.Log("No targets left. Moving to a random place to explore new targets.");
                if (speedhackcheckbox.Checked)
                {
                    Random rnd = new Random();
                    if (BotData.InitialPosition.X != null && BotData.InitialPosition.Y != null)
                    {
                        var randX = rnd.Next(200);
                        var randY = rnd.Next(200);
                        browser.ExecuteScriptAsync("socket.emit(\"move player\", {\r\ngoToX: " + randX * 40 +
                                                   ",\r\ngoToY: " + randY * 40 + ",\r\nclicked: { x: " + randX * 40 +
                                                   ", y: " + randY * 40 + " }\r\n});\r\n");
                        Thread.Sleep(2000);
                    }
                }
                else
                {
                    Random rnd = new Random();
                    if (BotData.InitialPosition.X != null && BotData.InitialPosition.Y != null)
                    {
                        var start = new Point((double)BotData.InitialPosition.X, (double)BotData.InitialPosition.Y);
                        //var randX = rnd.Next(Convert.ToInt32(start.X)/40 + 20);
                        //var randY = rnd.Next(Convert.ToInt32(start.Y)/40 + 20);
                        var randX = 4000;
                        var randY = 4000;
                        var (xstring, ystring) = GetPathTuple(randX, randY);
                        browser.ExecuteScriptAsync("socket.emit(\"move player\", {\r\ngoToX: " + xstring +
                                                   ",\r\ngoToY: " + ystring + ",\r\nclicked: { x: " + randX +
                                                   ", y: " + randY + " }\r\n});\r\n");
                        //Bot.Log(xstring + "\n" + ystring + "\n" + randX + "\n" + randY );
                    }
                }
            }
            else
            {
                try
                {
                    BotData.Nearest = GetNearestBox(BotData.CurrentPosition);
                    var positionX = BotData.Nearest[1].ObjectInitMessageClass.Position.X;
                    var positionY = BotData.Nearest[1].ObjectInitMessageClass.Position.Y;
                    var nick = BotData.Nearest[1].ObjectInitMessageClass.Nickname;
                    var id = BotData.Nearest[1].ObjectInitMessageClass.Id;

                    Bot.Log("Going to the next closest object: "
                            + BotData.Nearest[1].ObjectInitMessageClass.Nickname
                            + " " + positionX + " " + positionY);

                    var (xstring, ystring) = GetPathTuple(positionX, positionY);

                    if (speedhackcheckbox.Checked)
                    {
                        Thread.Sleep(100);
                        browser.ExecuteScriptAsync("socket.emit(\"move player\", {\r\ngoToX: " + positionX +
                                                   ",\r\ngoToY: " + positionY + ",\r\nclicked: { x: " + positionX +
                                                   ", y: " + positionY + " }\r\n});\r\n");
                        Thread.Sleep(1500);
                        browser.ExecuteScriptAsync("socket.emit(\"collect glow\", { glow_id: '" + id +
                                                   "', lang: hbsLang });");
                        BotSession.Collecting = false;
                    }
                    else
                    {
                        Thread.Sleep(100);
                        browser.ExecuteScriptAsync("socket.emit(\"move player\", {\r\ngoToX: " + xstring +
                                                   ",\r\ngoToY: " + ystring + ",\r\nclicked: { x: " + positionX +
                                                   ", y: " + positionY + " }\r\n});\r\n");
                        Thread.Sleep(1500);
                        browser.ExecuteScriptAsync("socket.emit(\"collect glow\", { glow_id: '" + id +
                                                   "', lang: hbsLang });");
                        BotSession.Collecting = true;
                    }
                }
                catch
                {
                    // ignored
                }
            }
            BotSession.Collecting = false;
        }

        public (StringBuilder, StringBuilder) GetPathTuple(long? positionX, long? positionY)
        {
            StringBuilder xstring = new StringBuilder(Convert.ToString(positionX));
            StringBuilder ystring = new StringBuilder(Convert.ToString(positionY));
            if (BotData.InitialPosition.X != null && BotData.InitialPosition.Y != null)
            {
                var start = new Point((double)BotData.InitialPosition.X, (double)BotData.InitialPosition.Y);
                if (positionX != null && positionY != null)
                {
                    var end = new Point(Convert.ToDouble(positionX), Convert.ToDouble(positionY));
                    var coordinates = new List<Point>(Bot.Coordinates(start, end));
                    xstring = new StringBuilder();
                    ystring = new StringBuilder();
                    xstring.Append("[");
                    for (var i = 0; i < coordinates.Count; i++)
                    {
                        xstring.Append(coordinates[i].X);
                        if (i < coordinates.Count - 1)
                        {
                            xstring.Append(", ");
                        }
                    }

                    xstring.Append("]");
                    ystring.Append("[");
                    for (var i = 0; i < coordinates.Count; i++)
                    {
                        ystring.Append(coordinates[i].Y);
                        if (i < coordinates.Count - 1)
                        {
                            ystring.Append(", ");
                        }
                    }

                    ystring.Append("]");
                    //Bot.Log(coordinates[coordinates.Count - 1].X
                    //        + " " + coordinates[coordinates.Count - 1].Y);
                }
            }
            return (xstring, ystring);
        }

        private List<ObjectInitMessageElement> GetNearestBox(BotPoint currentPosition)
        {
            lock (this)
            {
                var closestDistanceSquared = long.MaxValue;
                List<ObjectInitMessageElement> nearest = null;
                List<List<ObjectInitMessageElement>> boxcopy = null;
                try
                {
                    boxcopy = Client.GameObjects.Values.ToList();

                }
                catch
                {

                }

                if (boxcopy == null) return null;
                foreach (var gameObject in boxcopy)
                {
                    try
                    {
                        var objectPosition = new BotPoint(
                            gameObject[1].ObjectInitMessageClass.Position.X,
                            gameObject[1].ObjectInitMessageClass.Position.Y);

                        var distanceSquared = (objectPosition.X - currentPosition.X) *
                                              (objectPosition.X - currentPosition.X)
                                              + (objectPosition.Y - currentPosition.Y) *
                                              (objectPosition.Y - currentPosition.Y);

                        if (distanceSquared < closestDistanceSquared)
                        {
                            closestDistanceSquared = (long) distanceSquared;
                            nearest = gameObject;
                            BotData.RemoveObject = gameObject;
                            Client.GameObjects.Remove(gameObject[1].ObjectInitMessageClass.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"\Log.txt",
                            ex.ToString());
                        Bot.Log(ex.Message);
                    }
                }

                return nearest;
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            if (worker != null)
                if (worker.IsAlive)
                {
                    worker.Abort();
                }
            Bot.Log("Bot stopped.");
            Bot.Running = false;
            BotSession.CollectedDiamonds = 0;
            BotSession.CollectedElixir = 0;
            BotSession.CollectedGlows = 0;
            buttonStart.Invoke((MethodInvoker)(() =>
            {
                buttonStart.Enabled = true;
            }));
        }

        private void updateFormTimer_Tick(object sender, EventArgs e)
        {
            UpdateStats();
        }

        private void UpdateStats()
        {
            try
            {
                goldreslabel.Text = BotSession.PlayerGold;
                diamondreslabel.Text = BotSession.PlayerDiamond;
                expreslabel.Text = BotSession.PlayerExp;
                usernamelabel.Text = Account.UserName;
                useridlabel.Text = Account.ID;
                levellabel.Text = Account.Level;
                ranklabel.Text = Account.Rank;
                if (Bot.Running)
                {
                    collectedDiamonds.Text = BotSession.CollectedDiamonds.ToString();
                    gaineddiamondlabel.Text = BotSession.CollectedDiamonds.ToString();
                    gainedelixirlabel.Text = BotSession.CollectedElixir.ToString();
                    collectedGlows.Text = BotSession.CollectedGlows.ToString();
                    collectedElixir.Text = BotSession.CollectedElixir.ToString();
                    int num = Convert.ToInt32(new DateTime(DateTime.Now.Subtract(BotSession.sessionStartTime).Ticks).ToString("d "));
                    num--;
                    this.runtimeLabel.Text = num + " days " + new DateTime(DateTime.Now.Subtract(BotSession.sessionStartTime).Ticks).ToString("HH:mm:ss");
                    this.sessionStartTimeLabel.Text = BotSession.sessionStartTime.ToShortDateString() + " " + BotSession.sessionStartTime.ToLongTimeString();
                }
            }
            catch (Exception e)
            {
                Bot.Log(e.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "No payment options available yet",
                "Payment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void resetCertButton_Click(object sender, EventArgs e)
        {
            if (CertMaker.removeFiddlerGeneratedCerts(true))
            {
                Bot.Log("Unistalled Fiddler Certificates.");
            }
            MessageBox.Show("All Certificates have been removed!\nPlease restart the Bot, for the changes to take affect!", "Unistalled Certificate.", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void saveSettingsButton_Click(object sender, EventArgs e)
        {
            Client.SpeedHackValue = speedhacktrackbar.Value;
            Bot.Log("Saved settings.");
            SaveUserSettings();
        }

        private void speedhacktrackbar_Scroll(object sender, EventArgs e)
        {
            Client.SpeedHackValue = speedhacktrackbar.Value;
        }
    }
}
