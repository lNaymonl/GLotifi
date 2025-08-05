using System.Text.Json.Serialization;

namespace GLotifi.Models;

// TODO Change string to DateTime
public class Project
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("name_with_namespace")]
    public string NameWithNamespace { get; set; }
    [JsonPropertyName("path")]
    public string Path { get; set; }
    [JsonPropertyName("path_with_namespace")]
    public string PathWithNamespace { get; set; }
}