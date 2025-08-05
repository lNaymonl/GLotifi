using System.Text.Json.Serialization;

namespace GLotifi.Models;

// TODO Change string to DateTime
public class Author
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("username")]
    public string Username { get; set; }
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("state")]
    public string State { get; set; }
    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; }
    [JsonPropertyName("web_url")]
    public string WebUrl { get; set; }
}