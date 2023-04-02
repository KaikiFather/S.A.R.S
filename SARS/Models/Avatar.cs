using System;
using System.Collections.Generic;

namespace SARS.Models
{

    public class AvatarDetails
    {
        public int id { get; set; }
        public string avatarId { get; set; }
        public string avatarName { get; set; }
        public string avatarDescription { get; set; }
        public string authorId { get; set; }
        public string authorName { get; set; }
        public string imageUrl { get; set; }
        public string thumbnailUrl { get; set; }
        public string questAssetUrl { get; set; }
        public string pcAssetUrl { get; set; }
        public string releaseStatus { get; set; }
        public string unityVersion { get; set; }
        public DateTime recordCreated { get; set; }
    }


    public class Avatar
    {
        public AvatarDetails avatar { get; set; }
        public List<string> tags { get; set; }
    }

    public class AvatarResponse
    {
        public List<Avatar> avatars { get; set; }
        public bool authorized { get; set; }
        public bool banned { get; set; }
    }


    public class AvatarSearch
    {
        public string avatarId { get; set; }
        public string authorId { get; set; }
        public string avatarName { get; set; }
        public string authorName { get; set; }
        public List<Tag> tags { get; set; }
        public int amount { get; set; }
        public bool containsSearch { get; set; }
        public bool publicAvatars { get; set; }
        public bool privateAvatars { get; set; }
        public string key { get; set; }
        public bool debugMode { get; set; }
    }

    public class Tag
    {
        public string tag { get; set; }
    }

}