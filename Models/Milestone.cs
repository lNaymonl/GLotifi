using System.Text.Json.Serialization;

namespace GLotifi.Models;

// TODO Change string to DateTime
public class Milestone {
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("iid")]
    public int Iid { get; set; }
    [JsonPropertyName("project_id")]
    public int ProjectId { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("state")]
    public string State { get; set; }
    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; }
    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; }
    [JsonPropertyName("due_date")]
    public string DueDate {
        get {
            throw new NotImplementedException("Didn't figure out the exact type yet.");
        }
    }
}
