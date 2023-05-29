using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRChatAPI.Models
{
    public class CustomApi2FAVerify : CustomApiModel
    {
        [JsonProperty("verified")]
        public bool Verified { get; set; }

        public CustomApi2FAVerify(VRChatApiClient client)
            : base(client)
        {
        }
    }
}
