using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VRChatAPI.Models
{
    [Obfuscation(Exclude = true)]
    public abstract class CustomApiModel
    {
        [JsonIgnore] public static AdminOrApiWritableOnlyExcluderContractResolver Aoawoecr = new AdminOrApiWritableOnlyExcluderContractResolver();
        [JsonIgnore] public static CustomContractResolver CustomContractResolver = new CustomContractResolver();
        [JsonIgnore] public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings() { ContractResolver = Aoawoecr, NullValueHandling = NullValueHandling.Ignore };

        [JsonIgnore] public VRChatApiClient ApiClient;

        [JsonIgnore] public string Endpoint { get; set; }

        [JsonProperty("id")] public string Id { get; set; }

        public CustomApiModel(VRChatApiClient apiClient)
        {
            Endpoint = null;
            ApiClient = apiClient;
        }

        public CustomApiModel(VRChatApiClient apiClient, string endpoint)
        {
            Endpoint = endpoint;
            ApiClient = apiClient;
        }

        public string MakeRequestEndpoint(bool includeId = true)
        {
            return Endpoint + (!string.IsNullOrEmpty(Id) && includeId ? $"/{Id}" : string.Empty);
        }

        public static StringContent ToJsonContent(string json)
        {
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }

    [Obfuscation(Exclude = true)]
    public class AdminOrApiWritableOnlyExcluderContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            var propertyAttributes = property.AttributeProvider?.GetAttributes(false);
            if (propertyAttributes == null || propertyAttributes.Count == 0)
                return property;

            foreach (var propertyAttribute in propertyAttributes)
            {
                if (propertyAttribute is AdminOrApiWriteableOnly)
                    property.ShouldSerialize = _ => false;
            }

            return property;
        }
    }

    [Obfuscation(Exclude = true)]
    [AttributeUsage(AttributeTargets.Property)]
    public class AdminOrApiWriteableOnly : Attribute
    {

    }

    [Obfuscation(Exclude = true)]
    public class CustomContractResolver : DefaultContractResolver
    {

    }
}
