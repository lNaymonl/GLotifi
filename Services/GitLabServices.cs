using GLotifi.Core;
using GLotifi.Models;
using System.Text;
using System.Text.Json;

namespace GLotifi.Services
{
    public static class GitLabService
    {
        public static async Task<IEnumerable<Todo>> GetTodos()
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("PRIVATE-TOKEN", Constants.GitLabToken);

            var res = await httpClient.GetAsync($"{Constants.GitLabUrl}/api/v4/todos");
            var contentBytes = await res.Content.ReadAsByteArrayAsync();
            var contentString = Encoding.UTF8.GetString(contentBytes);

            var todos = JsonSerializer.Deserialize<Todo[]>(contentString);
            return todos ?? [];
        }

        public static async Task<IEnumerable<Todo>> GetUnannouncedTodos()
        {
            string fileContent = File.ReadAllText(Constants.TodoFilePath);
            var alreadyAnnounced = JsonSerializer.Deserialize<List<int>>(fileContent) ?? [];

            var todos = await GetTodos();
            File.WriteAllText(Constants.TodoFilePath, JsonSerializer.Serialize(todos.Select(t => t.Id)));

            return todos.Where(t => !alreadyAnnounced.Contains(t.Id));
        }
    }
}
