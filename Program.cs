using System.Text;
using System.Text.Json;
using DotNetEnv;
using GLotifi.Models;
using Microsoft.Toolkit.Uwp.Notifications;

namespace GLotifi;

public static class Program
{
    private static string TODO_FILE_PATH = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GLotify\\alreadyAnnounced.json");
    private static string GITLAB_URL = "";
    private static string GITLAB_TOKEN = "";
    private static int EXEC_EVERY_SEC = 30;

    public static string GetEnvVar(string name)
    {
        return Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} variable must be present in .env file");
    }

    private static DateTime LastExec = DateTime.Now;
    private static TimeSpan ExecEverySec = TimeSpan.Zero;

    public static void Main()
    {
        Env.Load(Path.Join(AppDomain.CurrentDomain.BaseDirectory, ".env"));

        var envTodoFilePath = Environment.GetEnvironmentVariable("TODO_FILE_PATH");
        if (envTodoFilePath != null) TODO_FILE_PATH = envTodoFilePath;

        GITLAB_URL = GetEnvVar("GITLAB_URL");
        GITLAB_TOKEN = GetEnvVar("GITLAB_TOKEN");
        EXEC_EVERY_SEC = int.Parse(GetEnvVar("EXEC_EVERY_SEC"));
        ExecEverySec = TimeSpan.FromSeconds(EXEC_EVERY_SEC);

        if (!File.Exists(TODO_FILE_PATH))
        {
            File.Create(TODO_FILE_PATH).Close();
            File.WriteAllText(TODO_FILE_PATH, "[]");
        }

        var notifyTask = Task.Run(() =>
        {
            while (true)
            {
                Task.Delay(100).Wait();
                var abc = DateTime.Now - LastExec;
                if (DateTime.Now - LastExec < ExecEverySec) continue;

                var task = GetUnanouncedTodos();

                task.Wait();
                var unannounced = task.Result.ToList();

                for (int i = 0; i < unannounced.Count; ++i)
                {
                    new ToastContentBuilder()
                        .AddToastActivationInfo("action=viewDetails", ToastActivationType.Foreground)
                        .AddAppLogoOverride(new Uri(Path.Join("file:///", AppDomain.CurrentDomain.BaseDirectory, "GLotifi.webp")))
                        .AddText(unannounced[i].Target.Title)
                        .AddText(unannounced[i].Target.Description)
                        .SetProtocolActivation(new Uri(unannounced[i].TargetUrl))
                        .SetToastDuration(ToastDuration.Long)
                        .Show();
                }

                LastExec = DateTime.Now;
            }
        });
        notifyTask.Wait();
    }

    public static async Task<IEnumerable<Todo>> GetTodos()
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("PRIVATE-TOKEN", GITLAB_TOKEN);
        var res = await httpClient.GetAsync($"{GITLAB_URL}/api/v4/todos");

        var content = await res.Content.ReadAsByteArrayAsync();
        var contentString = Encoding.UTF8.GetString(content);
        var todos = JsonSerializer.Deserialize<Todo[]>(contentString) ?? throw new NullReferenceException("Could not parse todos");

        return todos;
    }

    public static async Task<IEnumerable<Todo>> GetUnanouncedTodos()
    {
        string alreadyAnnouncedTodosStr = File.ReadAllText(TODO_FILE_PATH);
        List<int> alreadyAnnouncedTodos = [.. JsonSerializer.Deserialize<int[]>(alreadyAnnouncedTodosStr) ?? throw new NullReferenceException("Given file conent could not be parsed into int array!")];

        var todos = await GetTodos();

        File.WriteAllText(TODO_FILE_PATH, JsonSerializer.Serialize(todos.Select(todo => todo.Id)));

        var unannounced = todos.Where((todo) => !alreadyAnnouncedTodos.Contains(todo.Id)).ToList();

        return unannounced;
    }
}
