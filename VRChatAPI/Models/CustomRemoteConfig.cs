using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VRChatAPI.Models
{
    [Obfuscation(Exclude = true)]
    public class CustomRemoteConfig
    {
        #region ApiFields

        [JsonProperty("VoiceEnableDegradation")]
        public bool VoiceEnableDegradation { get; set; }

        [JsonProperty("VoiceEnableReceiverLimiting")]
        public bool VoiceEnableReceiverLimiting { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("announcements")]
        public List<Announcement> Announcements { get; set; }

        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }

        [JsonProperty("appName")]
        public string AppName { get; set; }

        [JsonProperty("buildVersionTag")]
        public string BuildVersionTag { get; set; }

        [JsonProperty("clientApiKey")]
        public string ClientApiKey { get; set; }

        [JsonProperty("clientBPSCeiling")]
        public int ClientBPSCeiling { get; set; }

        [JsonProperty("clientDisconnectTimeout")]
        public int ClientDisconnectTimeout { get; set; }

        [JsonProperty("clientReservedPlayerBPS")]
        public int ClientReservedPlayerBPS { get; set; }

        [JsonProperty("clientSentCountAllowance")]
        public int ClientSentCountAllowance { get; set; }

        [JsonProperty("contactEmail")]
        public string ContactEmail { get; set; }

        [JsonProperty("copyrightEmail")]
        public string CopyrightEmail { get; set; }

        [JsonProperty("currentTOSVersion")]
        public int CurrentTOSVersion { get; set; }

        [JsonProperty("defaultAvatar")]
        public string DefaultAvatar { get; set; }

        [JsonProperty("deploymentGroup")]
        public string DeploymentGroup { get; set; }

        [JsonProperty("devAppVersionStandalone")]
        public string DevAppVersionStandalone { get; set; }

        [JsonProperty("devDownloadLinkWindows")]
        public string DevDownloadLinkWindows { get; set; }

        [JsonProperty("devSdkUrl")]
        public string DevSdkUrl { get; set; }

        [JsonProperty("devSdkVersion")]
        public string DevSdkVersion { get; set; }

        [JsonProperty("devServerVersionStandalone")]
        public string DevServerVersionStandalone { get; set; }

        [JsonProperty("dis-countdown")]
        public DateTime DisCountdown { get; set; }

        [JsonProperty("disableAvatarCopying")]
        public bool DisableAvatarCopying { get; set; }

        [JsonProperty("disableAvatarGating")]
        public bool DisableAvatarGating { get; set; }

        [JsonProperty("disableCommunityLabs")]
        public bool DisableCommunityLabs { get; set; }

        [JsonProperty("disableCommunityLabsPromotion")]
        public bool DisableCommunityLabsPromotion { get; set; }

        [JsonProperty("disableEmail")]
        public bool DisableEmail { get; set; }

        [JsonProperty("disableEventStream")]
        public bool DisableEventStream { get; set; }

        [JsonProperty("disableFeedbackGating")]
        public bool DisableFeedbackGating { get; set; }

        [JsonProperty("disableHello")]
        public bool DisableHello { get; set; }

        [JsonProperty("disableRegistration")]
        public bool DisableRegistration { get; set; }

        [JsonProperty("disableSteamNetworking")]
        public bool DisableSteamNetworking { get; set; }

        [JsonProperty("disableTwoFactorAuth")]
        public bool DisableTwoFactorAuth { get; set; }

        [JsonProperty("disableUdon")]
        public bool DisableUdon { get; set; }

        [JsonProperty("disableUpgradeAccount")]
        public bool DisableUpgradeAccount { get; set; }

        [JsonProperty("downloadLinkWindows")]
        public string DownloadLinkWindows { get; set; }

        [JsonProperty("downloadUrls")]
        public DownloadUrls DownloadUrls { get; set; }

        [JsonProperty("dynamicWorldRows")]
        public List<DynamicWorldRow> DynamicWorldRows { get; set; }

        [JsonProperty("events")]
        public Events Events { get; set; }

        [JsonProperty("gearDemoRoomId")]
        public string GearDemoRoomId { get; set; }

        [JsonProperty("homeWorldId")]
        public string HomeWorldId { get; set; }

        [JsonProperty("homepageRedirectTarget")]
        public string HomepageRedirectTarget { get; set; }

        [JsonProperty("hubWorldId")]
        public string HubWorldId { get; set; }

        [JsonProperty("jobsEmail")]
        public string JobsEmail { get; set; }

        [JsonProperty("messageOfTheDay")]
        public string MessageOfTheDay { get; set; }

        [JsonProperty("moderationEmail")]
        public string ModerationEmail { get; set; }

        [JsonProperty("moderationQueryPeriod")]
        public int ModerationQueryPeriod { get; set; }

        [JsonProperty("notAllowedToSelectAvatarInPrivateWorldMessage")]
        public string NotAllowedToSelectAvatarInPrivateWorldMessage { get; set; }

        [JsonProperty("plugin")]
        public string Plugin { get; set; }

        [JsonProperty("releaseAppVersionStandalone")]
        public string ReleaseAppVersionStandalone { get; set; }

        [JsonProperty("releaseSdkUrl")]
        public string ReleaseSdkUrl { get; set; }

        [JsonProperty("releaseSdkVersion")]
        public string ReleaseSdkVersion { get; set; }

        [JsonProperty("releaseServerVersionStandalone")]
        public string ReleaseServerVersionStandalone { get; set; }

        [JsonProperty("sdkDeveloperFaqUrl")]
        public string SdkDeveloperFaqUrl { get; set; }

        [JsonProperty("sdkDiscordUrl")]
        public string SdkDiscordUrl { get; set; }

        [JsonProperty("sdkNotAllowedToPublishMessage")]
        public string SdkNotAllowedToPublishMessage { get; set; }

        [JsonProperty("sdkUnityVersion")]
        public string SdkUnityVersion { get; set; }

        [JsonProperty("serverName")]
        public string ServerName { get; set; }

        [JsonProperty("supportEmail")]
        public string SupportEmail { get; set; }

        [JsonProperty("timeOutWorldId")]
        public string TimeOutWorldId { get; set; }

        [JsonProperty("tutorialWorldId")]
        public string TutorialWorldId { get; set; }

        [JsonProperty("updateRateMsMaximum")]
        public int UpdateRateMsMaximum { get; set; }

        [JsonProperty("updateRateMsMinimum")]
        public int UpdateRateMsMinimum { get; set; }

        [JsonProperty("updateRateMsNormal")]
        public int UpdateRateMsNormal { get; set; }

        [JsonProperty("updateRateMsUdonManual")]
        public int UpdateRateMsUdonManual { get; set; }

        [JsonProperty("uploadAnalysisPercent")]
        public int UploadAnalysisPercent { get; set; }

        [JsonProperty("urlList")]
        public List<string> UrlList { get; set; }

        [JsonProperty("useReliableUdpForVoice")]
        public bool UseReliableUdpForVoice { get; set; }

        [JsonProperty("userUpdatePeriod")]
        public int UserUpdatePeriod { get; set; }

        [JsonProperty("userVerificationDelay")]
        public int UserVerificationDelay { get; set; }

        [JsonProperty("userVerificationRetry")]
        public int UserVerificationRetry { get; set; }

        [JsonProperty("userVerificationTimeout")]
        public int UserVerificationTimeout { get; set; }

        [JsonProperty("viveWindowsUrl")]
        public string ViveWindowsUrl { get; set; }

        [JsonProperty("whiteListedAssetUrls")]
        public List<string> WhiteListedAssetUrls { get; set; }

        [JsonProperty("worldUpdatePeriod")]
        public int WorldUpdatePeriod { get; set; }

        [JsonProperty("youtubedl-hash")]
        public string YoutubedlHash { get; set; }

        [JsonProperty("youtubedl-version")]
        public string YoutubedlVersion { get; set; }

        #endregion

        [JsonIgnore] public VRChatApiClient ApiClient;

        [JsonIgnore] public string Endpoint { get; set; }

        public CustomRemoteConfig()
        {
            Endpoint = "config";
        }

        public CustomRemoteConfig(VRChatApiClient apiClient)
        {
            Endpoint = "config";
            ApiClient = apiClient;
            ApiClient.ObjectStore["RemoteConfig"] = Get().GetAwaiter().GetResult();
            ApiClient.ObjectStore["ApiKey"] = ((CustomRemoteConfig)ApiClient.ObjectStore["RemoteConfig"]).ApiKey;
        }

        public async Task<CustomRemoteConfig> Get()
        {
            //return JsonConvert.DeserializeObject<CustomRemoteConfig>(await ApiClient.HttpFactory.GetStringAsync(MakeRequestEndpoint()).ConfigureAwait(false));
            var ret = await ApiClient.HttpFactory.GetAsync<CustomRemoteConfig>(MakeRequestEndpoint()).ConfigureAwait(false);
            ret.ApiClient = ApiClient;
            return ret;
        }

        public string MakeRequestEndpoint()
        {
            return Endpoint;
        }
    }

    [Obfuscation(Exclude = true)]
    public class Announcement
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    [Obfuscation(Exclude = true)]
    public class DownloadUrls
    {
        [JsonProperty("sdk2")]
        public string Sdk2 { get; set; }

        [JsonProperty("sdk3-worlds")]
        public string Sdk3Worlds { get; set; }

        [JsonProperty("sdk3-avatars")]
        public string Sdk3Avatars { get; set; }
    }

    [Obfuscation(Exclude = true)]
    public class DynamicWorldRow
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("sortHeading")]
        public string SortHeading { get; set; }

        [JsonProperty("sortOwnership")]
        public string SortOwnership { get; set; }

        [JsonProperty("sortOrder")]
        public string SortOrder { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }
    }

    [Obfuscation(Exclude = true)]
    public class Events
    {
        [JsonProperty("distanceClose")]
        public int DistanceClose { get; set; }

        [JsonProperty("distanceFactor")]
        public int DistanceFactor { get; set; }

        [JsonProperty("distanceFar")]
        public int DistanceFar { get; set; }

        [JsonProperty("groupDistance")]
        public int GroupDistance { get; set; }

        [JsonProperty("maximumBunchSize")]
        public int MaximumBunchSize { get; set; }

        [JsonProperty("notVisibleFactor")]
        public int NotVisibleFactor { get; set; }

        [JsonProperty("playerOrderBucketSize")]
        public int PlayerOrderBucketSize { get; set; }

        [JsonProperty("playerOrderFactor")]
        public int PlayerOrderFactor { get; set; }

        [JsonProperty("slowUpdateFactorThreshold")]
        public int SlowUpdateFactorThreshold { get; set; }

        [JsonProperty("viewSegmentLength")]
        public int ViewSegmentLength { get; set; }
    }
}
