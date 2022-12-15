﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SARS.Models
{
    public class Config
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ApiKey { get; set; }
        public string UnityLocation { get; set; }
        public string ClientVersion { get; set; }
        public string UnityVersion { get; set; }
        public DateTime ClientVersionLastUpdated { get; set; }
        public bool LightMode { get; set; }
        public string ThemeColor { get; set; }
        public string HotSwapName { get; set; }
        public List<string> FavoriteAvatars { get; set; }
        public List<string> RippedAvatars { get; set; }
    }
}