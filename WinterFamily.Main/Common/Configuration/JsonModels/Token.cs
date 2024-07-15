using Newtonsoft.Json;
using WinterFamily.Main.Common.Attributes;

namespace WinterFamily.Main.Common.Configuration.JsonModels;

[FileName("ExternalResources/token.json")]
internal class Token : IJsonConfiguration
{
    [JsonProperty("token")]
    public string? Value { get; private set; }
}
