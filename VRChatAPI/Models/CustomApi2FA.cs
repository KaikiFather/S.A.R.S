using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VRChatAPI.Models
{
    public class CustomApi2FA : CustomApiModel
    {
        public class CustomApi2FAContainer
        {
            [JsonProperty("code")]
            public string Code { get; set; }

            public CustomApi2FAContainer()
            {
            }

            public CustomApi2FAContainer(string code)
            {
                Code = code;
            }
        }

        [JsonProperty("requiresTwoFactorAuth")]
        public List<string> Supported2FATypes { get; set; }

        public CustomApi2FA(VRChatApiClient client)
            : base(client)
        {
        }

        public bool IsOTPSupported()
        {
            if (Supported2FATypes != null && Supported2FATypes.Count > 0)
            {
                return Supported2FATypes.Contains("otp");
            }
            return false;
        }

        public bool IsTOTPSupported()
        {
            if (Supported2FATypes != null && Supported2FATypes.Count > 0)
            {
                return Supported2FATypes.Contains("totp");
            }
            return false;
        }

        public bool IsSMSSupported()
        {
            if (Supported2FATypes != null && Supported2FATypes.Count > 0)
            {
                return Supported2FATypes.Contains("sms");
            }
            return false;
        }

        public bool IsEmailSupported()
        {
            if (Supported2FATypes != null && Supported2FATypes.Count > 0)
            {
                return Supported2FATypes.Contains("emailotp");
            }
            return false;
        }

        public string GetFirstSupported2FAType()
        {
            if (Supported2FATypes == null || Supported2FATypes.Count <= 0)
            {
                return string.Empty;
            }
            return Supported2FATypes.FirstOrDefault();
        }
    }
}