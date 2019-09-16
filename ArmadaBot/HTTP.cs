using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArmadaBot
{
    public class HTTP
    {
        public static CookieContainer cookie = new CookieContainer();

        public static bool PostRequest(string url, CookieCollection reqcookies, Dictionary<string, string> getParams,
            Dictionary<string, string> postParams, ref CookieCollection respcookies, ref string html)
        {
            try
            {
                if (getParams != null)
                {
                    url += "?";
                    foreach (var pair in getParams)
                    {
                        url += pair.Key + "=" + pair.Value + "&";
                    }
                }
                url = url.Substring(0, url.Length - 1);

                var req = (HttpWebRequest)WebRequest.Create(url);
                req.CookieContainer = cookie;
                req.Method = "POST";
                req.ContentLength = 0;
                req.ContentType = "application/x-www-form-urlencoded";
                req.Referer = "https://traviant.netlify.com/";
                req.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.115 Safari/537.36";
                req.Accept = "*/*";
                req.Timeout = 15 * 1000;

                //if (proxy != null)
                //{
                //    var s = proxy.Split(':');
                //    req.Proxy = new WebProxy(s[0], int.Parse(s[1]));
                //}


                if (postParams != null)
                {
                    string post = "";
                    //post += "?";
                    foreach (var pair in postParams)
                    {
                        post += pair.Key.Replace("[", "%5B").Replace("]", "%5D") + "=" + pair.Value + "&";
                    }
                    post = post.Substring(0, post.Length - 1);
                    byte[] buffer = Encoding.ASCII.GetBytes(post);

                    req.ContentLength = buffer.Length;
                    var reqstream = req.GetRequestStream();
                    reqstream.Write(buffer, 0, buffer.Length);
                    reqstream.Close();
                }

                var resp = (HttpWebResponse)req.GetResponse();

                respcookies = resp.Cookies;

                var stream = resp.GetResponseStream();

                var reader = new StreamReader(stream);
                html = reader.ReadToEnd();

                reader.Close();
                stream.Close();
                resp.Close();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string GetRequest(string url, Dictionary<string, string> getParams, string host, bool keepAlive, string proxy)
        {
            string html = string.Empty;

            try
            {
                if (getParams != null)
                {
                    url += "?";
                    foreach (var pair in getParams)
                    {
                        url += pair.Key + "=" + pair.Value + "&";
                    }
                }
                url = url.Substring(0, url.Length - 1);

                var req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";
                req.AllowAutoRedirect = true; // redundant ...
                req.Host = host;
                req.KeepAlive = keepAlive;
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
                req.Timeout = 15 * 1000;
                if (proxy != null)
                {
                    var s = proxy.Split(':');
                    req.Proxy = new WebProxy(s[0], int.Parse(s[1]));
                }

                req.Accept = "text/html, application/xhtml+xml, image/jxr, */*";

                req.CookieContainer = cookie;

                var resp = (HttpWebResponse)req.GetResponse();
                var stream = resp.GetResponseStream();

                var reader = new StreamReader(stream);
                html = reader.ReadToEnd();

                stream.Close();
                resp.Close();

                return html;

            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    using (WebResponse response = e.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        var responseStream = response.GetResponseStream();
                        using (Stream data = responseStream)
                        {
                            string text = new StreamReader(data).ReadToEnd();
                            html = text;
                        }
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
            return html;
        }
    }
}
