using System.Text.Json.Serialization;

namespace GLotifi.Models;

// TODO Change string to DateTime
public class Todo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("project")]
    public Project Project { get; set; }
    [JsonPropertyName("author")]
    public Author Author { get; set; }
    [JsonPropertyName("action_name")]
    public string ActionName { get; set; }
    [JsonPropertyName("target_type")]
    public string TargetType { get; set; }

    [JsonPropertyName("target_url")]
    public string TargetUrl { get; set; }
    [JsonPropertyName("target")]
    public Target Target { get; set; }
    [JsonPropertyName("body")]
    public string Body { get; set; }
    [JsonPropertyName("state")]
    public string State { get; set; }
    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; }
    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; }
}
