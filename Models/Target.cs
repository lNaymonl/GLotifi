using System.Text.Json.Serialization;

namespace GLotifi.Models;

// TODO Change string to DateTime
public class Target
{
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
    [JsonPropertyName("target_branch")]
    public string TargetBranch { get; set; }
    [JsonPropertyName("source_branch")]
    public string SourceBranch { get; set; }
    [JsonPropertyName("upvotes")]
    public int Upvotes { get; set; }
    [JsonPropertyName("downvotes")]
    public int Downvotes { get; set; }
    [JsonPropertyName("author")]
    public Author Author { get; set; }

    [JsonPropertyName("assignee")]
    public Author Assignee { get; set; }
    [JsonPropertyName("source_project_id")]
    public int SourceProjectId { get; set; }
    [JsonPropertyName("target_project_id")]
    public int TargetProjectId { get; set; }
    [JsonPropertyName("labels")]
    public string[] Labels { get; set; }
    [JsonPropertyName("draft")]
    public bool Draft { get; set; }
    [JsonPropertyName("work_in_progress")]
    public bool WorkInProgress { get; set; }
    [JsonPropertyName("merge_when_pipeline_succeeds")]
    public bool MergeWhenPipelineSucceeds { get; set; }
    [JsonPropertyName("merge_status")]
    public string MergeStatus { get; set; }
    [JsonPropertyName("subscribed")]
    public bool Subscribed { get; set; }
    [JsonPropertyName("user_notes_count")]
    public int UserNotesCount { get; set; }
}
