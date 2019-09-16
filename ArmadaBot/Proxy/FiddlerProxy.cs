using Fiddler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArmadaBot.ArmadaBattle.Messages;
using CefSharp.Structs;
using Newtonsoft.Json;

namespace ArmadaBot.Proxy
{
    public static class FiddlerProxy
    {
        private static SessionStateHandler _beforeResponse;
        private static SessionStateHandler _beforeRequest;

        public static void Start()
        {
            FiddlerApplication.Shutdown();
            FiddlerApplication.OnValidateServerCertificate += FiddlerApplication_OnValidateServerCertificate;
            SessionStateHandler _bResp;
            if ((_bResp = _beforeResponse) == null)
            {
                _bResp = (_beforeResponse = new SessionStateHandler(BeforeResponse));
                FiddlerApplication.BeforeResponse += _bResp;
            }
            SessionStateHandler _bReq;
            if ((_bReq = _beforeRequest) == null)
            {
                _bReq = (_beforeRequest = new SessionStateHandler(BeforeRequest));
                FiddlerApplication.BeforeRequest += _bReq;
            }
            InstallCert();
            FiddlerApplication.OnWebSocketMessage += FiddlerApplication_OnWebSocketMessage;
            FiddlerCoreStartupSettings startupSettings =
                new FiddlerCoreStartupSettingsBuilder()
                    .ListenOnPort(Server.FiddlerPort)
                    .DecryptSSL()
                    .OptimizeThreadPool()
                    .Build();

            Fiddler.FiddlerApplication.Startup(startupSettings);
        }

        private static async void FiddlerApplication_OnWebSocketMessage(object sender, WebSocketMessageEventArgs e)
        {

            //await Task.Run(() => Bot.Log("WSM: " + e.oWSM.PayloadAsString()));

            #region FilterMessages

            if (e.oWSM.PayloadAsString().Contains("remove game object"))
            {
                await Task.Run(() =>
                {
                    var deletedObject = RemoveMessage.FromJson(e.oWSM.PayloadAsString().Substring(13));

                    if (Client.GameObjects == null) return;
                    if (Client.GameObjects.ContainsKey(deletedObject[1].RemoveMessageClass.GameObjectId))
                    {
                        Client.GameObjects.Remove(deletedObject[1].RemoveMessageClass.GameObjectId);
                        //Bot.Log("Obje silindi: " + deletedObject[1].RemoveMessageClass.GameObjectId);
                    }
                });
            }

            if (e.oWSM.PayloadAsString().Contains("finish move"))
            {
                await Task.Run(() =>
                {
                    BotData.InitialPosition = BotData.MovingPosition;
                    Bot.Log("changed initial position");
                    BotSession.Collecting = false;
                });
            }

            if (e.oWSM.PayloadAsString().Contains("move player coord"))
            {
                await Task.Run(() =>
                {
                    try
                    {
                        string pattern = @"([0-9\(\)]+)";
                        string input = e.oWSM.PayloadAsString();
                        var x = Regex.Matches(input, pattern)[1].Groups[1].Value;
                        var y = Regex.Matches(input, pattern)[2].Groups[1].Value;
                        if (BotData.InitialPosition == null)
                        {
                            long.TryParse(x, out long xval);
                            long.TryParse(y, out long yval);
                            BotData.InitialPosition = new BotPoint(xval, yval);
                        }
                    }
                    catch
                    {

                    }

                });
            }

            if (e.oWSM.PayloadAsString().Contains("kazan")
                || e.oWSM.PayloadAsString().Contains("reward")
                || e.oWSM.PayloadAsString().Contains("Box zebrałeś"))
            {
                await Task.Run(() =>
                {
                    try
                    {
                        string pattern = @"Box you collected. As reward ([0-9\(\)]+) pieces ([İÖÇÜa-zA-Z]+) !";
                        if (e.oWSM.PayloadAsString().Contains("kazan"))
                        {
                            pattern = @"Ödül olarak ([0-9\(\)]+) adet ([İÖÇÜa-zA-Z]+) kazandın!";
                        }
                        else if (e.oWSM.PayloadAsString().Contains("Box zebrałeś"))
                        {
                            pattern = @"Box zebrałeś. W nagrodę ([0-9\(\)]+) kawałki ([İÖÇÜa-zA-Z]+) !";
                        }
                        
                        var input = e.oWSM.PayloadAsString();
                        var mVal = Regex.Match(input, pattern).Groups[1].Value;
                        var mType = Regex.Match(input, pattern).Groups[2].Value;
                        Int32.TryParse(mVal, out int num);
                        if (mType == "Elmas" || mType == "Diamond" || mType == "Diamenty")
                        {
                            BotSession.CollectedDiamonds += num;
                            Int32.TryParse(BotSession.PlayerDiamond, out int playerdiamond);
                            playerdiamond += num;
                            BotSession.PlayerDiamond = playerdiamond.ToString();
                        }
                        else if (mType == "İksir" || mType == "Eliksir" || mType == "Elixir")
                        {
                            BotSession.CollectedElixir++;
                        }

                        //BotSession.Collecting = false;
                    }
                    catch (Exception)
                    {

                    }

                    BotSession.CollectedGlows++;
                    //BotSession.Collecting = false;
                    Bot.Log("Collected Box!");
                });
            }

            if (e.oWSM.PayloadAsString().Contains("new game object"))
            {
                await Task.Run(() =>
                {
                    try
                    {
                        var currentObject = ObjectInitMessage.FromJson(e.oWSM.PayloadAsString().Substring(13));

                        if (currentObject[1].ObjectInitMessageClass.GameObjectType.Contains("glow"))
                        {
                            var currentObjectId = currentObject[1].ObjectInitMessageClass.Id;
                            if (!Client.GameObjects.ContainsKey(currentObjectId))
                            {
                                Client.GameObjects.Add(currentObjectId, currentObject);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Bot.Log(ex.ToString());
                    }
                });
            }

            if (e.oWSM.PayloadAsString().Contains("move player coord"))
            {
                await Task.Run(() =>
                {
                    try
                    {
                        string numbers = @"([0-9\(\)]+)";
                        string input = e.oWSM.PayloadAsString();
                        var posX = Regex.Matches(input, numbers)[1].Groups[1].Value;
                        var posY = Regex.Matches(input, numbers)[2].Groups[1].Value;
                        Int32.TryParse(posX, out int x);
                        Int32.TryParse(posY, out int y);
                        BotData.MovingPosition = new BotPoint(x, y);
                        BotData.LastCollectedItem = BotData.RemoveObject;
                    }
                    catch
                    {

                    }
                });
            }

            #endregion
        }

        public static T? ToNullable<T>(this string s) where T : struct
        {
            var result = new T?();
            try
            {
                if (!string.IsNullOrEmpty(s) && s.Trim().Length > 0)
                {
                    TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
                    result = (T)conv.ConvertFrom(s);
                }
            }
            catch
            {
                // ignored
            }

            return result;
        }

        private static void InstallCert()
        {
            try
            {
                if (!CertMaker.rootCertExists() && !CertMaker.createRootCert())
                {
                    throw new Exception("Could not create Root Certificate!");
                }
                if (!CertMaker.rootCertIsTrusted() && !CertMaker.trustRootCert())
                {
                    throw new Exception("Could not find valid Root Certificate for Fiddler!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Certificate Installer Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void FiddlerApplication_OnValidateServerCertificate(object sender, ValidateServerCertificateEventArgs e)
        {
            if (e.CertificatePolicyErrors == System.Net.Security.SslPolicyErrors.None)
            {
                return;
            }
            e.ValidityState = CertificateValidity.ForceValid;
        }

        private static void BeforeResponse(Session e)
        {
            e.bBufferResponse = true;
            e.utilDecodeResponse();
            try
            {
                if (e.GetResponseBodyAsString().Contains("game.gamePaused"))
                {
                    var body = e.GetResponseBodyAsString();
                    body = body.Replace("game.gamePaused", "game.gameResumed");
                    body = body.Replace("\"pagehide\" === t.type || \"blur\" === t.type || \"pageshow\" === t.type || \"focus\" === t.type ? void(\"pagehide\" === t.type || \"blur\" === t.type ? this.game.focusLoss(t) : \"pageshow\" !== t.type && \"focus\" !== t.type || this.game.focusGain(t)) : void(this.disableVisibilityChange || (document.hidden || document.mozHidden || document.msHidden || document.webkitHidden || \"pause\" === t.type ? this.game.gamePaused(t) : this.game.gameResumed(t)))", "false");
                    e.utilSetResponseBody(body);
                }
            }
            catch (Exception ex)
            {
                Bot.Log(ex.ToString());
            }
        }

        private static void BeforeRequest(Session e)
        {
            e.bBufferResponse = true;
            if (e.RequestHeaders.ExistsAndContains("Sec-WebSocket-Extensions", "permessage-deflate"))
            {
                e.RequestHeaders.Remove("Sec-WebSocket-Extensions");
            }
        }
    }
}
