using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VRChatAPI.Models;
using Newtonsoft.Json;
using System.Security.Policy;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Net.Mime.MediaTypeNames;

namespace VRChatAPI
{
    public class VRChatApiClient
    {
        public ObjectStore ObjectStore { get; set; }

        public CustomRemoteConfig CustomRemoteConfig { get; set; }
        public CustomApiUser CustomApiUser { get; set; }

        public HttpClient HttpClient;

        public HttpFactory HttpFactory;

        public bool DebugHttp = false;

        public string TwoFactorCode = null;


        public VRChatApiClient(int objectStoreSize = 15, string hmac = "")
        {
            ObjectStore = new ObjectStore(objectStoreSize);
            Initialize(hmac);

            CustomRemoteConfig = new CustomRemoteConfig(this);
            CustomApiUser = new CustomApiUser(this);
        }

        private void Initialize(string hmac)
        {
            StaticValues.ClientVersion = "2023.1.2p4-1290--Release";
            try
            {
                WebClient client = new WebClient();
                Stream stream = client.OpenRead("https://ares-mod.com/Version.txt");
                StreamReader reader = new StreamReader(stream);
                StaticValues.ClientVersion = reader.ReadToEnd();

            }
            catch { }

            ObjectStore["ApiUri"] = new Uri("https://api.vrchat.cloud/api/1/", UriKind.Absolute);
            ObjectStore["CookieContainer"] = new CookieContainer();
            ObjectStore["HttpClientHandler"] = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                CookieContainer = (CookieContainer)ObjectStore["CookieContainer"],
                UseProxy = false
            };
            ObjectStore["HttpClient"] = new HttpClient((HttpClientHandler)ObjectStore["HttpClientHandler"], true)
            {
                BaseAddress = (Uri)ObjectStore["ApiUri"],
                Timeout = TimeSpan.FromMinutes(90)
            };

            HttpClient = (HttpClient)ObjectStore["HttpClient"];

            ObjectStore["HttpFactory"] = new HttpFactory(HttpClient);
            HttpFactory = (HttpFactory)ObjectStore["HttpFactory"];

            StaticValues.MacAddress = hmac;
            var requestHeaders = HttpClient.DefaultRequestHeaders;
            requestHeaders.Clear();
            requestHeaders.UserAgent.ParseAdd("VRC.Core.BestHTTP");
            requestHeaders.AcceptEncoding.ParseAdd("identity");
            requestHeaders.TE.ParseAdd("identity");

            requestHeaders.Host = "api.vrchat.cloud";
            requestHeaders.Add("X-Client-Version", StaticValues.ClientVersion);
            requestHeaders.Add("X-Unity-Version", "2019.4.40f1");
            requestHeaders.Add("X-Platform", "standalonewindows");
            requestHeaders.Add("X-MacAddress", hmac);
        }

        public string GetApiKeyAsQuery()
        {
            if (ObjectStore.ContainsKey("ApiKey"))
                return $"?apiKey={(string)ObjectStore["ApiKey"]}";
            return "";
        }

        public string GetOrganizationAsAdditionalQuery()
        {
            return "&organization=vrchat";
        }

        public async Task DownloadFile(string url, string auth, string twoFactor, string fileLocation, System.Windows.Forms.ProgressBar progressBar, bool both = false)
        {
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    webClient.DownloadProgressChanged += (s, e) =>
                    {
                        if (progressBar.InvokeRequired)
                        {
                            // Call this same method but append THREAD2 to the text
                            Action safeWrite = delegate { progressBar.Value = e.ProgressPercentage; };
                            progressBar.Invoke(safeWrite);
                        }
                        else
                        {
                            progressBar.Value = e.ProgressPercentage;
                        }
                    };
                    webClient.BaseAddress = "https://api.vrchat.cloud";
                    webClient.Headers.Add("Accept", $"*/*");
                    webClient.Headers.Add("Cookie", $"auth={auth}; twoFactorAuth={twoFactor}");
                    webClient.Headers.Add("X-MacAddress", StaticValues.MacAddress);
                    webClient.Headers.Add("X-Client-Version",
                            StaticValues.ClientVersion);
                    webClient.Headers.Add("X-Platform",
                            "standalonewindows");
                    webClient.Headers.Add("user-agent",
                            "VRC.Core.BestHTTP");
                    webClient.Headers.Add("X-Unity-Version",
                            "2019.4.40f1");
                    await webClient.DownloadFileTaskAsync(new Uri(url), fileLocation);

                    // force just incase
                    if (progressBar.InvokeRequired)
                    {
                        // Call this same method but append THREAD2 to the text
                        Action safeWrite = delegate { progressBar.Value = 100; };
                        progressBar.Invoke(safeWrite);
                    }
                    else
                    {
                        progressBar.Value = 100;
                    }

                    webClient.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    if (ex.Message.Contains("403"))
                    {
                        MessageBox.Show("Error downloading Avatar, its likely that the account you are using has been banned.\nPlease login with a new account or try another avatar.");
                    }
                    else if (ex.Message.Contains("404"))
                    {
                        MessageBox.Show("Avatar has been removed from VRChat servers or this version doesn't exist");
                    }
                    else if (ex.Message.Contains("401"))
                    {
                        MessageBox.Show("Login with a alt VRChat account in the settings page");
                    }
                    else
                    {
                        MessageBox.Show(ex.Message);
                    }
                    if (progressBar.InvokeRequired)
                    {
                        // Call this same method but append THREAD2 to the text
                        Action safeWrite = delegate { progressBar.Value = 100; };
                        progressBar.Invoke(safeWrite);
                    }
                    else
                    {
                        progressBar.Value = 100;
                    }
                    webClient.Dispose();
                }
            }
        }

        public RootClass GetVersions(string url, string auth, string twoFactor)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.BaseAddress = "https://api.vrchat.cloud";
                webClient.Headers.Add("Accept", $"*/*");
                webClient.Headers.Add("Cookie", $"auth={auth}; twoFactorAuth={twoFactor}");
                webClient.Headers.Add("X-MacAddress", StaticValues.MacAddress);
                webClient.Headers.Add("X-Client-Version",
                        StaticValues.ClientVersion);
                webClient.Headers.Add("X-Platform",
                        "standalonewindows");
                webClient.Headers.Add("user-agent",
                        "VRC.Core.BestHTTP");
                webClient.Headers.Add("X-Unity-Version",
                        "2019.4.40f1");
                try
                {
                    string web = webClient.DownloadString(url);
                    RootClass items = JsonConvert.DeserializeObject<RootClass>(web);
                    return items;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return null;
                    //skip as its likely avatar is been yeeted from VRC servers
                }
            }
        }
    }
}
